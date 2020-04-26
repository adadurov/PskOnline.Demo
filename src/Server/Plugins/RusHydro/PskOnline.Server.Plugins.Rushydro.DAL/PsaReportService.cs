namespace PskOnline.Server.Plugins.RusHydro.DAL
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using PskOnline.Server.ObjectModel;
  using PskOnline.Server.Plugins.RusHydro.Logic;
  using PskOnline.Server.Plugins.RusHydro.ObjectModel;
  using PskOnline.Server.Shared.Exceptions;
  using PskOnline.Server.Shared.Multitenancy;
  using PskOnline.Server.Shared.Service;

  /// <summary>
  /// later on, we may consider adding Microsoft.Extensions.Caching.Memory.IMemoryCache;
  /// </summary>
  public class PsaReportService : IDepartmentPsaReportService
  {
    private static log4net.ILog _log = log4net.LogManager.GetLogger(typeof(PsaReportService));

    private readonly IPsaSummaryService _summaryService;
    private readonly ITenantEntityAccessChecker _tenantEntityAccessChecker;
    private readonly IPsaReportRepository _reportRepository;
    private readonly IOrgStructureReference _orgStructReference;
    private readonly ITimeService _timeService;
    private readonly Guid _tenantId;

    public PsaReportService(
      IPsaSummaryService summaryService,
      IOrgStructureReference orgStructReference,
      //IPsaReportRepository reportRepository,
      ITenantEntityAccessChecker tenantEntityAccessChecker,
      ITenantIdProvider tenantIdProvider,
      ITimeService timeService
      )
    {
      _summaryService = summaryService;
//      _reportRepository = reportRepository;
      _orgStructReference = orgStructReference;
      _tenantEntityAccessChecker = tenantEntityAccessChecker;
      _tenantId = tenantIdProvider.GetTenantId();
      _timeService = timeService;
    }

    public async Task<PsaReportDocument> GetCurrentShiftReportAsync(Guid departmentId)
    {
      // чтобы создать сводку по нужной смене, нужно понять:
      // какой отдел и филиал интересует пользователя?
      var org = await GetOrgStructureItemsAsync(departmentId);
      var dept = org.Item1;
      var branch = org.Item2;

      // какое сейчас местное время в филиале?
      var currentTimeInDepartment = GetCurrentLocalTime(branch);
      // какой смене соответствует данное время?
      var absoluteShiftIndex = WorkingShiftAbsoluteIndex.GetAbsoluteIndex(currentTimeInDepartment);
      // выбрать из репозитория сводок все сводки с указанным absoluteShiftIndex и отделом
      var shiftSummaries = await _summaryService.GetSummariesInDepartmentForShift(departmentId, absoluteShiftIndex);

      // составить из них отчет
      var descriptor = GetShiftDescriptor(currentTimeInDepartment);
      var reportDoc = new PsaReportDocument
      {
        BranchOfficeId = (dept?.BranchOfficeId).Value,
        BranchOfficeName = branch?.Name,
        DepartmentId = departmentId,
        DepartmentName = dept?.Name,
        ShiftDate = descriptor.ShiftDate,
        ShiftName = descriptor.ShiftName,
        ShiftNumber = descriptor.ShiftNumber,
        ShiftStartTime = descriptor.ShiftStartTime,
        Summaries = shiftSummaries
      };

      // fetch summaries for the shift in the department
      return reportDoc;
    }

    /// <summary>
    /// returns a tuple of a department and its parent branch office
    /// throws ItemNotFoundException if any of the two is not found
    /// throws UnauthorizedAccessException if any of the two don't belong to the proper tenant
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    private async Task<Tuple<Department, BranchOffice>> GetOrgStructureItemsAsync(Guid departmentId)
    {
      var dept = await _orgStructReference.GetDepartmentAsync(departmentId);
      if (dept == null)
      {
        throw ItemNotFoundException.NotFoundByKey("Id", departmentId.ToString(), nameof(Department)); ;
      }
      await _tenantEntityAccessChecker.ValidateAccessToEntityAsync(dept, Shared.Permissions.EntityAction.Read);
      var branch = await _orgStructReference.GetBranchOfficeAsync(dept.BranchOfficeId);
      if (branch == null)
      {
        throw ItemNotFoundException.NotFoundByKey("Id", dept.BranchOfficeId.ToString(), nameof(BranchOffice)); ;
      }
      await _tenantEntityAccessChecker.ValidateAccessToEntityAsync(branch, Shared.Permissions.EntityAction.Read);
      return Tuple.Create(dept, branch);
    }

    private DateTime GetCurrentLocalTime(BranchOffice branch)
    {
      var serverTime =  DateTime.UtcNow;
      var prepLocalTime = new DateTime(
        serverTime.Year, serverTime.Month, serverTime.Day,
        serverTime.Hour, serverTime.Minute, serverTime.Second, DateTimeKind.Unspecified);

      var timeZone = _timeService.GetTimeZone(branch.TimeZoneId);

      return prepLocalTime + timeZone.BaseUtcOffset;
    }

    private WorkingShiftDescriptor GetShiftDescriptor(DateTime completionLocalTime)
    {
      var absIndex = WorkingShiftAbsoluteIndex.GetAbsoluteIndex(completionLocalTime);
      var shiftDate = RusHydroScheduler.GetShiftStartDate(completionLocalTime, out bool _, out int shiftNumber);
      var shiftStart = RusHydroScheduler.GetShiftStartTime(completionLocalTime, out bool _, out int _);

      return new WorkingShiftDescriptor
      {
        ShiftAbsoluteIndex = absIndex,
        ShiftDate = shiftDate,
        ShiftStartTime = shiftStart.TimeOfDay,
        ShiftNumber = shiftNumber,
        // watch out, some localizable strings here!
        ShiftName = shiftNumber == 1 ? "день" : "ночь"
      };
    }

    public async Task AddSummaryAsync(SummaryDocument summary)
    {
      return;
      // чтобы найти сводку по нужной смене, нужно понять:
      // код отдела
      //
      // Абсолютный Индекс Смены (АИС), к которой относится данная сводка.
      // АИС состоит из десятичной записи даты, к которой относится данная смена
      // и номера смены
      var shiftDescriptor = GetShiftDescriptor(summary.CompletionTime.LocalDateTime);
      var count = 0;
      do
      {
        // получить экземпляр сводки за эту смену из хранилища
        var report = await GetOrInitReportAsync(summary, shiftDescriptor);

        // внести изменения

        try
        {
          // положить обновленный отчет в хранилище
          _reportRepository.Update(report);
          await _reportRepository.SaveChangesAsync();
          return;
        }
        catch( Exception ex )
        {
          // Видимо если эта сводка уже кем-то модифицируется.
          // BUG: изучить вопрос ConcurrencyToken и т.п.
          await Task.Delay(new Random().Next(50));
        }

        // если не получилось, попробовать еще раз
        // но только если сбой вызван проблемами конкурентного обновления
        // иначе зависнем
        // если за 10 попыток не смогли, вернуть 409 / Conflict
      }
      while (count++ < 10);
    }

    private async Task<Report> GetOrInitReportAsync(SummaryDocument summary, WorkingShiftDescriptor descriptor)
    {
      {
        var existingReport = await GetExistingReport(summary.DepartmentId, descriptor.ShiftAbsoluteIndex);
        if (existingReport != null)
        {
          // report found, return it
          return existingReport;
        }
      }

      var branch = await _orgStructReference.GetBranchOfficeAsync(summary.BranchOfficeId);
      var dept = await _orgStructReference.GetDepartmentAsync(summary.DepartmentId);

      var reportDoc = new PsaReportDocument
      {
        BranchOfficeId = summary.BranchOfficeId,
        BranchOfficeName = branch?.Name,
        DepartmentId = summary.DepartmentId,
        DepartmentName = dept?.Name,
        ShiftDate = descriptor.ShiftDate,
        ShiftName = descriptor.ShiftName,
        ShiftNumber = descriptor.ShiftNumber,
        ShiftStartTime = descriptor.ShiftStartTime,
        Summaries = new List<SummaryDocument>()
      };

      // create report, as it doesn't exist yet (the summary is the first for the shift)
      var newReport = new Report
      {
        DepartmentId = reportDoc.DepartmentId,
        BranchOfficeId = reportDoc.BranchOfficeId,
        ReportDocument = reportDoc,
      };

      try
      {
        // save the report
        _reportRepository.Add(newReport);
        await _reportRepository.SaveChangesAsync();
      }
      catch( Exception ex )
      {

      }
      // if failed to save, try to get the report again
      {
        var existingReport = await GetExistingReport(summary.DepartmentId, descriptor.ShiftAbsoluteIndex);
        if (existingReport != null)
        {
          // report found, return it
          return existingReport;
        }
      }
      throw new Exception("Cannot retrieve the existing report for update");
    }

    private async Task<Report> GetExistingReport(Guid departmentId, long shiftAbsIndex)
    {
      return await _reportRepository.GetSingleOrDefaultAsync(
        r => r.TenantId == _tenantId &&
             r.DepartmentId == departmentId &&
             r.ShiftAbsoluteIndex == shiftAbsIndex
      );
    }
  }
}
