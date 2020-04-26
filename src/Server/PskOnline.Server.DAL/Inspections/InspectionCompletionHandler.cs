namespace PskOnline.Server.DAL.Inspections
{
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Plugins;
  using PskOnline.Server.Shared.Repository;
  using System;
  using System.Collections.Generic;
  using System.Threading;
  using System.Threading.Tasks;

  public class InspectionCompletionHandler : IInspectionCompletionEventHandler
  {
    private readonly IServiceProvider _services;
    private readonly IGuidKeyedRepository<Inspection> _inspectionRepo;
    private readonly ILogger _logger;

    public InspectionCompletionHandler(IServiceProvider services,
                                       IGuidKeyedRepository<Inspection> inspectionRepo,
                                       ILogger<InspectionCompletionHandler> logger
                  )
    {
      _inspectionRepo = inspectionRepo;
      _services = services;
      _logger = logger;
    }

    public async Task<IEnumerable<PluginOutputDescriptor>> HandleInspectionCompletionAsync(Guid inspectionId, CancellationToken cancellationToken)
    {
      var plugins = FindAllPlugins();
      var results = new List<PluginOutputDescriptor>();
      var inspection = await GetInspection(inspectionId);
      foreach( var plugin in plugins )
      {
        try
        {
          if (plugin.GetMethodSetId() == inspection.MethodSetId)
          {
            var url = await plugin.ProcessInspectionResultsAsync(inspectionId, cancellationToken);
            results.Add(new PluginOutputDescriptor {
              PluginType = plugin.GetPluginSlug(),
              ResultsUrl = url
            });
          }
        }
        catch( Exception ex )
        {
          _logger.LogError(ex, $"Error processing inspection using plugin '{plugin.GetType().FullName}'");
        }
      }
      return results;
    }

    private async Task<Inspection> GetInspection(Guid inspectionId)
    {
      try
      {
        return await _inspectionRepo.GetAsync(inspectionId);
      }
      catch( ItemNotFoundException )
      {
        _logger.LogWarning($"The inspection with ID {inspectionId} was not found.");
        return null;
      }
    }

    private IEnumerable<IInspectionResultsHandler> FindAllPlugins()
    {
      try
      {
        return _services.GetServices<IInspectionResultsHandler>();
      }
      catch( Exception ex )
      {
        _logger.LogError(ex, "Error activating inspection result handling plugins");
        return new IInspectionResultsHandler[0];
      }
    }
  }
}
