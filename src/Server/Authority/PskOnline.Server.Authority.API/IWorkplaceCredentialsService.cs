namespace PskOnline.Server.Authority.API
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  using PskOnline.Server.Authority.API.Dto;

  public interface IWorkplaceCredentialsService
  {
    /// <summary>
    /// Registers the 'workplace' application using the <para>workplaceDescriptor</para>
    /// and returns the client credentials (id and secret) that must be used to authenticate
    /// a client as the workspace.
    /// </summary>
    /// <param name="workplaceDescriptor"></param>
    /// <remarks>
    /// Authentication is then done via /connect/token endpoint using 'client credentials' workflow.
    /// Upon authentication, Authority will issue an access token based on the scopes created
    /// during registration of the workplace.
    /// </remarks>
    Task<WorkplaceCredentialsDto> CreateWorkplaceAsync(WorkplaceDescriptorDto workplaceDescriptor);

    /// <summary>
    /// Creates a new secret for the specified workplace
    /// </summary>
    /// <returns>A newly generated random secret</returns>
    Task<string> UpdateWorkplaceSecretAsync(string clientId);

    /// <summary>
    /// Returns a collection of workplaces for the specified department
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<WorkplaceDto>> GetForDepartmentAsync(Guid departmentId);

    Task DeleteAsync(string clientId);
  }
}
