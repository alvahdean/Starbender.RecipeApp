param(
    [string]$SubscriptionId = "",
    [string]$Location = "eastus",
    [string]$ResourceGroup = "",
    [string]$ContainerAppName = "recipeapp",
    [string]$ContainerEnvName = "recipeapp-env",
    [string]$AcrName = "",
    [string]$ImageTag = "latest",
    [int]$TargetPort = 8080,
    [Parameter(Mandatory = $true)]
    [string]$SqlConnectionString,
    [switch]$EnableKeyVault,
    [string]$KeyVaultUri = "",
    [switch]$EnableEntraId,
    [switch]$UseLocalImage,
    [string]$LocalImageName = "",
    [string]$ManagedIdentityName = "recipe-app",
    [string]$ManagedIdentityResourceGroup = "",
    [switch]$SkipResourceGroupCreate,
    [switch]$SkipEnvironmentProvisioning
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $true
$env:AZURE_CORE_ONLY_SHOW_ERRORS = "True"

function Require-Command {
    param([string]$CommandName)
    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "Required command '$CommandName' is not installed or not on PATH."
    }
}

function New-AcrName {
    param([string]$BaseName)

    $normalized = ($BaseName.ToLower() -replace "[^a-z0-9]", "")
    if ($normalized.Length -lt 5) {
        $normalized = "recipeapp"
    }

    $suffix = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char]$_ })
    $combined = "$normalized$suffix"

    if ($combined.Length -gt 50) {
        return $combined.Substring(0, 50)
    }

    return $combined
}

Require-Command "az"

if ($UseLocalImage) {
    Require-Command "docker"
}

Write-Host "Checking Azure login..."
az account show 1>$null

if (-not [string]::IsNullOrWhiteSpace($SubscriptionId)) {
    Write-Host "Setting subscription to '$SubscriptionId'..."
    az account set --subscription $SubscriptionId
}

if ($SkipResourceGroupCreate) {
    Write-Host "Using existing resource group '$ResourceGroup' (skipping create)."
}
else {
    Write-Host "Ensuring resource group '$ResourceGroup' in '$Location'..."
    az group create --name $ResourceGroup --location $Location 1>$null
}

Write-Host "Ensuring Container Apps extension..."
az extension add --name containerapp --upgrade --allow-preview true 1>$null 2>$null

$envExists = az containerapp env show --name $ContainerEnvName --resource-group $ResourceGroup --query name -o tsv 2>$null
if ($SkipEnvironmentProvisioning) {
    if ([string]::IsNullOrWhiteSpace($envExists)) {
        throw "Container Apps environment '$ContainerEnvName' was not found in resource group '$ResourceGroup' and -SkipEnvironmentProvisioning was set."
    }
    Write-Host "Using existing Container Apps environment '$ContainerEnvName' (skipping workspace/environment create)."
}
elseif ([string]::IsNullOrWhiteSpace($envExists)) {
    $workspaceName = "$ContainerEnvName-law"
    Write-Host "Ensuring Log Analytics workspace '$workspaceName'..."
    az monitor log-analytics workspace create `
        --resource-group $ResourceGroup `
        --workspace-name $workspaceName `
        --location $Location 1>$null

    $workspaceCustomerId = az monitor log-analytics workspace show `
        --resource-group $ResourceGroup `
        --workspace-name $workspaceName `
        --query customerId -o tsv

    $workspaceSharedKey = az monitor log-analytics workspace get-shared-keys `
        --resource-group $ResourceGroup `
        --workspace-name $workspaceName `
        --query primarySharedKey -o tsv

    Write-Host "Creating Container Apps environment '$ContainerEnvName'..."
    az containerapp env create `
        --name $ContainerEnvName `
        --resource-group $ResourceGroup `
        --location $Location `
        --logs-workspace-id $workspaceCustomerId `
        --logs-workspace-key $workspaceSharedKey 1>$null
}
else {
    Write-Host "Using existing Container Apps environment '$ContainerEnvName'."
}

if ([string]::IsNullOrWhiteSpace($AcrName)) {
    $AcrName = New-AcrName -BaseName $ContainerAppName
}

if ([string]::IsNullOrWhiteSpace($ManagedIdentityResourceGroup)) {
    $ManagedIdentityResourceGroup = $ResourceGroup
}

Write-Host "Ensuring user-assigned managed identity '$ManagedIdentityName'..."
$managedIdentityId = az identity show `
    --name $ManagedIdentityName `
    --resource-group $ManagedIdentityResourceGroup `
    --query id -o tsv 2>$null

if ([string]::IsNullOrWhiteSpace($managedIdentityId)) {
    $managedIdentityId = az identity create `
        --name $ManagedIdentityName `
        --resource-group $ManagedIdentityResourceGroup `
        --location $Location `
        --query id -o tsv
}

$managedIdentityClientId = az identity show `
    --name $ManagedIdentityName `
    --resource-group $ManagedIdentityResourceGroup `
    --query clientId -o tsv

Write-Host "Ensuring Azure Container Registry '$AcrName'..."
$acrExists = az acr show --name $AcrName --resource-group $ResourceGroup --query name -o tsv 2>$null
if ([string]::IsNullOrWhiteSpace($acrExists)) {
    az acr create --name $AcrName --resource-group $ResourceGroup --location $Location --sku Basic 1>$null
}

$acrLoginServer = az acr show --name $AcrName --resource-group $ResourceGroup --query loginServer -o tsv
$imageRef = "$acrLoginServer/$ContainerAppName`:$ImageTag"

if ($UseLocalImage) {
    if ([string]::IsNullOrWhiteSpace($LocalImageName)) {
        throw "-LocalImageName is required when -UseLocalImage is specified."
    }

    Write-Host "Tagging local image '$LocalImageName' as '$imageRef'..."
    docker tag $LocalImageName $imageRef

    Write-Host "Logging in to ACR '$AcrName'..."
    az acr login --name $AcrName 1>$null

    Write-Host "Pushing image '$imageRef'..."
    docker push $imageRef
}
else {
    Write-Host "Building and pushing image '$imageRef' with ACR build..."
    az acr build `
        --registry $AcrName `
        --image "$ContainerAppName`:$ImageTag" `
        --file "src/Starbender.RecipeApp/Dockerfile" `
        "src" 1>$null
}

$acrUsername = az acr credential show --name $AcrName --query username -o tsv
$acrPassword = az acr credential show --name $AcrName --query "passwords[0].value" -o tsv

$keyVaultEnabledValue = if ($EnableKeyVault) { "true" } else { "false" }
$entraEnabledValue = if ($EnableEntraId) { "true" } else { "false" }

$envVars = @(
    "ASPNETCORE_ENVIRONMENT=Production"
    "ASPNETCORE_HTTP_PORTS=$TargetPort"
    "KeyVault__Enabled=$keyVaultEnabledValue"
    "Authentication__EntraId__Enabled=$entraEnabledValue"
    "AZURE_CLIENT_ID=$managedIdentityClientId"
    "ConnectionStrings__Default=secretref:sql-default-conn"
)

if ($EnableKeyVault -and -not [string]::IsNullOrWhiteSpace($KeyVaultUri)) {
    $envVars += "KeyVault__VaultUri=$KeyVaultUri"
}

Write-Host "Deploying Container App '$ContainerAppName'..."
$existingAppCount = az containerapp list `
    --resource-group $ResourceGroup `
    --query "[?name=='$ContainerAppName'] | length(@)" -o tsv

if ($existingAppCount -eq "0") {
    az containerapp create `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --environment $ContainerEnvName `
        --user-assigned $managedIdentityId `
        --image $imageRef `
        --ingress external `
        --target-port $TargetPort `
        --registry-server $acrLoginServer `
        --registry-username $acrUsername `
        --registry-password $acrPassword `
        --secrets "sql-default-conn=$SqlConnectionString" `
        --env-vars $envVars `
        --min-replicas 0 `
        --max-replicas 1 `
        --cpu 0.25 `
        --memory 0.5Gi 1>$null
}
else {
    az containerapp identity assign `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --user-assigned $managedIdentityId 1>$null

    az containerapp registry set `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --server $acrLoginServer `
        --username $acrUsername `
        --password $acrPassword 1>$null

    az containerapp secret set `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --secrets "sql-default-conn=$SqlConnectionString" 1>$null

    az containerapp update `
        --name $ContainerAppName `
        --resource-group $ResourceGroup `
        --image $imageRef `
        --set-env-vars $envVars `
        --min-replicas 0 `
        --max-replicas 1 `
        --cpu 0.25 `
        --memory 0.5Gi 1>$null
}

$fqdn = az containerapp show --name $ContainerAppName --resource-group $ResourceGroup --query properties.configuration.ingress.fqdn -o tsv
Write-Host ""
Write-Host "Deployment complete."
Write-Host "Image: $imageRef"
Write-Host "URL: https://$fqdn"
