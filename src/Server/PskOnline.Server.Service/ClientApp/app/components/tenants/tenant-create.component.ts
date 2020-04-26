import { Component, OnInit, ViewChild } from '@angular/core';

import { TenantCreate } from '../../models/tenant-create.model';
import { TenantService } from "../../services/tenant.service";
import { AlertService, MessageSeverity } from '../../services/alert.service';
import { CreatedWithGuid } from '../../models/created-with-guid.model';

@Component({
  selector: 'tenant-create',
  templateUrl: './tenant-create.component.html',
  styleUrls: ['./tenant-create.component.css']
})
export class TenantCreateComponent implements OnInit {

  private isSaving = false;
  private showValidationErrors = false;
  private tenant: TenantCreate = new TenantCreate();
  private tenantNew: TenantCreate = new TenantCreate();

  constructor(
    private alertService: AlertService,
    private tenantService: TenantService) {

  }

  ngOnInit() {
  }

  public formResetToggle = true;

  public changesSavedCallback: (Tenant) => void;
  public changesFailedCallback: () => void;
  public changesCancelledCallback: () => void;

  @ViewChild('f')
  private form;

  //ViewChilds Required because ngIf hides template variables from global scope
  @ViewChild('name')
  private name;

  @ViewChild('slug')
  private slug;

  @ViewChild('primaryContactName')
  private primaryContactName;

  @ViewChild('primaryContactEmail')
  private primaryContactEmail;

  @ViewChild('tenantAdminEmail')
  private tenantAdminEmail;

  private save() {
    this.isSaving = true;
    this.alertService.startDelayedMessage("Saving changes...");

      this.tenantService.newTenant(this.tenantNew).subscribe(
        tenant => this.saveSuccessHelper(tenant), error => this.saveFailedHelper(error));
  }

  private saveSuccessHelper(createdTenant?: CreatedWithGuid) {

    if (createdTenant)
      this.tenantNew.tenantDetails.id = createdTenant.id;

    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.showValidationErrors = false;

    this.alertService.showMessage("Success", `Tenant \"${this.tenantNew.tenantDetails.name}\" was created successfully`, MessageSeverity.success);

    if (this.changesSavedCallback)
      this.changesSavedCallback(this.tenantNew.tenantDetails);

    Object.assign(this.tenant, this.tenantNew);
    this.tenantNew = new TenantCreate();
    this.resetForm();
  }

  private saveFailedHelper(error: any) {
    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.alertService.showStickyMessage("Save Error", "The below errors occured whilst saving your changes:", MessageSeverity.error, error);
    this.alertService.showStickyMessage(error, null, MessageSeverity.error);

    if (this.changesFailedCallback)
      this.changesFailedCallback();
  }

  private cancel() {
      this.tenantNew = this.tenant = new TenantCreate();

    this.showValidationErrors = false;
    this.resetForm();

    this.alertService.showOperationCancelledMessage();
    this.alertService.resetStickyMessage();

    if (this.changesCancelledCallback)
      this.changesCancelledCallback();
  }

  private close() {
    this.tenantNew = this.tenant = new TenantCreate();
    this.showValidationErrors = false;
    this.resetForm();
  }

  resetForm(replace = false) {

    if (!replace) {
      this.form.reset();
    }
    else {
      this.formResetToggle = false;

      setTimeout(() => {
        this.formResetToggle = true;
      });
    }
  }

  newTenant() {

    this.tenant = this.tenantNew = new TenantCreate();
    // preset the service expiration date to 1 year ahead of the current time
    this.tenant.tenantDetails.serviceDetails.serviceExpireDate = new Date(new Date().setFullYear(new Date().getFullYear() + 1));
    this.showValidationErrors = true;

    return this.tenantNew;
  }
}
