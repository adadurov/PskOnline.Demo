namespace PskOnline.Server.DAL
{
  using System;
  using System.Threading.Tasks;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.ObjectModel;
  using PskOnline.Server.Shared.Service;
  using PskOnline.Server.Shared.Validators;

  public class UserOrgStructureReferencesValidator : IUserOrgStructureReferencesValidator
  {
    private readonly IService<Department> _departmentService;
    private readonly IService<BranchOffice> _branchOfficeService;
    private readonly IService<Position> _positionService;

    public UserOrgStructureReferencesValidator(
      IService<Department> departmentService,
      IService<BranchOffice> branchOfficeService,
      IService<Position> positionService)
    {
      _departmentService = departmentService;
      _branchOfficeService = branchOfficeService;
      _positionService = positionService;
    }

    public async Task ValidateOrgStructureReferences(Guid userId, string userName, Guid? branchOfficeId, Guid? departmentId, Guid? positionId)
    {
      await ValidateEntityReference(userId, userName, _branchOfficeService, branchOfficeId);
      await ValidateEntityReference(userId, userName, _departmentService, departmentId);
      await ValidateEntityReference(userId, userName, _positionService, positionId);

      if (departmentId.HasValue && branchOfficeId.HasValue)
      {
        var department = await _departmentService.GetAsync(departmentId.Value);
        if (department.BranchOfficeId != branchOfficeId.Value)
        {
          var msg = $"The branch office that the users belongs to must be the same as the branch office that the user's department belongs to.";
          throw new BadRequestException(msg);
        }
      }
    }

    private async Task ValidateEntityReference<T>(Guid userId, string userName, IService<T> service, Guid? entityId) where T : class, ITenantEntity, IGuidIdentity
    {
      if (!entityId.HasValue)
      {
        return;
      }
      try
      {
        await service.GetAsync(entityId.Value);
        return;
      }
      catch (UnauthorizedAccessException ex)
      {
        throw BuildBadReferenceException<T>(userId, userName, entityId.Value, ex);
      }
      catch (ItemNotFoundException ex)
      {
        throw BuildBadReferenceException<T>(userId, userName, entityId.Value, ex);
      }
    }

    private static BadRequestException BuildBadReferenceException<T>(Guid userId, string userName, Guid entityId, Exception ex)
    {
      var message =
        $"Invalid reference to {typeof(T).Name} with Id='{entityId}' " +
        $"(from user UserName='{userName}', Id='{userId}'): {ex.Message}";
      return BadRequestException.BadReference(message, ex);
    }

  }
}
