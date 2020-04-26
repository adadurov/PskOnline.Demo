namespace PskOnline.Server.Shared.Contracts.Service
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;
  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.ObjectModel;

  public interface IInspectionService
  {
    Task<Guid> BeginInspectionAsync(Inspection inspection);

    Task<IEnumerable<PluginOutputDescriptor>> CompleteInspectionAsync(Guid inspectionId, DateTimeOffset completionTime, CancellationToken cancellationToken);

    Task<IEnumerable<Inspection>> GetAllAsync(int? skip, int? take);

    Task<Inspection> GetAsync(Guid id);

    Task RemoveAsync(Guid id);

    /// <summary>
    /// returns the number of inspections completed AFTER the specified timestamp
    /// </summary>
    /// <param name="timestampe"></param>
    /// <returns></returns>
    Task<long> GetInspectionCountSinceAsync(Guid tenantId, DateTimeOffset timestamp);

    Task<Inspection> GetInspectionWithTestsAsync(Guid inspectionId);
  }
}
