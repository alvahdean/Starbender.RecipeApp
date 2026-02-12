namespace Starbender.Core;

public interface IHasId : IHasId<int> { }

public interface IHasId<T>
{
    T Id { get; }
}
