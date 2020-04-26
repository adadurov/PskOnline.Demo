namespace PskOnline.Server.DAL.Inspections
{
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  public interface IInspectionCompletionEventHandler
  {
    Task<IEnumerable<PluginOutputDescriptor>> HandleInspectionCompletionAsync(Guid inspectionId, CancellationToken cancellationToken);
  }
}
