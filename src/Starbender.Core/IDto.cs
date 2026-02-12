namespace Starbender.Core;

/// <summary>
/// Interface tag for identifying objects intended as 
/// publicly exposed data transfer objects (DTO)
/// </summary>
public interface IDto { }

/// <summary>
/// Interface tag for identifying objects intended as 
/// publicly exposed data transfer objects (DTO)
/// which also have an Id property of type TKey
/// </summary>
public interface IDto<TKey> : IDto, IHasId<TKey> { }
