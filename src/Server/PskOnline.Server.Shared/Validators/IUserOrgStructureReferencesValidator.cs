using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PskOnline.Server.Shared.Validators
{
  public interface IUserOrgStructureReferencesValidator
  {
    Task ValidateOrgStructureReferences(Guid id, string userName, Guid? branchOfficeId, Guid? departmentId, Guid? positionId);
  }
}
