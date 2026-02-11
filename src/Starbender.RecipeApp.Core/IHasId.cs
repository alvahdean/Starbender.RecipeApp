namespace Starbender.RecipeApp.Core;

public interface IHasId : IHasId<int> { }

public interface IHasId<T>
{
    T Id { get; }
}
