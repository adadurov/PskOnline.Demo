import { fadeInOut } from '../../services/animations';
import { AfterViewInit } from '@angular/core';
import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { AlertService, DialogType, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from "../../services/app-translation.service";
import { Utilities } from "../../services/utilities";
import { forkJoin } from 'rxjs/observable/forkJoin';

import { AccountService } from '../../services/account.service';
import { BranchOfficeService } from '../../services/branch-office.service';
import { BranchTreeView } from '../viewmodels/branch-tree-view.model';
import { BranchOffice } from '../../models/branch-office.model';
import { Department } from '../../models/department.model';
import { DepartmentService } from '../../services/department.service';
import { Employee } from '../../models/employee.model';
import { EmployeeInfoComponent } from '../employees/employee-info.component'
import { EmployeeView } from '../viewmodels/employee-view.model';
import { EmployeeService } from '../../services/employee.service';
import { Position } from '../../models/position.model';
import { PositionService } from '../../services/position.service';
import { Permission } from '../../models/permission.model';
import { DepTreeView } from '../viewmodels/dep-tree-view.model';

@Component({
  selector: 'employees-management',
  templateUrl: './employees-management.component.html',
  styleUrls: ['./employees-management.component.css'],
  animations: [fadeInOut]
})
export class EmployeesManagementComponent implements OnInit, AfterViewInit {

  @ViewChild('orgStructureTree')
  private orgStructureTree;

  nodes: BranchTreeView[] = [];
  options = {};
  loadingIndicator: boolean;

  branchOffices: BranchOffice[] = [];

  departments: Department[] = [];

  positions: Position[] = [];

  currentBranchOffice: BranchOffice = undefined;

  currentDepartment: Department = undefined;

  editingEmployeeName: string;

  employeeListColumns: any[] = [];
  employeeListRows: EmployeeView[] = [];

  // cache for searching employees
  employeeListRowsCache: EmployeeView[] = [];

  ////////////////////////////////////////////////////////////////////////////////////
  @ViewChild('indexTemplate')
  indexTemplate: TemplateRef<any>;

  @ViewChild('employeeFullNameTemplate')
  employeeFullNameTemplate: TemplateRef<any>;

  @ViewChild('actionsTemplate')
  actionsTemplate: TemplateRef<any>;

  ////////////////////////////////////////////////////////////////////////////////////
  @ViewChild('employeeEditorModal')
  private employeeEditorModal;

  @ViewChild('employeeEditor')
  private employeeEditor: EmployeeInfoComponent;


  constructor(
    private employeeService: EmployeeService,
    private accountService: AccountService,
    private branchOfficeService: BranchOfficeService,
    private departmentService: DepartmentService,
    private positionService: PositionService,
    private alertService: AlertService,
    private translationService: AppTranslationService) {

  }

  ngOnInit() {
    let gT = (key: string) => this.translationService.getTranslation(key);

    this.employeeListColumns = [
      { prop: 'fullName', name: gT('employees.mgmt.fullName'), width: 90, cellTemplate: this.employeeFullNameTemplate },
      { prop: 'positionName', name: gT('employees.mgmt.positionName'), width: 90, cellTemplate: this.employeeFullNameTemplate }
    ];

    if (this.canManageEmployees) {
      this.employeeListColumns.push(
        { name: '', width: 180, cellTemplate: this.actionsTemplate, resizeable: false, canAutoResize: false, sortable: false, draggable: false }
      );
    }

    this.loadOrgStructureData();

    this.employeeEditor.changesCancelledCallback = () => { this.employeeEditorModal.hide(); };
    this.employeeEditor.changesSavedCallback = (employee) => {
      this.employeeEditorModal.hide();
      let i = this.employeeListRowsCache.findIndex(e => e.id == employee.id);
      if (i >= 0) {
        this.employeeListRowsCache.splice(i, 1);
      }
      var e = new EmployeeView(employee);
      e.positionName = this.positions.find(p => p.id == e.positionId).name;
      this.employeeListRowsCache.push(e);
      this.employeeListRowsCache.sort((a, b) => a.fullName > b.fullName ? 1 : -1);
      this.employeeListRows = this.employeeListRowsCache;
    };
  }

  ngAfterViewInit() {
    this.orgStructureTree.treeModel.expandAll();
  }

  loadOrgStructureData() {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;

    let branchesObs = this.branchOfficeService.getBranchOffices();
    let departmentsObs = this.departmentService.getDepartments();
    let positionsObs = this.positionService.getPositions();

    forkJoin([branchesObs, departmentsObs, positionsObs]).subscribe(
      result => this.onOrgStructureLoaded(result["0"], result["1"], result["2"]),
      error => this.onOrgStructureLoadFailed(error));
  }

  onOrgStructureLoaded(branches: BranchOffice[], departments: Department[], positions: Position[]) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.branchOffices = branches;
    this.departments = departments;
    this.positions = positions;

    branches = branches.sort((a, b) => a.name.localeCompare(b.name));
    this.nodes = branches.map(b =>
      new BranchTreeView(
        b,
        departments.filter(d => d.branchOfficeId == b.id).sort((a, b) => a.name.localeCompare(b.name))
      )
    );

    this.orgStructureTree.treeModel.expandAll();
  }

  onOrgStructureLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.alertService.showStickyMessage("Load Error", `Unable to retrieve org structure from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);
  }

  // see https://angular2-tree.readme.io/docs
  onTreeFocus($event) {
    // datatable should display employees
    if ($event.node.data instanceof DepTreeView) {
      this.currentDepartment = this.departments.find(d => d.id == $event.node.data.id);
      if (this.currentDepartment) {
        this.currentBranchOffice = this.branchOffices.find(b => b.id == this.currentDepartment.branchOfficeId);
      }
      this.loadEmployeesForCurrentDepartment();
    }
    else {
      this.currentDepartment = undefined;
      this.currentBranchOffice = this.branchOffices.find(b => b.id == $event.node.data.id);
    }
  }

  loadEmployeesForCurrentDepartment() {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;
    this.employeeListRows = [];
    if (this.currentDepartment) {
      this.departmentService.getEmployeesInDepartment(this.currentDepartment.id).subscribe(
        result => this.onEmployeesLoadSuccessful(result),
        error => this.onEmployeesLoadFailed(error));
    };
  }

  onEmployeesLoadSuccessful(employees: Employee[]) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.employeeListRowsCache = employees.map(employee => {
      let e = new EmployeeView(employee);
      e.positionName = this.positions.find(p => p.id == e.positionId).name;
      return e;
    });

    this.employeeListRows = this.employeeListRowsCache.map(e => e);
  }

  onEmployeesLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;
  }

  onNewEmployee(row: Employee) {
    if (this.currentDepartment && this.currentBranchOffice) {
      this.editingEmployeeName = null;
      this.employeeEditor.newEmployee(this.currentBranchOffice.id, this.currentDepartment.id);
      this.employeeEditorModal.show();
    }
  }

  onEmployeeEditorModalHidden() {
    this.employeeEditor.resetForm(true);
  }

  editEmployee(employee: EmployeeView) {
    let editedEmployee = new Employee();
    Object.assign(editedEmployee, employee);
    this.editingEmployeeName = editedEmployee.fullName;

    this.employeeEditor.editEmployee(editedEmployee);
    this.employeeEditorModal.show();

  }

  deleteEmployee(employee: EmployeeView) {
    if (employee) {
      this.alertService.showDeleteWarning(
        "employees.mgmt.employeeAccusative",
        employee.fullName,
        () => { // the action is executed upon confirmation
          this.employeeService.deleteEmployee(employee.id).subscribe(
            response => {
              let i = this.employeeListRowsCache.findIndex(e => e.id == employee.id);
              this.employeeListRowsCache.splice(i, 1);
              this.employeeListRows = this.employeeListRowsCache;
            },
            error => {
              this.alertService.showMessage("Error", error, MessageSeverity.error);
            });
        });
    }
  }

  onSearchChanged(value: string) {
    if (value && value.length > 0) {
      this.employeeListRows = this.employeeListRowsCache.filter(r => Utilities.searchArray(value, false, r.fullName, r.positionName));
    }
    else {
      this.employeeListRows = this.employeeListRowsCache;
    }
  }

  get canManageEmployees() {
    return this.accountService.userHasPermission(Permission.manageTenantOrgStructurePermission);
  }
}
