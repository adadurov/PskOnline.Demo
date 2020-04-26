
import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { TenantService } from "../../services/tenant.service";
import { AccountService } from "../../services/account.service";
import { Utilities } from '../../services/utilities';
import { Tenant } from '../../models/tenant.model';
import { Permission } from '../../models/permission.model';


@Component({
    selector: 'tenant-info',
    templateUrl: './tenant-info.component.html',
    styleUrls: ['./tenant-info.component.css']
})
export class TenantInfoComponent implements OnInit {

    private isEditMode = false;
    private isNewTenant = false;
    private isSaving = false;
    private isChangePassword = false;
    private showValidationErrors = false;
    private editingTenantName: string;
    private uniqueId: string = Utilities.uniqueId();
    private tenant: Tenant = new Tenant();
    private tenantEdit: Tenant;

    public formResetToggle = true;

    public changesSavedCallback: () => void;
    public changesFailedCallback: () => void;
    public changesCancelledCallback: () => void;

    @Input()
    isViewOnly: boolean;

    @Input()
    isGeneralEditor = false;

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

    constructor(
      private alertService: AlertService,
      private tenantService: TenantService,
      private accountService: AccountService) {
    }

    ngOnInit() {
        if (!this.isGeneralEditor) {
            this.loadCurrentTenantData();
        }
    }

    private loadCurrentTenantData() {
      this.alertService.startDefaultLoadingMessage();

        this.tenantService.getTenant().subscribe(
          results => this.onCurrentUserDataLoadSuccessful(results[0]),
          error => this.onCurrentUserDataLoadFailed(error));
    }


    private onCurrentUserDataLoadSuccessful(tenant: Tenant) {
      this.alertService.stopLoadingMessage();
      this.tenant = tenant;
    }

    private onCurrentUserDataLoadFailed(error: any) {
        this.alertService.stopLoadingMessage();
        this.alertService.showStickyMessage(
          "Load Error",
          `Unable to retrieve tenant data from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
            MessageSeverity.error, error);

        this.tenant = new Tenant();
    }


    private showErrorAlert(caption: string, message: string) {
        this.alertService.showMessage(caption, message, MessageSeverity.error);
    }


    private edit() {
        if (!this.isGeneralEditor) {
            this.tenantEdit = new Tenant();
            Object.assign(this.tenantEdit, this.tenant);
        }
        else {
            if (!this.tenantEdit)
                this.tenantEdit = new Tenant();
        }

        this.isEditMode = true;
        this.showValidationErrors = true;
        this.isChangePassword = false;
    }

    private save() {
        this.isSaving = true;
        this.alertService.startDelayedMessage("Saving changes...");

        this.tenantService.updateTenant(this.tenantEdit).subscribe(
          response => this.saveSuccessHelper(), error => this.saveFailedHelper(error));
    }


    private saveSuccessHelper(tenant?: Tenant) {

        if (tenant)
            Object.assign(this.tenantEdit, tenant);

        this.isSaving = false;
        this.alertService.stopLoadingMessage();
        this.isChangePassword = false;
        this.showValidationErrors = false;

        Object.assign(this.tenant, this.tenantEdit);
        this.tenantEdit = new Tenant();
        this.resetForm();


        if (this.isGeneralEditor) {
            this.alertService.showMessage("Success", `Changes to tenant \"${this.tenant.name}\" was saved successfully`, MessageSeverity.success);
        }

        this.isEditMode = false;

        if (this.changesSavedCallback)
            this.changesSavedCallback();
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
        if (this.isGeneralEditor)
            this.tenantEdit = this.tenant = new Tenant();
        else
            this.tenantEdit = new Tenant();

        this.showValidationErrors = false;
        this.resetForm();

        this.alertService.showOperationCancelledMessage();
        this.alertService.resetStickyMessage();

        if (!this.isGeneralEditor)
            this.isEditMode = false;

        if (this.changesCancelledCallback)
            this.changesCancelledCallback();
    }


    private close() {
        this.tenantEdit = this.tenant = new Tenant();
        this.showValidationErrors = false;
        this.resetForm();
        this.isEditMode = false;

        if (this.changesSavedCallback)
            this.changesSavedCallback();
    }


    private unlockTenant() {
        this.isSaving = true;
        this.alertService.startDelayedMessage("Unblocking tenant...");


        this.tenantService.unblockTenant(this.tenantEdit.id)
            .subscribe(response => {
                this.isSaving = false;
                this.tenantEdit.isLockedOut = false;
                this.alertService.stopLoadingMessage();
                this.alertService.showMessage("Success", "Tenant has been successfully unblocked", MessageSeverity.success);
            },
            error => {
                this.isSaving = false;
                this.alertService.stopLoadingMessage();
                this.alertService.showStickyMessage("Unblock Error", "The below errors occured whilst unblocking the tenant:", MessageSeverity.error, error);
                this.alertService.showStickyMessage(error, null, MessageSeverity.error);
            });
    }


    resetForm(replace = false) {
        this.isChangePassword = false;

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

    editTenant(tenant: Tenant) {
        if (tenant) {
            this.isGeneralEditor = true;
            this.isNewTenant = false;

            this.editingTenantName = tenant.name;
            this.tenant = new Tenant();
            this.tenantEdit = new Tenant();
            Object.assign(this.tenant, tenant);
            Object.assign(this.tenantEdit, tenant);
            this.edit();

            return this.tenantEdit;
        }
        else {
            return null;
        }
    }

    displayTenant(tenant: Tenant) {
        this.tenant = new Tenant();
        Object.assign(this.tenant, tenant);
        this.isEditMode = false;
    }

    get canEditGlobalTenants() {
      return this.accountService.userHasPermission(Permission.manageTenantsPermission);
    }
}
