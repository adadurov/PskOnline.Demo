namespace PskOnline.Service.Test
{
  using System;
  using System.Linq.Expressions;
  using System.Threading.Tasks;

  using NSubstitute;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Repository;

  public static class IRepositoryExtensions
  {
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mockRepo"></param>
    /// <param name="entity">the entity that the repository should contain</param>
    /// <remarks>
    ///   pass null as an entity to mock an empty repository
    /// </remarks>
    public static void SetupRepositoryWithEntity<T>(this IRepository<T, Guid> mockRepo, T entity) where T : class, IGuidIdentity
    {
      if (entity != null)
      {
        mockRepo.Query().Returns(new[] { entity }.AsEntityAsyncQueryable());
        mockRepo.Get(entity.Id).Returns(entity);
        mockRepo.GetAsync(entity.Id).Returns(Task.FromResult(entity));
        
        // if the expression evaluates to true, return the entity
        mockRepo.GetSingleOrDefaultAsync(
          Arg.Is<Expression<Func<T, bool>>>(e => e.Compile()(entity)))
          .Returns(entity);
        // if the expression evaluates to true, return the entity
        mockRepo.GetSingleOrDefaultAsync(
          Arg.Is<Expression<Func<T, bool>>>(e => e.Compile()(entity)))
          .Returns(Task.FromResult(entity));
      }
      else
      {
        mockRepo.Query().Returns(new T[] { }.AsEntityAsyncQueryable());
        mockRepo.Get(Arg.Any<Guid>()).Returns(default(T));
        mockRepo.GetAsync(Arg.Any<Guid>()).Returns(Task.FromResult(default(T)));
        mockRepo.GetSingleOrDefault(Arg.Any<Expression<Func<T, bool>>>()).Returns(default(T));
        mockRepo.GetSingleOrDefaultAsync(Arg.Any<Expression<Func<T, bool>>>()).Returns(Task.FromResult(default(T)));
      }

    }
  }
}
