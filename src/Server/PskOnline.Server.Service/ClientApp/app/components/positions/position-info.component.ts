
import { Component, OnInit, ViewChild, Input } from '@angular/core';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AccountService } from "../../services/account.service";
import { Utilities } from '../../services/utilities';
import { Position } from '../../models/position.model';
import { PositionService } from '../../services/position.service';

@Component({
  selector: 'position-info',
  templateUrl: './position-info.component.html',
  styleUrls: ['./position-info.component.css']
})
export class PositionInfoComponent implements OnInit {

  private isEditMode = false;
  private isNewPosition = false;
  private isSaving = false;
  private isChangePassword = false;
  private isEditingSelf = false;
  private showValidationErrors = false;
  private editingPositionName: string;
  private uniqueId: string = Utilities.uniqueId();
  private position: Position = new Position();
  private positionEdit: Position;

  public formResetToggle = true;

  public changesSavedCallback: () => void;
  public changesFailedCallback: () => void;
  public changesCancelledCallback: () => void;

  @Input()
  isViewOnly: boolean;

  @ViewChild('f')
  private form;

  //ViewChilds Required because ngIf hides template variables from global scope
  @ViewChild('positionName')
  private positionName;

  constructor(
    private alertService: AlertService,
    private accountService: AccountService,
    private positionService: PositionService) {
  }

  ngOnInit() {
  }

  private showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  private edit() {
      if (!this.positionEdit)
        this.positionEdit = new Position();
    this.isEditMode = true;
    this.showValidationErrors = true;
    this.isChangePassword = false;
  }

  private save() {
    this.isSaving = true;
    this.alertService.startDelayedMessage("Saving changes...");
    if (this.isNewPosition) {

      this.positionService.newPosition(this.positionEdit).subscribe(pos => this.saveSuccessHelper(pos), error => this.saveFailedHelper(error));
    }
    else {
      this.positionService.updatePosition(this.positionEdit).subscribe(response => this.saveSuccessHelper(), error => this.saveFailedHelper(error));
    }
  }

  private saveSuccessHelper(position?: Position) {

    if (position)
      Object.assign(this.positionEdit, position);

    this.isSaving = false;
    this.alertService.stopLoadingMessage();
    this.isChangePassword = false;
    this.showValidationErrors = false;

    Object.assign(this.position, this.positionEdit);
    this.positionEdit = new Position();
    this.resetForm();

    if( this.isNewPosition )
      this.alertService.showMessage("Success", `Position \"${this.position.name}\" was created successfully`, MessageSeverity.success);
    else
      this.alertService.showMessage("Success", `Changes to position \"${this.position.name}\" were saved successfully`, MessageSeverity.success);

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
    this.positionEdit = this.position = new Position();

    this.showValidationErrors = false;
    this.resetForm();

    this.alertService.showOperationCancelledMessage();
    this.alertService.resetStickyMessage();

    if (this.changesCancelledCallback)
      this.changesCancelledCallback();
  }

  private close() {
    this.positionEdit = this.position = new Position();
    this.showValidationErrors = false;
    this.resetForm();
    this.isEditMode = false;

    if (this.changesSavedCallback)
      this.changesSavedCallback();
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

  newPosition() {
    this.isNewPosition = true;
    this.editingPositionName = null;
    this.position = this.positionEdit = new Position();
    this.edit();

    return this.positionEdit;
  }

  editPosition(position: Position) {
    if (position) {
      this.isNewPosition = false;

      this.editingPositionName = position.name;
      this.position = new Position();
      this.positionEdit = new Position();
      Object.assign(this.position, position);
      Object.assign(this.positionEdit, position);
      this.edit();

      return this.positionEdit;
    }
    else {
      return this.newPosition();
    }
  }

  displayPosition(position: Position) {

    this.position = new Position();
    Object.assign(this.position, position);

    this.isEditMode = false;
  }
}
