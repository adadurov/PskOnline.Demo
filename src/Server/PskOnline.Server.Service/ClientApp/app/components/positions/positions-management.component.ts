
import { Component, OnInit, AfterViewInit, TemplateRef, ViewChild, Input } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';

import { AlertService, DialogType, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from "../../services/app-translation.service";
import { AccountService } from "../../services/account.service";
import { PositionService } from "../../services/position.service";
import { Utilities } from "../../services/utilities";
import { Permission } from '../../models/permission.model';
import { Position } from '../../models/position.model';
import { PositionInfoComponent } from "./position-info.component";


@Component({
  selector: 'positions-management',
  templateUrl: './positions-management.component.html',
  styleUrls: ['./positions-management.component.css']
})
export class PositionsManagementComponent implements OnInit, AfterViewInit {
  columns: any[] = [];
  rows: Position[] = [];
  rowsCache: Position[] = [];
  editedPosition: Position;
  sourcePosition: Position;
  editingPositionName: { name: string };
  loadingIndicator: boolean;

  @ViewChild('indexTemplate')
  indexTemplate: TemplateRef<any>;

  @ViewChild('positionNameTemplate')
  positionNameTemplate: TemplateRef<any>;

  @ViewChild('actionsTemplate')
  actionsTemplate: TemplateRef<any>;

  @ViewChild('editorModal')
  editorModal: ModalDirective;

  @ViewChild('positionEditor')
  positionEditor: PositionInfoComponent;

  constructor(
    private alertService: AlertService,
    private translationService: AppTranslationService,
    private accountService: AccountService,
    private positionService: PositionService) {
  }

  ngOnInit() {

    let gT = (key: string) => this.translationService.getTranslation(key);

    this.columns = [
      { prop: "index", name: '#', width: 40, cellTemplate: this.indexTemplate, canAutoResize: false },
      { prop: 'name', name: gT('positions.mgmt.Name'), width: 90, cellTemplate: this.positionNameTemplate },
    ];

    if (this.canManageOrgStructure) {
      this.columns.push(
        { name: '', width: 180, cellTemplate: this.actionsTemplate, resizeable: false, canAutoResize: false, sortable: false, draggable: false }
      );
    }

    this.loadData();
  }


  ngAfterViewInit() {

    this.positionEditor.changesSavedCallback = () => {
      this.addNewUserToList();
      this.editorModal.hide();
    };

    this.positionEditor.changesCancelledCallback = () => {
      this.editedPosition = null;
      this.sourcePosition = null;
      this.editorModal.hide();
    };
  }


  addNewUserToList() {
    if (this.sourcePosition) {
      Object.assign(this.sourcePosition, this.editedPosition);

      let sourceIndex = this.rowsCache.indexOf(this.sourcePosition, 0);
      if (sourceIndex > -1)
        Utilities.moveArrayItem(this.rowsCache, sourceIndex, 0);

      sourceIndex = this.rows.indexOf(this.sourcePosition, 0);
      if (sourceIndex > -1)
        Utilities.moveArrayItem(this.rows, sourceIndex, 0);

      this.editedPosition = null;
      this.sourcePosition = null;
    }
    else {
      let position = new Position();
      Object.assign(position, this.editedPosition);
      this.editedPosition = null;

      let maxIndex = 0;
      for (let u of this.rowsCache) {
        if ((<any>u).index > maxIndex)
          maxIndex = (<any>u).index;
      }

      (<any>position).index = maxIndex + 1;

      this.rowsCache.splice(0, 0, position);
      this.rows.splice(0, 0, position);
    }
  }


  loadData() {
    this.alertService.startDefaultLoadingMessage();
    this.loadingIndicator = true;

    this.positionService.getPositions().subscribe(
      positions => this.onDataLoadSuccessful(positions),
      error => this.onDataLoadFailed(error));
  }


  onDataLoadSuccessful(positions: Position[]) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.rowsCache = [...positions];
    this.rows = positions;
  }


  onDataLoadFailed(error: any) {
    this.alertService.stopLoadingMessage();
    this.loadingIndicator = false;

    this.alertService.showStickyMessage("Load Error", `Unable to retrieve positions from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
      MessageSeverity.error, error);
  }


  onSearchChanged(value: string) {
    this.rows = this.rowsCache.filter(r => Utilities.searchArray(value, false, r.name));
  }

  onEditorModalHidden() {
    this.editingPositionName = null;
    this.positionEditor.resetForm(true);
  }


  newPosition() {
    this.editingPositionName = null;
    this.sourcePosition = null;
    this.editedPosition = this.positionEditor.newPosition();
    this.editorModal.show();
  }


  editPosition(row: Position) {
    this.editingPositionName = { name: row.name };
    this.sourcePosition = row;
    this.editedPosition = this.positionEditor.editPosition(row);
    this.editorModal.show();
  }


  deletePosition(row: Position) {
    this.alertService.showDeleteWarning("positions.mgmt.positionAccusative", row.name, () => this.deletePositionHelper(row));
  }


  deletePositionHelper(row: Position) {

    this.alertService.startDelayedMessage("Deleting...");
    this.loadingIndicator = true;

    this.positionService.deletePosition(row)
      .subscribe(results => {
        this.alertService.stopLoadingMessage();
        this.loadingIndicator = false;

        this.rowsCache = this.rowsCache.filter(item => item !== row)
        this.rows = this.rows.filter(item => item !== row)
      },
        error => {
          this.alertService.stopLoadingMessage();
          this.loadingIndicator = false;

          this.alertService.showStickyMessage("Delete Error", `An error occured while deleting the position.\r\nError: "${Utilities.getHttpResponseMessage(error)}"`,
            MessageSeverity.error, error);
        });
  }

  get canManageOrgStructure() {
    return this.accountService.userHasPermission(Permission.manageTenantOrgStructurePermission);
  }

}
