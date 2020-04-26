namespace PskOnline.Server.DAL
{
  using System;

  using System.Threading.Tasks;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Repository;

  /// <summary>
  /// Validates that referenced entity exist and belong to the same tenant as the referring entity
  /// </summary>
  public static class ReferenceValidatorForTenant
  {
    public static async Task ValidateRequiredEntityReference<TParent, TChild>(
      TParent referringEntity, IGuidKeyedRepository<TChild> referenceRepository, Guid childId)
      where TParent : IGuidIdentity, ITenantEntity where TChild : class, IGuidIdentity, ITenantEntity
    {
      if (childId == Guid.Empty)
      {
        throw GetMissingRequiredReferenceException<TParent, TChild>(referringEntity, childId);
      }
      await ValidateOptionalEntityReference(referringEntity, referenceRepository, childId);
    }

    public static async Task ValidateOptionalEntityReference<TParent, TChild>(
      TParent referringEntity, IGuidKeyedRepository<TChild> referenceRepository, Guid childId)
      where TParent : IGuidIdentity, ITenantEntity where TChild : class, IGuidIdentity, ITenantEntity
    {
      if (childId == Guid.Empty)
      {
        return;
      }

      var child = await referenceRepository.GetAsync(childId);
      if( child == null || child.TenantId != referringEntity.TenantId )
      {
        throw GetModelValidationException<TParent, TChild>(referringEntity, childId);
      }
      //catch (UnauthorizedAccessException ex)
      //{
      //  throw GetModelValidationException<TParent, TChild>(referringEntity, childId, ex);
      //}
      //catch (ItemNotFoundException ex)
      //{
      //  throw GetModelValidationException<TParent, TChild>(referringEntity, childId, ex);
      //}
    }

    private static BadRequestException GetModelValidationException<TParent, TChild>(
      TParent referringEntity, Guid childId)
      where TParent : IGuidIdentity where TChild : IGuidIdentity
    {
      var message = 
        $"{typeof(TParent).Name} (id='{referringEntity.Id}') " + 
        $"refers to an invalid {typeof(TChild).Name} (child Id='{childId}')";
      return BadRequestException.BadReference(message);
    }

    private static BadRequestException GetMissingRequiredReferenceException<TParent, TChild>(
      TParent referringEntity, Guid childId)
      where TParent : IGuidIdentity where TChild : IGuidIdentity
    {
      var message =
        $"{typeof(TParent).Name} (id='{referringEntity.Id}') " + 
        $"is missing a mandatory reference to {typeof(TChild).Name}.";
      return BadRequestException.BadReference(message);
    }

  }
}
