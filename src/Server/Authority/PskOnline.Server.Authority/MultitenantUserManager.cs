namespace PskOnline.Server.Authority
{
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using PskOnline.Server.Authority.Interfaces;
  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.Multitenancy;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class MultitenantUserManager<TUser> : UserManager<TUser> where TUser: ApplicationUser
  {
    private readonly IUserRepository _userRepository;
    private readonly ITenantService _tenantService;
    private readonly ITenantSlugProvider _tenantSlugProvider;

    public MultitenantUserManager(
        IUserStore<TUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<TUser> passwordHasher,
        IEnumerable<IUserValidator<TUser>> userValidators,
        IEnumerable<IPasswordValidator<TUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<TUser>> logger,
        ITenantSlugProvider tenantSlugProvider,
        ITenantService tenantService,
        IUserRepository userRepository)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
      _tenantSlugProvider = tenantSlugProvider;
      _tenantService = tenantService;
      _userRepository = userRepository;
    }

    public override Task<IdentityResult> CreateAsync(TUser user, string password)
    {
      return base.CreateAsync(user, password);
    }

    public override async Task<TUser> FindByNameAsync(string userName)
    {
      ThrowIfDisposed();

      var slug = _tenantSlugProvider.GetSlug();
      var tenantId = await _tenantService.GetTenantIdBySlug(slug);

      return await FindByNameInTenantAsync(userName, tenantId);
    }

    public override async Task<TUser> FindByEmailAsync(string email)
    {
      ThrowIfDisposed();
      if (email == null)
      {
        throw new ArgumentNullException(nameof(email));
      }

      var slug = _tenantSlugProvider.GetSlug();
      var tenantId = await _tenantService.GetTenantIdBySlug(slug);

      return await FindByEmailInTenantAsync(email, tenantId);
    }

    public async Task<TUser> FindByNameInTenantAsync(string userName, Guid tenantId)
    {
      ThrowIfDisposed();
      if (userName == null)
      {
        throw new ArgumentNullException(nameof(userName));
      }

      var user = await _userRepository
        .Query()
        .SingleOrDefaultAsync(u => u.UserName == userName && u.TenantId == tenantId, CancellationToken);

      if (user == null)
      {
        return null;
      }

      return await Store.FindByIdAsync(user.Id.ToString(), CancellationToken);
    }

    public async Task<TUser> FindByEmailInTenantAsync(string email, Guid tenantId)
    {
      ThrowIfDisposed();
      if (email == null)
      {
        throw new ArgumentNullException(nameof(email));
      }

      var user = await _userRepository
        .Query()
        .SingleOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId, CancellationToken);

      if (user == null)
      {
        return null;
      }

      return await Store.FindByIdAsync(user.Id.ToString(), CancellationToken);
    }
  }
}
