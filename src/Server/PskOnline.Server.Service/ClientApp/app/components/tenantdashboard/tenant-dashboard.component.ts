
import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { fadeInOut } from '../../services/animations';
import { TenantService } from '../../services/tenant.service';
import { TenantOperationsSummary } from '../../models/tenant-operations-summary.model';
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { Utilities } from '../../services/utilities';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'tenant-dashboard',
    templateUrl: './tenant-dashboard.component.html',
    styleUrls: ['./tenant-dashboard.component.css'],
    animations: [fadeInOut]
})
export class TenantDashboardComponent implements OnInit {

  tenantOperationsSummary: TenantOperationsSummary = new TenantOperationsSummary();

  loadingIndicator: boolean;

  constructor(private tenantService: TenantService,
              private authService: AuthService,
              private alertService: AlertService) {
  }

  ngOnInit(): void {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;
    var tenantId = this.authService.lastLoggedInUser.tenantId;
    this.tenantService.getTenantOperationsSummary(tenantId).subscribe(
      result => this.onDataLoadSuccessful(result),
      error => this.onDataLoadFailed(error)
    );
  }

  onDataLoadSuccessful(tenantOperationsSummary: TenantOperationsSummary) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.tenantOperationsSummary = tenantOperationsSummary;
  }

  onDataLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.alertService.showStickyMessage(
      "Ошибка загрузки",
      `Не удалось загрузить данные организации.\r\nОшибки: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);
  }

}
