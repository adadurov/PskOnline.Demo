import { fadeInOut } from '../../services/animations';
import { forkJoin } from 'rxjs/observable/forkJoin';
import { Component, OnInit, OnDestroy, ViewChild, Input } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';
import { AlertService, DialogType, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from "../../services/app-translation.service";
import { Utilities } from "../../services/utilities";

import { DepartmentService } from '../../services/department.service';
import { BranchOfficeService } from '../../services/branch-office.service';
import { BranchTreeView } from '../viewmodels/branch-tree-view.model';
import { BranchOffice } from '../../models/branch-office.model';
import { Department } from '../../models/department.model';
import { BranchOfficeInfoComponent } from '../branch-office/branch-office-info.component';
import { DepartmentCreateComponent } from '../department/department-create.component';
import { DepTreeView } from '../viewmodels/dep-tree-view.model';
import { CredentialsEncoder } from './credentials-encoder';
import { DeptWorkplaces } from '../viewmodels/dept-workplaces.model';
import { WindowLocationService } from '../../services/window-location.service';

@Component({
  selector: 'orgstructure-management',
  templateUrl: './orgstructure-management.component.html',
  styleUrls: ['./orgstructure-management.component.css'],
  animations: [fadeInOut]
})
export class OrgStructureManagementComponent implements OnInit {

  @ViewChild('orgStructureTree')
  private orgStructureTree;

  nodes: BranchTreeView[] = [];

  options = {};

  branchOffices: BranchOffice[] = [];

  departments: Department[] = [];

  currentBranchOffice: BranchOffice = undefined;

  currentDepartment: Department = undefined;

  lastCreatedDepartment: Department = new Department();

  lastCreatedDepartmentCredentials: DeptWorkplaces = new DeptWorkplaces ();

  editingBranchName: string;

  ////////////////////////////////////////////////////////////////////////////////////
  @ViewChild('branchEditorModal')
  private branchEditorModal;

  @ViewChild('branchEditor')
  private branchEditor: BranchOfficeInfoComponent;

  private editedBranch: BranchOffice = new BranchOffice();

  ////////////////////////////////////////////////////////////////////////////////////

  @ViewChild('departmentCreatorModal')
  private departmentCreatorModal;

  @ViewChild('departmentCreatorComponent')
  private departmentCreatorComponent: DepartmentCreateComponent;

  @ViewChild('newDepartmentSummaryModal')
  private newDepartmentSummaryModal;

  @ViewChild('detailsForClipboard')
  private detailsForClipboard;

  private editingDepartmentName: string;
  ////////////////////////////////////////////////////////////////////////////////////

  loadingIndicator: boolean;

  constructor(
    private branchOfficeService: BranchOfficeService,
    private departmentService: DepartmentService,
    private alertService: AlertService,
    private translationService: AppTranslationService,
    private hostNameProviderService: WindowLocationService)
  {

  }

  ngOnInit() {
//    let gT = (key: string) => this.translationService.getTranslation(key);

    this.branchEditor.changesSavedCallback = (branch) => {
      this.addOrUpdateBranch(branch);
      this.branchEditorModal.hide();
    };

    this.branchEditor.changesCancelledCallback = () => {
      this.editedBranch = null;
      this.branchEditorModal.hide();
    };

    this.departmentCreatorComponent.changesSavedCallback = (dept, opCred, audCred) => {
      // the handler is huge because of closure issues
      this.addOrUpdateDepartment(dept);
      this.departmentCreatorModal.hide();

      // display details of the created department
      this.lastCreatedDepartment = dept;
      var encoder = new CredentialsEncoder();
      this.lastCreatedDepartmentCredentials = encoder.EncodeMultiple(
        this.hostNameProviderService, opCred, audCred); 
      this.newDepartmentSummaryModal.show();
    };

    this.departmentCreatorComponent.changesCancelledCallback = () => {
      this.departmentCreatorModal.hide();
    };

    this.loadData();
  }

  loadData() {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;

    let branchesObs = this.branchOfficeService.getBranchOffices();
    let departmentsObs = this.departmentService.getDepartments();

    forkJoin([branchesObs, departmentsObs]).subscribe(
      result => this.onDataLoadSuccessful(result["0"], result["1"]),
      error => this.onDataLoadFailed(error));
  }

  onDataLoadSuccessful(branches: BranchOffice[], departments: Department[]) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.branchOffices = branches;
    this.departments = departments;

    branches = branches.sort((a, b) => a.name.localeCompare(b.name));
    this.nodes = branches.map(b =>
      new BranchTreeView(
        b,
        departments.filter(d => d.branchOfficeId == b.id).sort((a, b) => a.name.localeCompare(b.name))
      )
    );
    this.orgStructureTree.treeModel.expandAll();
  }

  onDataLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.alertService.showStickyMessage("Load Error", `Unable to retrieve tenants from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);
  }

  onTreeFocus($event) {
    // see https://angular2-tree.readme.io/docs
    if ($event.node.data instanceof DepTreeView) {
      this.currentDepartment = this.departments.find(d => d.id == $event.node.data.id);
      if (null != this.currentDepartment) {
        this.currentBranchOffice = this.branchOffices.find(b => b.id == this.currentDepartment.branchOfficeId);
      }
    }
    else {
      this.currentDepartment = undefined;
      this.currentBranchOffice = this.branchOffices.find(b => b.id == $event.node.data.id);
    }
  }

  newBranchOffice() {
    this.editedBranch = this.branchEditor.newBranchOffice();
    this.editingBranchName = null;
    this.branchEditorModal.show();
  }

  editBranchOffice() {
    this.editedBranch = this.currentBranchOffice;
    this.branchEditor.editBranchOffice(this.editedBranch);
    this.editingBranchName = this.editedBranch.name;
    this.branchEditorModal.show();
  }

  viewBranchOffice() {
    this.editedBranch = this.currentBranchOffice;
    this.editingBranchName = null;//this.editedBranch.name;
    this.branchEditor.displayBranchOffice(this.editedBranch);
    this.branchEditorModal.show();
  }

  deleteBranchOffice() {
    var orgTree = this.orgStructureTree;
    var nodes = this.nodes;
    var curBranchOffice = this.currentBranchOffice;
    var alertService = this.alertService;

    // branch office has departments?
    var branchOfficeNode = nodes.find(b => b.id == curBranchOffice.id);
    if (branchOfficeNode.children.length > 0) {
      this.alertService.showDialog(
        'Сначала необходимо удалить все отделы филиала \"' + curBranchOffice.name + '\"!',
        DialogType.alert, () => { }
      );
      return;
    }
    // user confirms deletion?
    this.alertService.showDeleteWarning(
      "orgstructure.mgmt.branchOfficeAccusative", curBranchOffice.name,
      () => { 
        this.branchOfficeService.deleteBranchOffice(curBranchOffice.id).subscribe(
          response => {
            var index = nodes.findIndex(b => b.id == curBranchOffice.id);
            nodes = nodes.splice(index, 1);
            orgTree.treeModel.update();
            this.currentBranchOffice = undefined;
          },
          error => {
            alertService.showMessage("Error", error, MessageSeverity.error);
          }
        );
      });
  }

  async newDepartment() {
    await this.departmentCreatorComponent.newDepartment(this.currentBranchOffice.id, this.currentBranchOffice.name);
    this.departmentCreatorModal.show();
  }

  viewDepartment() {
    this.alertService.showDialog(
      'Не реализовано',
      DialogType.alert,
      () => { // the action is executed upon confirmation
      });
  }

  editDepartment() {
    this.alertService.showDialog(
      'Не реализовано',
      DialogType.alert,
      () => { // the action is executed upon confirmation
      });
  }

  deleteDepartment() {
    var orgTree = this.orgStructureTree;
    var nodes = this.nodes;
    var curBranchOffice = this.currentBranchOffice;
    var curDepartment = this.currentDepartment;
    var alertService = this.alertService;

    this.alertService.showDeleteWarning(
      "orgstructure.mgmt.departmentAccusative",
      this.currentDepartment.name, () =>
      { // the action is executed upon confirmation
        this.departmentService.deleteDepartment(this.currentDepartment.id).subscribe(
          response => {
            var branchNode = nodes.find(b => b.id == curBranchOffice.id);
            var deptIndex = branchNode.children.findIndex(d => d.id == curDepartment.id);
            branchNode.children.splice(deptIndex, 1);
            orgTree.treeModel.update();
            this.currentDepartment = undefined;
          },
          error => {
            alertService.showMessage("Error", error, MessageSeverity.error);
          }
        );
      });
  }

  addOrUpdateBranch(branchOffice: BranchOffice) {
    var existingTreeNode = this.nodes.find(n => n.id === branchOffice.id);
    if (!existingTreeNode) {
      // FIXME: new branch always added to the end
      var newNode = new BranchTreeView(branchOffice, []);
      this.branchOffices.push(branchOffice);
      this.nodes.push(newNode);
      this.orgStructureTree.treeModel.update();
      this.orgStructureTree.treeModel.setFocusedNode(newNode);
    }
    else {
      existingTreeNode.name = branchOffice.name;
      var existingBranch = this.branchOffices.find(b => b.id === branchOffice.id);
      existingBranch.name = branchOffice.name;
      existingBranch.timeZoneId = branchOffice.timeZoneId;
      this.orgStructureTree.treeModel.update();

      this.currentBranchOffice = new BranchOffice();
      Object.assign(this.currentBranchOffice, existingBranch);
    }
  }

  addOrUpdateDepartment(department: Department) {
    var existingBranchTreeNode = this.nodes.find(n => n.id === department.branchOfficeId);

    var existingDepartmentTreeNode = existingBranchTreeNode.children.find(d => d.id === department.id);

    if (!existingDepartmentTreeNode) {
      // FIXME: new branch always added to the end
      var newNode = new DepTreeView(department.id, department.name);
      this.departments.push(department);
      existingBranchTreeNode.children.push(newNode);
      this.orgStructureTree.treeModel.update();
      this.orgStructureTree.treeModel.setFocusedNode(newNode);
    }
    else {
      existingDepartmentTreeNode.name = department.name;
      var existingDepartment = this.departments.find(d => d.id === department.id);
      existingDepartment.name = department.name;
      this.orgStructureTree.treeModel.update();
    }
  }

  onBranchOfficeEditorModalHidden() {
    this.editingBranchName = null;
    this.branchEditor.resetForm(true);
  }

  onDepartmentCreatorModalHidden() {
    this.editingDepartmentName = null;
    this.departmentCreatorComponent.resetForm(true);
  }

  hideNewDepartmentSummaryModal() {
    this.newDepartmentSummaryModal.hide();
    this.lastCreatedDepartmentCredentials = new DeptWorkplaces();
    this.lastCreatedDepartment = new Department();
  }

  copyDetailsOfDepartment() {
    var str = this.detailsForClipboard.nativeElement.innerText;
    const el = document.createElement('textarea');
    el.value = str;
    el.setAttribute('readonly', '');
    el.style.position = 'absolute';
    el.style.left = '-9999px';
    document.body.appendChild(el);
    el.select();
    document.execCommand('copy');
    document.body.removeChild(el);
  }
}
