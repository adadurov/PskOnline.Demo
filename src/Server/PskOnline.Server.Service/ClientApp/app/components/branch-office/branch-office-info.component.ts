import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { BranchOfficeService } from "../../services/branch-office.service";
import { AccountService } from "../../services/account.service";
import { Utilities } from '../../services/utilities';
import { BranchOffice } from '../../models/branch-office.model';
import { Permission } from '../../models/permission.model';
import { TimeZoneService } from '../../services/time-zone.service';
import { TimeZoneView } from '../viewmodels/time-zone-view.model';

@Component({
  selector: 'branch-office-info',
  templateUrl: './branch-office-info.component.html',
  styleUrls: ['./branch-office-info.component.css']
})
export class BranchOfficeInfoComponent implements OnInit {

  private isEditMode = false;
  private isNewBranchOffice = false;
  private isSaving = false;
  private showValidationErrors = false;
  private editingBranchOfficeName: string;
  private uniqueId: string = Utilities.uniqueId();
  private branchOffice: BranchOffice = new BranchOffice();
  private branchOfficeEdit: BranchOffice = new BranchOffice();

  private allTimeZones: TimeZoneView[];

  private currentTimeZone: TimeZoneView;

  private currentTimeZoneId: string;
  private currentTimeZoneDisplayName: string; // for viewing branch office information

  public formResetToggle = true;

  public changesSavedCallback: (BranchOffice) => void;
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

  @ViewChild('timeZone')
  private timeZone;

  constructor(
    private alertService: AlertService,
    private branchOfficeService: BranchOfficeService,
    private accountService: AccountService,
    private timeZoneService: TimeZoneService) {
    this.allTimeZones = timeZoneService.GetTimeZones();
  }

  ngOnInit() {
  }

  private showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  private edit() {
    if (!this.isGeneralEditor) {
      this.branchOfficeEdit = new BranchOffice();
      Object.assign(this.branchOfficeEdit, this.branchOffice);
    }
    else {
      if (!this.branchOfficeEdit)
        this.branchOfficeEdit = new BranchOffice();
    }

    this.isEditMode = true;
    this.showValidationErrors = true;
  }

  onSubmit() {
    if (this.form.form.valid) {
      this.save();
    }
    else {
      this.showErrorAlert(
        'Branch office name is required',
        'Please enter a name (minimum of 2 and maximum of 200 characters)'
      );
    }
  }

  private save() {
    this.isSaving = true;
    this.alertService.startDefaultSavingMessage();

    // required to make the objec available in subscribe routines
    var branchOfficeForUpdate = this.branchOfficeEdit;
    branchOfficeForUpdate.timeZoneId = this.currentTimeZone.id;

    if (this.isNewBranchOffice) {
      this.branchOfficeService.newBranchOffice(branchOfficeForUpdate).subscribe(
        response => { branchOfficeForUpdate.id = response.id; this.saveSuccessHelper(branchOfficeForUpdate); },
        error => this.saveFailedHelper(error));
    }
    else {
      this.branchOfficeService.updateBranchOffice(branchOfficeForUpdate).subscribe(
        response => this.saveSuccessHelper(branchOfficeForUpdate),
        error => this.saveFailedHelper(error));
    }
  }

  private compareTimeZones(tz1: TimeZoneView, tz2: TimeZoneView): boolean {
    return tz1 && tz2 ? tz1.id === tz2.id : tz1 === tz2;
  }

  private saveSuccessHelper(branchOffice: BranchOffice) {
    if (branchOffice) {
      Object.assign(this.branchOfficeEdit, branchOffice);
    }

    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.showValidationErrors = false;

    Object.assign(this.branchOffice, this.branchOfficeEdit);
    this.branchOfficeEdit = new BranchOffice();
    this.resetForm();

    if (this.isGeneralEditor) {
      this.alertService.showMessage(
        "Success",
        `Changes to branch office \"${this.branchOffice.name}\" was saved successfully`,
        MessageSeverity.success);
    }

    this.isEditMode = false;

    if (this.changesSavedCallback) {
      this.changesSavedCallback(branchOffice);
    }
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
      this.branchOfficeEdit = this.branchOffice = new BranchOffice();
    else
      this.branchOfficeEdit = new BranchOffice();

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
    this.branchOfficeEdit = this.branchOffice = new BranchOffice();
    this.showValidationErrors = false;
    this.resetForm();
    this.isEditMode = false;

    if (this.changesSavedCallback)
      this.changesCancelledCallback();
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

  newBranchOffice() {
    this.isNewBranchOffice = true;
    this.editingBranchOfficeName = null;
    this.branchOffice = this.branchOfficeEdit = new BranchOffice();
    this.edit();

    return this.branchOfficeEdit;
  }

  editBranchOffice(branchOffice: BranchOffice) {
    if (branchOffice) {
      this.isGeneralEditor = true;
      this.isNewBranchOffice = false;

      this.editingBranchOfficeName = branchOffice.name;
      this.branchOffice = new BranchOffice();
      this.branchOfficeEdit = new BranchOffice();
      Object.assign(this.branchOffice, branchOffice);
      Object.assign(this.branchOfficeEdit, branchOffice);
      this.currentTimeZone = this.allTimeZones.find(t => t.id == branchOffice.timeZoneId);
      if (this.currentTimeZone) {
        this.currentTimeZoneDisplayName = this.currentTimeZone.displayName;
        this.currentTimeZoneId = this.currentTimeZone.id;
      }
      this.edit();

      return this.branchOfficeEdit;
    }
    else {
      return null;
    }
  }

  displayBranchOffice(branchOffice: BranchOffice) {
    this.branchOffice = new BranchOffice();
    Object.assign(this.branchOffice, branchOffice);
    this.currentTimeZone = this.allTimeZones.find(t => t.id == branchOffice.timeZoneId);
    if (this.currentTimeZone) {
      this.currentTimeZoneDisplayName = this.currentTimeZone.displayName;
      this.currentTimeZoneId = this.currentTimeZone.id;
    }
    this.isEditMode = false;
  }

  get canEditGlobalEmployees() {
    return this.accountService.userHasPermission(Permission.manageTenantOrgStructurePermission);
  }
}
