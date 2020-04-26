import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { DepartmentService } from "../../services/department.service";
import { AccountService } from "../../services/account.service";
import { Utilities } from '../../services/utilities';
import { Department } from '../../models/department.model';
import { Permission } from '../../models/permission.model';
import * as pwdGenerator from 'generate-password-browser';
import { DepartmentWorkplaceCredentials } from '../../models/department-workplace-credentials.model';
import { DepartmentWorkplace } from '../../models/department-workplace.model';
import { PskOnlineScopes } from '../../models/pskonline-scopes.model';

@Component({
  selector: 'department-create',
  templateUrl: './department-create.component.html',
  styleUrls: ['./department-create.component.css']
})
export class DepartmentCreateComponent implements OnInit {

  private isSaving = false;
  private showValidationErrors = false;
  private uniqueId: string = Utilities.uniqueId();
  private departmentEdit: Department = new Department();

  private branchOfficeName: string;
  private branchOfficeId: string;

  public formResetToggle = true;

  public changesSavedCallback: (d: Department, o: DepartmentWorkplaceCredentials, a: DepartmentWorkplaceCredentials) => void;
  public changesFailedCallback: () => void;
  public changesCancelledCallback: () => void;

  @ViewChild('f')
  private form;

  //ViewChilds Required because ngIf hides template variables from global scope
  @ViewChild('nameInput')
  private nameInput;

  constructor(
    private alertService: AlertService,
    private departmentService: DepartmentService,
    private accountService: AccountService) {
  }

  ngOnInit() {
  }

  private showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  private async save() {
    this.isSaving = true;
    this.alertService.startDelayedMessage("Saving new department...");
    this.departmentEdit.branchOfficeId = this.branchOfficeId;

    var newDepartmentId: string = null;
    var opCreds: DepartmentWorkplaceCredentials = null;
    var audCreds: DepartmentWorkplaceCredentials = null;

    try {
      var response = await this.departmentService.newDepartment(this.departmentEdit).toPromise();
      this.departmentEdit.id = newDepartmentId = response.id;

      // create credentials for operator workbench
      var opBenchDesc = new DepartmentWorkplace();
      opBenchDesc.scopes = PskOnlineScopes.OperatorWorkplaceScope;
      opCreds = await this.departmentService.newCredentials(newDepartmentId, opBenchDesc).toPromise();

      // create credentials for auditor workbench
      var audBenchDesc = new DepartmentWorkplace();
      audBenchDesc.scopes = PskOnlineScopes.AuditorWorkplaceScope;
      audCreds = await this.departmentService.newCredentials(newDepartmentId, audBenchDesc).toPromise();
    }
    catch (error) {
      // FIXME: maybe it is worth rolling back the entire failed transaction -- 
      // remove department and credentials that have been created before the failure occurred
      this.saveFailedHelper(error);
      return;
    }
    // callback to inform the owner about successful completion
    this.saveSuccessHelper(this.departmentEdit, opCreds, audCreds);
  }

  private saveSuccessHelper(departmentCreated: Department, opCreds: DepartmentWorkplaceCredentials, audCreds: DepartmentWorkplaceCredentials) {

    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.showValidationErrors = false;

    this.alertService.showMessage("Success", `Department \"${this.departmentEdit.name}\" created successfully`, MessageSeverity.success);

    if (this.changesSavedCallback)
      this.changesSavedCallback(this.departmentEdit, opCreds, audCreds);

    this.departmentEdit = new Department();
    this.branchOfficeId = null;
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
    this.departmentEdit = new Department();

    this.showValidationErrors = false;
    this.resetForm();

    this.alertService.showOperationCancelledMessage();
    this.alertService.resetStickyMessage();

    if (this.changesCancelledCallback)
      this.changesCancelledCallback();
  }

  private close() {
    this.departmentEdit = new Department();
    this.showValidationErrors = false;
    this.resetForm();

    if (this.changesCancelledCallback)
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

  newDepartment(branchOfficeId:string, branchOfficeName: string) {
    this.departmentEdit = new Department();

    var pwdOptions = {
      length: 15, // defaults to 10
      numbers: true, // defaults to false
      symbols: true, // defaults to false
      uppercase: true, // defaults to true
      strict: true // defaults to false
    };

    this.branchOfficeId = branchOfficeId;
    this.branchOfficeName = branchOfficeName;
    this.showValidationErrors = true;

    return this.departmentEdit;
  }

}
