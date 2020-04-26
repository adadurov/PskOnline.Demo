
import { Component, OnInit, AfterViewInit, TemplateRef, ViewChild, Input } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap/modal';

import { AlertService, DialogType, MessageSeverity } from '../../services/alert.service';
import { AppTranslationService } from "../../services/app-translation.service";
import { AccountService } from "../../services/account.service";
import { Utilities } from "../../services/utilities";
import { Role } from '../../models/role.model';
import { Permission } from '../../models/permission.model';
import { User } from '../../models/user.model';
import { UserDisplay } from '../../models/user-display.model';
import { UserEdit } from '../../models/user-edit.model';
import { UserEditDisplay } from '../../models/user-edit-display.model';
import { UserInfoComponent } from "./user-info.component";


@Component({
    selector: 'users-management',
    templateUrl: './users-management.component.html',
    styleUrls: ['./users-management.component.css']
})
export class UsersManagementComponent implements OnInit, AfterViewInit {
    columns: any[] = [];
    rows: UserDisplay[] = [];
    rowsCache: UserDisplay[] = [];
    editedUser: UserEditDisplay;
    sourceUser: UserEditDisplay;
    editingUserName: { name: string };
    loadingIndicator: boolean;

    allRoles: Role[] = [];


    @ViewChild('indexTemplate')
    indexTemplate: TemplateRef<any>;

    @ViewChild('userNameTemplate')
    userNameTemplate: TemplateRef<any>;

    @ViewChild('rolesTemplate')
    rolesTemplate: TemplateRef<any>;

    @ViewChild('actionsTemplate')
    actionsTemplate: TemplateRef<any>;

    @ViewChild('editorModal')
    editorModal: ModalDirective;

    @ViewChild('userEditor')
    userEditor: UserInfoComponent;

    constructor(private alertService: AlertService, private translationService: AppTranslationService, private accountService: AccountService) {
    }


    ngOnInit() {

        let gT = (key: string) => this.translationService.getTranslation(key);

        this.columns = [
            { prop: "index", name: '#', width: 40, cellTemplate: this.indexTemplate, canAutoResize: false },
          { prop: 'userName', name: gT('users.mgmt.UserName'), width: 90, cellTemplate: this.userNameTemplate },
          { prop: 'fullUserName', name: gT('users.mgmt.FullName'), width: 120 },
          { prop: 'email', name: gT('users.mgmt.Email'), width: 140 },
          { prop: 'roles', name: gT('users.mgmt.Roles'), width: 120, cellTemplate: this.rolesTemplate },
          { prop: 'phoneNumber', name: gT('users.mgmt.PhoneNumber'), width: 100 }
        ];

        if (this.canManageUsers)
            this.columns.push({ name: '', width: 180, cellTemplate: this.actionsTemplate, resizeable: false, canAutoResize: false, sortable: false, draggable: false });

        this.loadData();
    }


    ngAfterViewInit() {

        this.userEditor.changesSavedCallback = () => {
            this.addNewUserToList();
            this.editorModal.hide();
        };

        this.userEditor.changesCancelledCallback = () => {
            this.editedUser = null;
            this.sourceUser = null;
            this.editorModal.hide();
        };
    }


    addNewUserToList() {
        if (this.sourceUser) {
            Object.assign(this.sourceUser, this.editedUser);

            let sourceIndex = this.rowsCache.indexOf(this.sourceUser, 0);
            if (sourceIndex > -1)
                Utilities.moveArrayItem(this.rowsCache, sourceIndex, 0);

            sourceIndex = this.rows.indexOf(this.sourceUser, 0);
            if (sourceIndex > -1)
                Utilities.moveArrayItem(this.rows, sourceIndex, 0);

            this.editedUser = null;
            this.sourceUser = null;
        }
        else {
            let user = new UserDisplay();
            Object.assign(user, this.editedUser);
            this.editedUser = null;

            let maxIndex = 0;
            for (let u of this.rowsCache) {
                if ((<any>u).index > maxIndex)
                    maxIndex = (<any>u).index;
            }

            (<any>user).index = maxIndex + 1;

            this.rowsCache.splice(0, 0, user);
            this.rows.splice(0, 0, user);
        }
    }


    loadData() {
      this.alertService.startDefaultLoadingMessage();
        this.loadingIndicator = true;

        if (this.canViewRoles) {
          this.accountService.getUsersAndRoles().subscribe(
            results => this.onDataLoadSuccessful(results[0], results[1]),
            error => this.onDataLoadFailed(error));
        }
        else {
          this.accountService.getUsers().subscribe(
            users => this.onDataLoadSuccessful(users, this.accountService.currentUser.roles.map(x => new Role(x.id, x.name))),
            error => this.onDataLoadFailed(error));
        }
    }


    onDataLoadSuccessful(users: User[], roles: Role[]) {
        this.alertService.stopLoadingMessage();
        this.loadingIndicator = false;

        // convert loaded data to view model
        var displayUsers: UserDisplay[];

        displayUsers = users.map((user, index, users) => {
            var userDisplay = new UserDisplay();
            Object.assign(userDisplay, user);
            userDisplay.index = index + 1;
            return userDisplay;
        });

        this.rowsCache = [...displayUsers];
        this.rows = displayUsers;

        this.allRoles = roles;
    }


    onDataLoadFailed(error: any) {
        this.alertService.stopLoadingMessage();
        this.loadingIndicator = false;

        this.alertService.showStickyMessage("Load Error", `Unable to retrieve users from the server.\r\nErrors: "${Utilities.getHttpResponseMessage(error)}"`,
            MessageSeverity.error, error);
    }


    onSearchChanged(value: string) {
        this.rows = this.rowsCache.filter(r => Utilities.searchArray(value, false, r.userName, r.fullUserName, r.email, r.phoneNumber, r.jobTitle, r.roles.map(r=>r.name)));
    }

    onEditorModalHidden() {
        this.editingUserName = null;
        this.userEditor.resetForm(true);
    }


    newUser() {
        this.editingUserName = null;
        this.sourceUser = null;
        this.editedUser = this.userEditor.newUser(this.allRoles);
        this.editorModal.show();
    }


    editUser(row: UserEditDisplay) {
        this.editingUserName = { name: row.userName };
        this.sourceUser = row;
        this.editedUser = this.userEditor.editUser(row, this.allRoles);
        this.editorModal.show();
    }


  deleteUser(row: UserEditDisplay) {
    this.alertService.showDeleteWarning("users.mgmt.userAccusativeCase", row.userName , () => this.doDeleteUser(row));
  }


    doDeleteUser(row: UserEditDisplay) {

        this.alertService.startDelayedMessage("Deleting...");
        this.loadingIndicator = true;

        this.accountService.deleteUser(row)
            .subscribe(results => {
                this.alertService.stopLoadingMessage();
                this.loadingIndicator = false;

                this.rowsCache = this.rowsCache.filter(item => item !== row)
                this.rows = this.rows.filter(item => item !== row)
            },
            error => {
                this.alertService.stopLoadingMessage();
                this.loadingIndicator = false;

                this.alertService.showStickyMessage("Delete Error", `An error occured while deleting the user.\r\nError: "${Utilities.getHttpResponseMessage(error)}"`,
                    MessageSeverity.error, error);
            });
    }



    get canAssignRoles() {
      return this.accountService.userHasPermission(Permission.assignGlobalRolesPermission) ||
        this.accountService.userHasPermission(Permission.assignTenantRolesPermission);
    }

    get canViewRoles() {
      return this.accountService.userHasPermission(Permission.viewGlobalRolesPermission) ||
        this.accountService.userHasPermission(Permission.viewTenantRolesPermission);
    }

    get canManageUsers() {
      return this.accountService.userHasPermission(Permission.manageGlobalUsersPermission) ||
        this.accountService.userHasPermission(Permission.manageTenantUsersPermission);
    }

    get canViewTenants() {
      return this.accountService.userHasPermission(Permission.viewTenantsPermission);
    }

    get canManageTenants() {
      return this.accountService.userHasPermission(Permission.manageTenantsPermission);
    }

}
