namespace Starbender.Core;

/// <summary>
/// Interface tag for identifying objects intended as EF entities
/// </summary>
public interface IEntity { }

/// <summary>
/// Interface tag for identifying objects intended as EF entities 
/// which also have an Id property of type TKey
/// </summary>
public interface IEntity<TKey> : IEntity, IHasId<TKey> { }
