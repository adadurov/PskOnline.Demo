
import { Component, ViewChild } from '@angular/core';

import { AlertService, MessageSeverity } from '../../services/alert.service';
import { AccountService } from "../../services/account.service";
import { Role } from '../../models/role.model';
import { Permission } from '../../models/permission.model';


@Component({
    selector: 'role-editor',
    templateUrl: './role-editor.component.html',
    styleUrls: ['./role-editor.component.css']
})
export class RoleEditorComponent {

    private isNewRole = false;
    private isSaving: boolean;
    private showValidationErrors: boolean = true;
    private editingRoleId: string;
    private roleEdit: Role = new Role(null, "");
    private allPermissions: Permission[] = [];
    private selectedValues: { [key: string]: boolean; } = {};

    public formResetToggle = true;

    public changesSavedCallback: () => void;
    public changesFailedCallback: () => void;
    public changesCancelledCallback: () => void;


    @ViewChild('f')
    private form;

    constructor(private alertService: AlertService, private accountService: AccountService) {
    }

    private showErrorAlert(caption: string, message: string) {
        this.alertService.showMessage(caption, message, MessageSeverity.error);
    }

    private save() {
        this.isSaving = true;
        this.alertService.startDelayedMessage("Saving changes...");

        this.roleEdit.permissions = this.getSelectedPermissions();

        if (this.isNewRole) {
            this.accountService.newRole(this.roleEdit).subscribe(role => this.saveSuccessHelper(role), error => this.saveFailedHelper(error));
        }
        else {
            this.accountService.updateRole(this.roleEdit).subscribe(response => this.saveSuccessHelper(), error => this.saveFailedHelper(error));
        }
    }

    private saveSuccessHelper(role?: Role) {
        if (role)
            Object.assign(this.roleEdit, role);

        this.isSaving = false;
        this.alertService.stopLoadingMessage();
        this.showValidationErrors = false;

        if (this.isNewRole)
          this.alertService.showMessage(
            "Success",
            `Role \"${this.roleEdit.name}\" was created successfully`,
            MessageSeverity.success);
        else
          this.alertService.showMessage(
            "Success",
            `Changes to role \"${this.roleEdit.name}\" was saved successfully`,
            MessageSeverity.success);

        this.roleEdit = new Role(null, "");
        this.resetForm();


        if (!this.isNewRole && this.accountService.currentUser.roles.some(r => r.id == this.editingRoleId))
            this.refreshLoggedInUser();

        if (this.changesSavedCallback)
            this.changesSavedCallback();
    }

    private refreshLoggedInUser() {
        this.accountService.refreshLoggedInUser()
            .subscribe(user => { },
            error => {
              this.alertService.resetStickyMessage();
              this.alertService.showStickyMessage(
                "Refresh failed",
                "An error occured whilst refreshing logged in user information from the server",
                MessageSeverity.error, error);
            });
    }

    private saveFailedHelper(error: any) {
        this.isSaving = false;
        this.alertService.stopLoadingMessage();
      this.alertService.showStickyMessage(
        "Save Error",
        "The below errors occured whilst saving your changes:",
        MessageSeverity.error,
        error);
        this.alertService.showStickyMessage(error, null, MessageSeverity.error);

        if (this.changesFailedCallback)
            this.changesFailedCallback();
    }

    private cancel() {
        this.roleEdit = new Role(null, "");

        this.showValidationErrors = false;
        this.resetForm();

      this.alertService.showOperationCancelledMessage();

        this.alertService.resetStickyMessage();

        if (this.changesCancelledCallback)
            this.changesCancelledCallback();
    }

    private selectAll() {
        this.allPermissions.forEach(p => this.selectedValues[p.value] = true);
    }

    private selectNone() {
        this.allPermissions.forEach(p => this.selectedValues[p.value] = false);
    }

    private toggleGroup(groupName: string) {
        let firstMemberValue: boolean;

        this.allPermissions.forEach(p => {
            if (p.groupName != groupName)
                return;

            if (firstMemberValue == null)
                firstMemberValue = this.selectedValues[p.value] == true;

            this.selectedValues[p.value] = !firstMemberValue;
        });
    }

    private getSelectedPermissions() {
        return this.allPermissions.filter(p => this.selectedValues[p.value] == true);
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

    newRole(allPermissions: Permission[]) {
        this.isNewRole = true;
        this.showValidationErrors = true;

        this.editingRoleId = null;
        this.allPermissions = allPermissions;
        this.selectedValues = {};
        this.roleEdit = new Role(null, "");

        return this.roleEdit;
    }

    editRole(role: Role, allPermissions: Permission[]) {
        if (role) {
            this.isNewRole = false;
            this.showValidationErrors = true;

            this.editingRoleId = role.name;
            this.allPermissions = allPermissions;
            this.selectedValues = {};
            console.info(role);
            role.permissions.forEach(p => this.selectedValues[p.value] = true);
            this.roleEdit = new Role(null, "");
            Object.assign(this.roleEdit, role);

            return this.roleEdit;
        }
        else {
            return this.newRole(allPermissions);
        }
    }

    get canManageRoles() {
      return this.accountService.userHasPermission(Permission.manageGlobalRolesPermission) ||
        this.accountService.userHasPermission(Permission.manageTenantRolesPermission);
    }
}
