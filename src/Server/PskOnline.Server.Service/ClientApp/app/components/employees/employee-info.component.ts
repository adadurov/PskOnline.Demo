
import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { DatePipe } from '@angular/common';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { EmployeeService } from "../../services/employee.service";
import { PositionService } from "../../services/position.service";
import { AccountService } from "../../services/account.service";
import { Utilities } from '../../services/utilities';
import { Employee } from '../../models/employee.model';
import { Permission } from '../../models/permission.model';
import { Position } from '../../models/position.model';
import { GenderView } from '../viewmodels/gender-view.model';
import { Gender } from '../../models/enums';
import { AppTranslationService } from '../../services/app-translation.service';


@Component({
  selector: 'employee-info',
  templateUrl: './employee-info.component.html',
  styleUrls: ['./employee-info.component.css']
})
export class EmployeeInfoComponent implements OnInit {

  private isEditMode = false;
  private isNewEmployee = false;
  private isSaving = false;
  private showValidationErrors = false;
  private editingEmployeeName: string;
  private uniqueId: string = Utilities.uniqueId();
  private employee: Employee = new Employee();
  private employeeEdit: Employee = new Employee();

  private currentPosition: Position;
  private allPositions: Position[] = [];

  private currentGender: GenderView;
  private allGenders: GenderView[] = [];

  public formResetToggle = true;

  public changesSavedCallback: (Employee) => void;
  public changesFailedCallback: () => void;
  public changesCancelledCallback: () => void;

  @Input()
  isViewOnly: boolean;

  @Input()
  isGeneralEditor = false;

  @ViewChild('f')
  private form;

  //ViewChilds Required because ngIf hides template variables from global scope
  @ViewChild('firstName')
  private firstName;

  @ViewChild('lastName')
  private lastName;

  @ViewChild('patronymic')
  private patronymic;

  @ViewChild('position')
  private position;

  @ViewChild('position')
  private gender;

  constructor(
    private employeeService: EmployeeService,
    private positionService: PositionService,
    private accountService: AccountService,
    private alertService: AlertService,
    private translationService: AppTranslationService) {
  }

  ngOnInit() {
    let gT = (key: string) => this.translationService.getTranslation(key);

    this.loadRequiredDataData();
    let gNone = new GenderView(Gender.Unknown, gT('employees.editor.genderNone'));
    let gMale = new GenderView(Gender.Male, gT('employees.editor.genderMale'));
    let gFemale = new GenderView(Gender.Female, gT('employees.editor.genderFemale'));
    this.allGenders = [gNone, gMale, gFemale];
  }

  private loadRequiredDataData() {
    this.alertService.startDefaultLoadingMessage();

    this.positionService.getPositions().subscribe(
      result => this.onPositionsDataLoadSuccess(result),
      error => this.onPositionsDataLoadFailed(error));
  }

  private onPositionsDataLoadSuccess(positions: Position[]) {
    this.alertService.stopLoadingMessage();
    if (positions.length == 0) {
      this.alertService.showStickyMessage(
        "Справочник должностей пуст",
        "Сначала необходимо создать должности в справочнике должностей вашей огранизации!",
        MessageSeverity.warn);
    }
    this.allPositions = positions;
    this.currentPosition = positions.find(p => p.id == this.employeeEdit.positionId);
  }

  private onPositionsDataLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.alertService.showStickyMessage(
      "Load Error",
      `Unable to retrieve employee data from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);

    this.employee = new Employee();
  }

  private showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  private edit() {
    this.isEditMode = true;
    this.showValidationErrors = true;
  }

  private save() {
    this.isSaving = true;
    this.alertService.startDelayedMessage("Saving changes...");

    if (this.currentPosition) {
      this.employeeEdit.positionId = this.currentPosition.id;
    }
    if (this.currentGender) {
      this.employeeEdit.gender = this.currentGender.code;
    }

    if (this.isNewEmployee) {
      this.employeeService.newEmployee(this.employeeEdit).subscribe(
        response => { this.employeeEdit.id = response.id; this.saveSuccessHelper(this.employeeEdit), error => this.saveFailedHelper(error) });
    }
    else {
      this.employeeService.updateEmployee(this.employeeEdit).subscribe(
        response => this.saveSuccessHelper(this.employeeEdit), error => this.saveFailedHelper(error));
    }
  }

  private saveSuccessHelper(employee?: Employee) {

    if (employee)
      Object.assign(this.employeeEdit, employee);

    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.showValidationErrors = false;

    Object.assign(this.employee, this.employeeEdit);

    this.employeeEdit = new Employee();
    this.resetForm();

    this.isEditMode = false;

    if (this.isNewEmployee) {
      this.alertService.showMessage("Success", `Employee \"${this.employee.fullName}\" was saved successfully`, MessageSeverity.success);
    }
    else {
      this.alertService.showMessage("Success", `Changes to employee \"${this.employee.fullName}\" was saved successfully`, MessageSeverity.success);
    }

    if (this.changesSavedCallback)
      this.changesSavedCallback(employee);

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
      this.employeeEdit = this.employee = new Employee();
    else
      this.employeeEdit = new Employee();

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
    this.employeeEdit = this.employee = new Employee();
    this.showValidationErrors = false;
    this.resetForm();
    this.isEditMode = false;

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

  newEmployee(branchOfficeId: string, departmentId: string) {
    this.isNewEmployee = true;
    this.editingEmployeeName = null;
    this.employee = this.employeeEdit = new Employee();
    this.currentPosition = null;
    this.currentGender = null;
    this.employeeEdit.branchOfficeId = branchOfficeId;
    this.employeeEdit.departmentId = departmentId;
    this.edit();

    return this.employeeEdit;
  }

  editEmployee(employee: Employee) {
    if (employee) {
      this.isGeneralEditor = true;
      this.isNewEmployee = false;

      this.editingEmployeeName = employee.fullName;
      this.employee = new Employee();
      this.employeeEdit = new Employee();
      Object.assign(this.employee, employee);
      Object.assign(this.employeeEdit, employee);

      this.currentPosition = this.allPositions.find(p => p.id == this.employeeEdit.positionId);
      this.currentGender = this.allGenders.find(g => g.code == this.employeeEdit.gender);

      this.edit();

      return this.employeeEdit;
    }
    else {
      return null;
    }
  }

  displayEmployee(employee: Employee) {
    this.employee = new Employee();
    Object.assign(this.employee, employee);
    this.isEditMode = false;
  }

  public comparePositions(r1: Position, r2: Position): boolean {
    return r1 && r2 ? r1.id === r2.id : r1 === r2;
  }

  public compareGenders(g1: GenderView, g2: GenderView): boolean {
    return g1 && g2 ? g1.code === g2.code : g1 === g2;
  }

  get canEditGlobalEmployees() {
    return this.accountService.userHasPermission(Permission.manageTenantOrgStructurePermission);
  }
}
