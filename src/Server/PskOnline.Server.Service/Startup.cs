namespace PskOnline.Server.Service
{
  using System;
  using System.Data.SqlClient;
  using System.IO;
  using System.Reflection;

  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Rewrite;
  using Microsoft.AspNetCore.SpaServices.Webpack;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Logging.Console;

  using Swashbuckle.AspNetCore.Swagger;

  using PskOnline.Server.DAL;
  using PskOnline.Server.DAL.OrgStructure;
  using PskOnline.Server.DAL.Inspections;
  using PskOnline.Server.DAL.Multitenancy;
  using PskOnline.Server.DAL.OrgStructure.Interfaces;
  using PskOnline.Server.DAL.Time;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Service.Helpers;
  using PskOnline.Server.Service.Middleware;
  using PskOnline.Server.Service.Multitenant;
  using PskOnline.Server.Shared.Contracts.Service;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Repository;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Validators;

  public class Startup
  {
    static readonly LoggerFactory MyLoggerFactory
        = new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) });

    public IConfiguration Configuration { get; }

    private readonly ILoggerFactory _loggerFactory;

    public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
      _loggerFactory = loggerFactory;
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      ConfigureLogger(_loggerFactory);
      var logger = _loggerFactory.CreateLogger<Startup>();

      var appExtensionsLoader = new ServiceExtensionsLoader(
        Configuration, _loggerFactory);

      // Add cors
      services.AddCors();


      // Add framework services.
      services.AddMvc()
      .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      // load application parts from non-referenced 'extension' assemblies
      // starting with Authority plugin that configures authentication and authorization
      appExtensionsLoader.AddServices(services);

      // FIXME: upgrade to Swagger 2.5 broke authentication
      // Not resolved within 2 hours -- rolled back the NuGet package upgrade
      // Many reports about similar issues in Swashbuckle git. Typical solutions didn't help.
      // Need to dig deeper, so putting into Technical Debt for Production
      services.AddSwaggerGen(options =>
      {
        options.AddSecurityDefinition("OpenID Connect", new OAuth2Scheme
        {
          Type = "oauth2",
          Flow = "password",
          TokenUrl = "/connect/token"
        });
        options.SwaggerDoc("v1", new Info { Title = "Psk.Online API", Version = "v1" });

        var xmlMainServicePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        options.IncludeXmlComments(xmlMainServicePath);
        var xmlAuthorityDocs = Path.Combine(AppContext.BaseDirectory, "PskOnline.Server.Authority.xml");
        options.IncludeXmlComments(xmlAuthorityDocs);
        var xmlAuthorityApiDocs = Path.Combine(AppContext.BaseDirectory, "PskOnline.Server.Authority.API.xml");
        options.IncludeXmlComments(xmlAuthorityApiDocs);
      });

      ConfigureStaticAssets();

      // Configurations
      services.Configure<SmtpConfig>(Configuration.GetSection("SmtpConfig"));

      // Application Services
      services.AddScoped<IEmailer, Emailer>();
      services.AddScoped<IEmailService, MailGunEmailService>();

      string connectionString = GetConnectionString(logger);

      services.AddDbContext<ApplicationDbContext>(options =>
      {
        //options.UseLazyLoadingProxies();
        options.UseSqlServer(
          connectionString,
          b => {
            b.MigrationsAssembly("PskOnline.Server.DAL");
            b.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
              );
          });
        //        options.UseLoggerFactory(MyLoggerFactory);
      });

      services.AddSingleton<ITimeService, TimeService>();

      // Tenant Context
      services.AddScoped<IAccessScopeFilter, TenantScopeFilter>();
      services.AddScoped<ITenantIdProvider, TenantIdProvider>();
      services.AddScoped<ITenantSlugProvider, TenantSlugProvider>();

      services.AddScoped<ITenantAccessChecker, TenantAccessChecker>();
      services.AddScoped<ITenantEntityAccessChecker, TenantEntityAccessChecker>();

      services.AddScoped<IUserOrgStructureReferencesValidator, UserOrgStructureReferencesValidator>();

      // Repositories
      services.AddScoped<IGuidKeyedRepository<Tenant>, TenantRepository>();
      services.AddScoped<IGuidKeyedRepository<BranchOffice>, Repository<BranchOffice, ApplicationDbContext>>();
      services.AddScoped<IGuidKeyedRepository<Department>, Repository<Department, ApplicationDbContext>>();
      services.AddScoped<IGuidKeyedRepository<Position>, Repository<Position, ApplicationDbContext>>();
      services.AddScoped<IGuidKeyedRepository<Employee>, Repository<Employee, ApplicationDbContext>>();
      services.AddScoped<IGuidKeyedRepository<Inspection>, Repository<Inspection, ApplicationDbContext>>();
      services.AddScoped<IGuidKeyedRepository<Test>, Repository<Test, ApplicationDbContext>>();

      // Domain Services
      services.AddScoped<IService<Tenant>, TenantService>();
      services.AddScoped<ITenantService, TenantService>();
      services.AddScoped<IService<BranchOffice>, BranchOfficeService>();
      services.AddScoped<IService<Department>, DepartmentService>();
      services.AddScoped<IService<Position>, PositionService>();
      services.AddScoped<IEmployeeService, EmployeeService>();

      services.AddScoped<ITenantCreator, TenantCreator>();

      services.AddScoped<IInspectionService, InspectionService>();
      services.AddScoped<ITestService, TestService>();

      services.AddScoped<IInspectionCompletionEventHandler, InspectionCompletionHandler>();

      // DB Creation and Seeding
      services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();
    }

    private string GetConnectionString(ILogger<Startup> logger)
    {
      var builder = new SqlConnectionStringBuilder(Configuration["ConnectionStrings:DefaultConnection"]);
      string password = GetDbPasswordFromSecrets();
      if (!string.IsNullOrEmpty(password))
      {
        // if a user secret is defined, use it instead of the password
        // that may be present in the connection string
        builder.Password = password;
        logger.LogInformation("Using DbPassword from secrets");
      }
      else
      {
        logger.LogWarning("Using database password from connection string in app settings!");
      }
      logger.LogInformation($"Using MSSQL server at {builder.DataSource}.");
      return builder.ConnectionString;
    }

    private string GetDbPasswordFromSecrets()
    {
      try
      {

        return Configuration["DbPassword"];
      }
      catch
      {
        return null;
      }
    }

    private void ConfigureStaticAssets()
    {
      StaticStartup.Startup();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      EmailTemplates.Initialize(env);

      var startupLogger = loggerFactory.CreateLogger<Startup>();
      startupLogger.LogInformation($"Using environment: {env.EnvironmentName}");

      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
        app.UseMiddleware<ExceptionToHttpStatusConverter>();
        app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
        {
          HotModuleReplacement = true
        });
        // for developers' convenience, redirect visitors from / to /admin
        var option = new RewriteOptions();
        option.AddRedirect("^$", "/admin/");
        app.UseRewriter(option);
      }
      else
      {
        app.UseMiddleware<ExceptionToHttpStatusConverter>();
      }

      app.UseCors();
      app.UseAuthentication();

      app.UseStaticFiles();
      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PskOnline API V1");
      });

      app.UseMvcWithDefaultRoute();

      // fall back to serving the admin SPA
      // in case the request path starts with /admin
      app.MapWhen(x => x.Request.Path.Value.StartsWith("/admin"), builder =>
      {
        builder.UseMvc(routes =>
        {
          routes.MapSpaFallbackRoute(
              name: "spa-fallback",
              defaults: new { controller = "Home", action = "Index" });
        });
      });

    }

    private void ConfigureLogger(ILoggerFactory loggerFactory)
    {
      loggerFactory.AddLog4Net();

      loggerFactory.AddDebug(LogLevel.Warning);
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddFile(Configuration.GetSection("Logging"));

      Utilities.ConfigureLogger(loggerFactory);
    }
  }
}
