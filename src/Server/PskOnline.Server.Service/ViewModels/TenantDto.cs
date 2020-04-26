namespace PskOnline.Server.Service.ViewModels
{
  using FluentValidation;
  using System;
  using System.ComponentModel.DataAnnotations;

  public class TenantDto
  {
    public string Id { get; set; }

    public string Name { get; set; }

    public string Comment { get; set; }

    [Required(
      ErrorMessage = "Slug is required",
      AllowEmptyStrings = false)]
    [RegularExpression(
      pattern:"^[a-z0-9_-]+$",
      ErrorMessage = "Only lower-case latin letters, numbers, dash or underscore are allowed")]
    public string Slug { get; set; }

    [Required(ErrorMessage = "Primary contact information is required")]
    public ContactInfoDto PrimaryContact { get; set; }

    [Required(ErrorMessage = "Service details information is required")]
    public ServiceDetailsDto ServiceDetails { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime DateModified { get; set; }
  }


  public class TenantViewModelValidator : AbstractValidator<TenantDto>
  {
    public TenantViewModelValidator()
    {
      RuleFor(register => register.Name).NotEmpty().WithMessage("Tenant name cannot be empty");
    }
  }
}
