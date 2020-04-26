namespace PskOnline.Server.DAL.Inspections
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using PskOnline.Server.ObjectModel;

  public interface ITestService
  {
    Task<Guid> AddAsync(Test test);

    Task<IEnumerable<Test>> GetAllAsync(int? skip, int? take);

    Task<Test> GetAsync(Guid id);

    Task RemoveAsync(Guid id);
  }

}
