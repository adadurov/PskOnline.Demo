namespace PskOnline.Server.Service
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  using Microsoft.AspNetCore.Mvc.ApplicationParts;

  using PskOnline.Server.Shared.Plugins;

  public class ServiceExtensionsLoader
  {
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _log;

    public ServiceExtensionsLoader(
      IConfiguration configuration, ILoggerFactory loggerFactory)
    {
      _log = loggerFactory.CreateLogger<ServiceExtensionsLoader>();
      _configuration = configuration;
      _loggerFactory = loggerFactory;
    }

    public void AddServices(IServiceCollection serviceCollection)
    {
      DoForEachExtensionAssembly(assembly =>
        {
          var pluginCollection = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Contains(typeof(IServerPlugin)));
           
          foreach ( var pluginType in pluginCollection )
          {
            try
            {
              _log.LogInformation($"Activating plugin: {pluginType.Name}");
              var serverPlugin = Activator.CreateInstance(pluginType) as IServerPlugin;
              _log.LogInformation($"Activated plugin: {pluginType.Name}");
              if (serverPlugin != null)
              {
                _log.LogInformation($"Loading services from plugin: {pluginType.Name}");
                serverPlugin?.AddServices(
                  _configuration, serviceCollection, _loggerFactory);
                _log.LogInformation($"Loaded services from plugin: {pluginType.Name}");
              }
            }
            catch (Exception e)
            {
              _log.LogError(e, $"Could not instantiate {pluginType.FullName}");
              throw;
            }
          }
        },
        null
      );
    }

    private void DoForEachExtensionAssembly(Action<Assembly> assemblyAction, Action<Assembly, Exception> handleException)
    {
      var pluginAssemblies = _configuration["ServerPlugins"];
      var assembly = Assembly.GetExecutingAssembly();
      var searchDir = System.IO.Path.GetDirectoryName(assembly.Location);

      string oldCurDir = Environment.CurrentDirectory;

      try
      {
        Environment.CurrentDirectory = searchDir;
        if (pluginAssemblies != null)
        {
          foreach (var name in pluginAssemblies.Split(';', StringSplitOptions.RemoveEmptyEntries))
          {
            try
            {
              var loadName = System.IO.Path.GetFileNameWithoutExtension(name);

              _log.LogInformation($"Loading assembly {loadName}.");
              var assemblyWithPlugin = Assembly.Load(loadName);

              try
              {
                assemblyAction?.Invoke(assemblyWithPlugin);
              }
              catch (Exception e)
              {
                if (handleException != null)
                {
                  try
                  {
                    handleException(assemblyWithPlugin, e);
                  }
                  catch( Exception ex )
                  {
                    _log.LogError(ex, "Error in user exception handler");
                  }
                }
                else
                {
                  _log.LogError(e, "Error in user action handling the extension assembly " + assembly.FullName );
                }
                throw;
              }
            }
            catch (Exception ex)
            {
              _log.LogError(ex,
                $"Can't load assembly from: {name} (searched in {Environment.CurrentDirectory})");
            }
          }
        }
      }
      finally
      {
        // restore current directory
        Environment.CurrentDirectory = oldCurDir;
      }
    }



    public void LoadIntoApplicationParts(IList<ApplicationPart> applicationParts)
    {
      DoForEachExtensionAssembly(assemblyWithPlugin =>
        {
          var appPart = new AssemblyPart(assemblyWithPlugin);
          applicationParts.Add(appPart);
          _log.LogInformation($"Created application part from {assemblyWithPlugin.FullName}.");
        },
        (assembly, exception) =>
        {
          _log.LogError(exception, "Could not create assembly part from " + assembly.FullName);
        });
    }
  }
}
