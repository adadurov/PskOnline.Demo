namespace PskOnline.Server.Authority
{
  using System;

  using AutoMapper;
  using Microsoft.AspNetCore.Identity;

  using PskOnline.Server.Authority.API.Dto;

  /// <summary>
  /// Coverts IdentityRoleClaim&lt;Guid&gt; to PermissionDto using IClaimsService.
  /// 
  /// Usage example:
  /// 
  /// Mapper.Map&lt;RoleDto&gt;(
  ///   role, 
  ///   options =&gt; options.ConstructServicesUsing(
  ///     t =&gt; new ClaimValueToPermissionDtoConverter(_claimsService)));
  /// </summary>
  public sealed class ClaimValueToPermissionDtoConverter : ITypeConverter<IdentityRoleClaim<Guid>, PermissionDto>
  {
    private readonly IClaimsService _claimsService;

    public ClaimValueToPermissionDtoConverter(IClaimsService claimsService)
    {
      _claimsService = claimsService;
    }

    public PermissionDto Convert(IdentityRoleClaim<Guid> source, PermissionDto destination, ResolutionContext context)
    {
      return Mapper.Map<PermissionDto>(_claimsService.GetPermissionByValue(source.ClaimValue));
    }

  }
}