
import { Component, OnInit, AfterViewInit, TemplateRef, ViewChild, Input } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';

import { AlertService, DialogType, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from "../../services/app-translation.service";
import { AccountService } from "../../services/account.service";
import { TenantService } from "../../services/tenant.service";
import { Utilities } from "../../services/utilities";

import { Tenant } from '../../models/tenant.model';
import { TenantCreate } from '../../models/tenant-create.model';
import { Permission } from '../../models/permission.model';
import { TenantInfoComponent } from "./tenant-info.component";
import { TenantCreateComponent } from "./tenant-create.component";


@Component({
  selector: 'tenants-management',
  templateUrl: './tenants-management.component.html',
  styleUrls: ['./tenants-management.component.css']
})
export class TenantsManagementComponent implements OnInit, AfterViewInit {
  columns: any[] = [];
  rows: Tenant[] = [];
  rowsCache: Tenant[] = [];
  createdTenant: TenantCreate;
  editedTenant: Tenant;
  sourceTenant: Tenant;
  editingTenantName: { name: string };
  loadingIndicator: boolean;


  @ViewChild('indexTemplate')
  indexTemplate: TemplateRef<any>;

  @ViewChild('tenantNameTemplate')
  tenantNameTemplate: TemplateRef<any>;

  @ViewChild('actionsTemplate')
  actionsTemplate: TemplateRef<any>;

  @ViewChild('editorModal')
  editorModal: ModalDirective;

  @ViewChild('tenantEditor')
  tenantEditor: TenantInfoComponent;

  @ViewChild('creatorModal')
  creatorModal: ModalDirective;

  @ViewChild('tenantCreator')
  tenantCreator: TenantCreateComponent;

  constructor(
    private alertService: AlertService,
    private translationService: AppTranslationService,
    private accountService: AccountService,
    private tenantService: TenantService) {
  }

  ngOnInit() {

    let gT = (key: string) => this.translationService.getTranslation(key);

    this.columns = [
      { prop: "index", name: '#', width: 40, cellTemplate: this.indexTemplate, canAutoResize: false },
      { prop: 'name', name: gT('tenants.mgmt.Name'), width: 100 },
      { prop: 'slug', name: gT('tenants.mgmt.Slug'), width: 70 },
      { prop: 'serviceDetails.serviceExpireDate', name: gT('tenants.mgmt.ExpDate'), width: 90, cellTemplate: this.tenantNameTemplate },
      { prop: 'serviceDetails.serviceMaxUsers', name: gT('tenants.mgmt.MaxUsers'), width: 50 },
      { prop: 'serviceDetails.serviceMaxEmployees', name: gT('tenants.mgmt.MaxEmployees'), width: 50 },
      { prop: 'primaryContact.email', name: gT('tenants.mgmt.PrimaryContactEmail'), width: 140 },
      { prop: 'primaryContact.fullName', name: gT('tenants.mgmt.PrimaryContactFullName'), width: 140 },
      { prop: 'primaryContact.mobilePhoneNumber', name: gT('tenants.mgmt.PrimaryContactMobilePhone'), width: 110 },

    ];

    if (this.canManageTenants)
      this.columns.push({ name: '', width: 120, cellTemplate: this.actionsTemplate, resizeable: false, canAutoResize: false, sortable: false, draggable: false });

    this.loadData();
  }


  ngAfterViewInit() {

    this.tenantCreator.changesSavedCallback = (newTenant: Tenant) => {
      this.addNewTenantToList(newTenant);
      this.creatorModal.hide();
    };

    this.tenantCreator.changesCancelledCallback = () => {
      this.creatorModal.hide();
    }

    this.tenantEditor.changesSavedCallback = () => {
      this.updateTenantInList();
      this.editorModal.hide();
    };

    this.tenantEditor.changesCancelledCallback = () => {
      this.editedTenant = null;
      this.sourceTenant = null;
      this.editorModal.hide();
    };
  }

  updateTenantInList() {

  }

  addNewTenantToList(createdTenant: Tenant) {
    if (this.sourceTenant) {
      Object.assign(this.sourceTenant, this.editedTenant);

      let sourceIndex = this.rowsCache.indexOf(this.sourceTenant, 0);
      if (sourceIndex > -1)
        Utilities.moveArrayItem(this.rowsCache, sourceIndex, 0);

      sourceIndex = this.rows.indexOf(this.sourceTenant, 0);
      if (sourceIndex > -1)
        Utilities.moveArrayItem(this.rows, sourceIndex, 0);

      this.editedTenant = null;
      this.sourceTenant = null;
    }
    else {
      let maxIndex = 0;
      for (let u of this.rowsCache) {
        if ((<any>u).index > maxIndex)
          maxIndex = (<any>u).index;
      }

      (<any>createdTenant).index = maxIndex + 1;

      this.rowsCache.splice(0, 0, createdTenant);
      this.rows.splice(0, 0, createdTenant);
    }
  }


  loadData() {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;

    if (this.canManageTenants) {
      this.tenantService.getTenants().subscribe(
        results => this.onDataLoadSuccessful(results),
        error => this.onDataLoadFailed(error)
      );
    }
  }


  onDataLoadSuccessful(tenants: Tenant[]) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    tenants.forEach((tenant, index, tenants) => {
      (<any>tenant).index = index + 1;
    });

    this.rowsCache = [...tenants];
    this.rows = tenants;
  }


  onDataLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.alertService.showStickyMessage("Load Error", `Unable to retrieve tenants from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);
  }


  onSearchChanged(value: string) {
    this.rows = this.rowsCache.filter(r =>
      Utilities.searchArray(value, false,
        r.name, r.slug, r.comment,
        r.primaryContact.fullName, r.primaryContact.email,
        r.primaryContact.mobilePhoneNumber, r.primaryContact.officePhoneNumber,
        r.primaryContact.city, r.primaryContact.streetAddress, r.primaryContact.comment));
  }

  onEditorModalHidden() {
    this.editingTenantName = null;
    this.tenantEditor.resetForm(true);
  }

  onCreatorModalHidden() {
    this.editingTenantName = null;
    this.tenantCreator.resetForm(true);
  }

  newTenant() {
    this.editingTenantName = null;
    this.sourceTenant = null;
    this.createdTenant = this.tenantCreator.newTenant();
    this.creatorModal.show();
  }

  editTenant(row: Tenant) {
    this.editingTenantName = { name: row.name };
    this.sourceTenant = row;
    this.editedTenant = this.tenantEditor.editTenant(row);
    this.editorModal.show();
  }


  deleteTenant(row: Tenant) {
    this.alertService.showDeleteWarning("tenants.mgmt.tenantAccusative", row.name, () => this.deleteTenantHelper(row));
  }


  deleteTenantHelper(row: Tenant) {

    this.alertService.startDelayedMessage("Deleting...");
    this.loadingIndicator = true;

    this.tenantService.deleteTenant(row)
      .subscribe(results => {
        this.alertService.stopLoadingMessage();
        this.loadingIndicator = false;

        this.rowsCache = this.rowsCache.filter(item => item !== row)
        this.rows = this.rows.filter(item => item !== row)
      },
        error => {
          this.alertService.stopLoadingMessage();
          this.loadingIndicator = false;

          this.alertService.showStickyMessage("Delete Error", `An error occured whilst deleting the tenant.\r\nError: "${Utilities.getHttpResponseMessage(error)}"`,
            MessageSeverity.error, error);
        });
  }

  get canManageTenants() {
    return this.accountService.userHasPermission(Permission.manageTenantsPermission);
  }

  get canViewTenants() {
    return this.accountService.userHasPermission(Permission.viewTenantsPermission);
  }

}
