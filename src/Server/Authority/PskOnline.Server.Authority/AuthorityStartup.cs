namespace PskOnline.Server.Authority
{
  using System;
  using System.Data.SqlClient;
  using AspNet.Security.OpenIdConnect.Primitives;

  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using PskOnline.Server.Authority.API.Constants;
  using PskOnline.Server.Authority.EntityFramework;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Authority.Roles;
  using PskOnline.Server.Shared.EFCore;
  using PskOnline.Server.Shared.Plugins;

  public class AuthorityStartup : IServerPlugin
  {
    public void AddServices(
      IConfiguration Configuration,
      IServiceCollection services,
      ILoggerFactory loggerFactory)
    {
      var logger = loggerFactory.CreateLogger<AuthorityStartup>();

      string connectionString = GetConnectionString(Configuration, logger);

      services.AddDbContext<AuthorityDbContext>(options =>
      {
        options.UseSqlServer(
          connectionString,
          b => {
            b.MigrationsAssembly("PskOnline.Server.Authority");
            b.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
              );
          });
        options.UseOpenIddict<PskApplication, PskAuthorization, PskScope, PskToken, string>();
        // options.UseLazyLoadingProxies();
        // options.UseLoggerFactory(MyLoggerFactory);
      });

      // add identity
      // per ASP.NET team, custom validators must come before 'AddIdentity' call
      // https://github.com/aspnet/Identity/issues/1112
      services.AddScoped<IRoleValidator<ApplicationRole>, MulititenantRoleValidator>();
      services.AddScoped<IUserValidator<ApplicationUser>, MultitenantUserValidator<ApplicationUser>>();
      services.AddScoped<IUserRepository, AuthorityDbContext>();
      services.AddScoped<IUserRoleRepository, AuthorityDbContext>();

      services.AddIdentity<ApplicationUser, ApplicationRole>()
          .AddEntityFrameworkStores<AuthorityDbContext>()
          .AddUserManager<MultitenantUserManager<ApplicationUser>>()
          .AddDefaultTokenProviders();

      // Configure Identity options and password complexity here
      services.Configure<IdentityOptions>(options =>
      {
        // User settings
        options.User.RequireUniqueEmail = false;

        options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
        options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
        options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
      });

      services.AddOpenIddict()
        .AddCore(options =>
        {
          options.UseEntityFrameworkCore()
                 .UseDbContext<AuthorityDbContext>()
                 .ReplaceDefaultEntities<PskApplication, PskAuthorization, PskScope, PskToken, string>();
        })
        .AddServer(options =>
        {
          options.UseMvc();

          options.UseReferenceTokens();

          options.EnableTokenEndpoint("/connect/token");
                 //.EnableLogoutEndpoint("/connect/logout")
                 //.EnableUserinfoEndpoint("/api/account/users/me");

          options.AllowPasswordFlow()
                 .AllowRefreshTokenFlow()
                 .AllowClientCredentialsFlow();

          options.RegisterScopes(
                                 OpenIdConnectConstants.Scopes.OpenId,
                                 OpenIdConnectConstants.Scopes.Email,
                                 OpenIdConnectConstants.Scopes.Profile,
                                 OpenIdConnectConstants.Scopes.Phone,
                                 OpenIdConnectConstants.Scopes.Profile,
                                 OpenIdConnectConstants.Scopes.OfflineAccess,
                                 "roles",
                                 PskOnlineScopes.DeptAuditorWorkplace,
                                 PskOnlineScopes.DeptOperatorWorkplace,
                                 PskOnlineScopes.BranchAuditorWorkplace,
                                 PskOnlineScopes.TenantAuditorWorkplace);

          options.AcceptAnonymousClients();

          // In production, HTTPS is handled by nginx
          options.DisableHttpsRequirement();
        })
        .AddValidation(options => options.UseReferenceTokens());

      services.AddPskOnlineAuthorization();

      services.AddAuthentication(options =>
      {
        // this should be after OpenIddict configs; for details, see
        // https://github.com/openiddict/openiddict-core/issues/594
        // if at some point you need a more stable solution, see
        // https://stackoverflow.com/questions/46464469/how-to-configureservices-authentication-based-on-routes-in-asp-net-core-2-0/46475072#46475072
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
      });

      services.AddScoped<IRestrictedAccountManager, RestrictedAccountManager>();
      services.AddScoped<IAccountManager, UnrestrictedAccountManager>();
      services.AddScoped<IClaimsService, ClaimsService>();
      services.AddScoped<IRestrictedRoleService, RestrictedRoleService>();
      services.AddScoped<IRoleService, UnrestrictedRoleService>();
      services.AddScoped<API.IRoleService, UnrestrictedRoleService>();
      services.AddScoped<API.IAccountService, UnrestrictedAccountManager>();
      services.AddScoped<API.IWorkplaceCredentialsService, WorkplaceCredentialsService>();

      services.AddSingleton<AutoMapperConfig>();

      services.AddTransient<IDatabaseInitializer, AuthorityDbInitializer>();

    }

    private string GetConnectionString(IConfiguration configuration, ILogger<AuthorityStartup> logger)
    {
      var builder = new SqlConnectionStringBuilder(configuration["ConnectionStrings:DefaultConnection"]);
      string password = GetDbPasswordFromSecrets(configuration);
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

    private string GetDbPasswordFromSecrets(IConfiguration Configuration)
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

  }
}
