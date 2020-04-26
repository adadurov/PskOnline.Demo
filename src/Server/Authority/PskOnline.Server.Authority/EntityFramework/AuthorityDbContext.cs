namespace PskOnline.Server.Authority.EntityFramework
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Threading;

  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore;

  using PskOnline.Server.Authority.ObjectModel;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Authority.Interfaces;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.EntityFrameworkCore.Infrastructure;
  using Microsoft.Extensions.Options;
  using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

  /// <summary>
  /// </summary>
  public class AuthorityDbContext : 
    IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IUserRepository, IUserRoleRepository
  {
    public string CurrentUserId { get; set; }

    public AuthorityDbContext(DbContextOptions<AuthorityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      //      base.OnModelCreating(builder);
      IdentityDbContext_OnModelCreating<
        ApplicationUser, Guid, ApplicationRole, IdentityUserRole<Guid>,
        IdentityUserClaim<Guid>, IdentityRoleClaim<Guid>, IdentityUserLogin<Guid>, IdentityUserToken<Guid>
        >(builder);

      builder.UseOpenIddict<PskApplication, PskAuthorization, PskScope, PskToken, string>();

      builder.ApplyConfiguration<ApplicationUser>(new IdentityEntityConfig());
      builder.ApplyConfiguration<ApplicationRole>(new IdentityEntityConfig());
      builder.ApplyConfiguration(new PskApplicationEntityConfig());
    }

    #region ASP.Net Core Identity rewritten members

    protected void IdentityDbContext_OnModelCreating<TUser, TKey, TRole, TUserRole, TUserClaim, TRoleClaim, TUserLogin, TUserToken>(ModelBuilder builder)
        where TUser : ApplicationUser
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
      IdentityUserContext_OnModelCreating<TUser, TKey, TUserToken, TUserClaim, TUserLogin>(builder);

      builder.Entity<TUser>(b =>
      {
        b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
      });

      builder.Entity<TRole>(b =>
      {
        b.HasKey(r => r.Id);
        b.HasIndex(r => r.NormalizedName).HasName("RoleNameIndex").IsUnique();
        b.ToTable("AspNetRoles");
        b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();

        b.Property(u => u.Name).HasMaxLength(256);
        b.Property(u => u.NormalizedName).HasMaxLength(256);

        b.HasMany<TUserRole>().WithOne().HasForeignKey(ur => ur.RoleId).IsRequired();
        b.HasMany<TRoleClaim>().WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();
      });

      builder.Entity<TRoleClaim>(b =>
      {
        b.HasKey(rc => rc.Id);
        b.ToTable("AspNetRoleClaims");
      });

      builder.Entity<TUserRole>(b =>
      {
        b.HasKey(r => new { r.UserId, r.RoleId });
        b.ToTable("AspNetUserRoles");
      });
    }

    protected void IdentityUserContext_OnModelCreating<TUser, TKey, TUserToken, TUserClaim, TUserLogin>(ModelBuilder builder)
        where TUser : ApplicationUser
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
      var storeOptions = GetStoreOptions();
      var maxKeyLength = storeOptions?.MaxLengthForKeys ?? 0;
      var encryptPersonalData = storeOptions?.ProtectPersonalData ?? false;
      PersonalDataConverter converter = null;

      builder.Entity<TUser>(b =>
      {
        b.HasKey(u => u.Id);
        b.HasIndex(u => new {u.NormalizedUserName, u.TenantId } ).HasName("UserNameTenantIndex").IsUnique();
        b.HasIndex(u => new { u.NormalizedEmail, u.TenantId } ).HasName("EmailTenantIndex");
        b.ToTable("AspNetUsers");
        b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();

        b.Property(u => u.UserName).HasMaxLength(256);
        b.Property(u => u.NormalizedUserName).HasMaxLength(256);
        b.Property(u => u.Email).HasMaxLength(256);
        b.Property(u => u.NormalizedEmail).HasMaxLength(256);

        if (encryptPersonalData)
        {
          converter = new PersonalDataConverter(this.GetService<IPersonalDataProtector>());
          var personalDataProps = typeof(TUser).GetProperties().Where(
                          prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
          foreach (var p in personalDataProps)
          {
            if (p.PropertyType != typeof(string))
            {
              throw new InvalidOperationException("Protection is supported only on string fields");
            }
            b.Property(typeof(string), p.Name).HasConversion(converter);
          }
        }

        b.HasMany<TUserClaim>().WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
        b.HasMany<TUserLogin>().WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
        b.HasMany<TUserToken>().WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
      });

      builder.Entity<TUserClaim>(b =>
      {
        b.HasKey(uc => uc.Id);
        b.ToTable("AspNetUserClaims");
      });

      builder.Entity<TUserLogin>(b =>
      {
        b.HasKey(l => new { l.LoginProvider, l.ProviderKey });

        if (maxKeyLength > 0)
        {
          b.Property(l => l.LoginProvider).HasMaxLength(maxKeyLength);
          b.Property(l => l.ProviderKey).HasMaxLength(maxKeyLength);
        }

        b.ToTable("AspNetUserLogins");
      });

      builder.Entity<TUserToken>(b =>
      {
        b.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

        if (maxKeyLength > 0)
        {
          b.Property(t => t.LoginProvider).HasMaxLength(maxKeyLength);
          b.Property(t => t.Name).HasMaxLength(maxKeyLength);
        }

        if (encryptPersonalData)
        {
          var tokenProps = typeof(TUserToken).GetProperties().Where(
                          prop => Attribute.IsDefined(prop, typeof(ProtectedPersonalDataAttribute)));
          foreach (var p in tokenProps)
          {
            if (p.PropertyType != typeof(string))
            {
              throw new InvalidOperationException("Protection is supported only on string fields");
            }
            b.Property(typeof(string), p.Name).HasConversion(converter);
          }
        }

        b.ToTable("AspNetUserTokens");
      });
    }

    private StoreOptions GetStoreOptions() => this.GetService<IDbContextOptions>()
                        .Extensions.OfType<CoreOptionsExtension>()
                        .FirstOrDefault()?.ApplicationServiceProvider
                        ?.GetService<IOptions<IdentityOptions>>()
                        ?.Value?.Stores;

    private class PersonalDataConverter : ValueConverter<string, string>
    {
      public PersonalDataConverter(IPersonalDataProtector protector)
        : base(s => protector.Protect(s), s => protector.Unprotect(s), null)
      { }
    }


    #endregion



    public override int SaveChanges()
    {
      UpdateAuditEntities();
      return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
      UpdateAuditEntities();
      return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
      UpdateAuditEntities();
      return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
    {
      UpdateAuditEntities();
      return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateAuditEntities()
    {
      var modifiedEntries = ChangeTracker.Entries()
          .Where(x => x.Entity is IAuditableEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

      foreach (var entry in modifiedEntries)
      {
        var entity = (IAuditableEntity)entry.Entity;
        DateTime now = DateTime.UtcNow;

        if (entry.State == EntityState.Added)
        {
          entity.CreatedDate = now;
          entity.CreatedBy = CurrentUserId;
        }
        else
        {
          base.Entry(entity).Property(x => x.CreatedBy).IsModified = false;
          base.Entry(entity).Property(x => x.CreatedDate).IsModified = false;
          entity.UpdatedDate = now;
          entity.UpdatedBy = CurrentUserId;
        }
      }
    }

    public DbSet<PskApplication> WorkplaceApplications { get; set; }

    IQueryable<IdentityUserRole<Guid>> IUserRoleRepository.Query()
    {
      return UserRoles.AsQueryable();
    }

    IQueryable<ApplicationUser> IUserRepository.Query()
    {
      return Users.AsQueryable();
    }

    /// <summary>
    /// Migrate the database to current version
    /// </summary>
    public void MigrateToCurrentVersion()
    {
      Database.Migrate();
    }
  }
}
