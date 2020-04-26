namespace PskOnline.Server.Shared.Plugins
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public interface IInspectionResultsHandler
  {
    /// <summary>
    /// Returns the ID of the method set that the plugin can handle
    /// </summary>
    /// <returns></returns>
    string GetMethodSetId();

    /// <summary>
    /// returns a that identifies the plugin
    /// </summary>
    /// <returns></returns>
    string GetPluginSlug();

    /// <summary>
    /// Processes the results of the specified inspection and returns the URL
    /// that may be used to HTTP GET the results
    /// </summary>
    /// <param name="inspectionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> ProcessInspectionResultsAsync(Guid inspectionId, CancellationToken cancellationToken);
  }
}
