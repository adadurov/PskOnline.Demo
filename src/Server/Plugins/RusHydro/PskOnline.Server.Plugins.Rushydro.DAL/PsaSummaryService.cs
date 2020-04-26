namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using Microsoft.EntityFrameworkCore;

  public class PsaSummaryService : IPsaSummaryService
  {
    private readonly IPsaSummaryRepository _psaSummaryRepository;
    private readonly ITenantEntityAccessChecker _tenantEntityAccessChecker;
    private readonly ITenantIdProvider _tenantIdProvider;

    public PsaSummaryService(
      IPsaSummaryRepository psaSummaryRepository,
      ITenantEntityAccessChecker tenantEntityAccessChecker,
      ITenantIdProvider tenantIdProvider)
    {
      _psaSummaryRepository = psaSummaryRepository;
      _tenantEntityAccessChecker = tenantEntityAccessChecker;
      _tenantIdProvider = tenantIdProvider;
    }

    private Task<Summary> TryGetSummaryEntityForInspectionAsync(Guid inspectionId)
    {
      return _psaSummaryRepository.GetSingleOrDefaultAsync(
        s => s.TenantId == _tenantIdProvider.GetTenantId() && s.InspectionId == inspectionId);
    }

    public async Task<SummaryDocument> GetSummaryForInspectionAsync(Guid inspectionId)
    {
      var summary = await TryGetSummaryEntityForInspectionAsync(inspectionId);
      if( summary == null)
      {
        throw ItemNotFoundException.NotFoundByKey(nameof(Summary.InspectionId), inspectionId.ToString(), nameof(Summary));
      }
      await _tenantEntityAccessChecker.ValidateAccessToEntityAsync(summary, Shared.Permissions.EntityAction.Read);
      return summary.SummaryDocument;
    }

    public IEnumerable<SummaryDocument> GetSummariesInDepartmentForPeriod(
      Guid departmentId,
      DateTimeOffset completedAfter,
      DateTimeOffset completedNotLaterThan)
    {
      return _psaSummaryRepository.GetSummariesInDepartmentForPeriod(
        _tenantIdProvider.GetTenantId(), departmentId, completedAfter, completedNotLaterThan).
        Select(s => s.SummaryDocument);
    }

    public async Task<Guid> AddAsync(SummaryDocument doc)
    {
      var existing = await TryGetSummaryEntityForInspectionAsync(doc.InspectionId);
      if (existing != null)
      {
        throw ItemAlreadyExistsException.MatchingEntityExists("RusHydroPsaSummary", existing.Id);
      }
      if (doc.Id == Guid.Empty)
      {
        doc.Id = Guid.NewGuid();
      }
      var shiftAbsIndex = WorkingShiftAbsoluteIndex.GetAbsoluteIndex(doc.CompletionTime.LocalDateTime);
      var deptReportId = DepartmentShiftReportIdBuilder.BuildReportId(doc.DepartmentId, shiftAbsIndex);
      var summary = new Summary
      {
        Id = doc.Id,
        DepartmentShiftReportId = deptReportId,
        ShiftAbsoluteIndex = shiftAbsIndex,
        BranchOfficeId = doc.BranchOfficeId,
        CompletionTime = doc.CompletionTime,
        DepartmentId = doc.DepartmentId,
        EmployeeId = doc.Employee.Id,
        InspectionId = doc.InspectionId,
        SummaryDocument = doc,
        TenantId = _tenantIdProvider.GetTenantId(),
        UpdateDate = doc.UpdateDate
      };

      _psaSummaryRepository.Add(summary);
      await _psaSummaryRepository.SaveChangesAsync();
      return summary.Id;
    }

    public Task<List<SummaryDocument>> GetSummariesInDepartmentForShift(Guid departmentId, long absoluteShiftIndex)
    {
      var depShiftReportId = DepartmentShiftReportIdBuilder.BuildReportId(departmentId, absoluteShiftIndex);

      return _psaSummaryRepository.Query()
        .Where(s => s.DepartmentShiftReportId == depShiftReportId)
        .Select(s => s.SummaryDocument).ToListAsync();
    }
  }
}
