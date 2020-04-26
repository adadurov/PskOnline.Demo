import { fadeInOut } from '../../services/animations';
import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import 'rxjs/add/operator/switchMap';

import { BootstrapTabDirective } from "../../directives/bootstrap-tab.directive";
import { AccountService } from "../../services/account.service";
import { Permission, PermissionValues } from '../../models/permission.model';


@Component({
    selector: 'configuration',
    templateUrl: './configuration.component.html',
    styleUrls: ['./configuration.component.css'],
    animations: [fadeInOut]
})
export class ConfigurationComponent implements OnInit, OnDestroy {

    isTenantsActivated = false;
    isEmployeesActivated = false;
    isOrgStructureActivated = false;
    isPositionsActivated = false;
    isUsersActivated = false;
    isRolesActivated = false;

    fragmentSubscription: any;

    readonly tenantsTab = "tenants";
    readonly employeesTab = "employees";
    readonly orgStructureTab = "orgstructure";
    readonly positionsTab = "positions";
    readonly usersTab = "users";
    readonly rolesTab = "roles";
    
    @ViewChild("tab")
    tab: BootstrapTabDirective;


    constructor(private route: ActivatedRoute, private accountService: AccountService) {
    }


    ngOnInit() {
        this.fragmentSubscription = this.route.fragment.subscribe(anchor => this.showContent(anchor));
    }


    ngOnDestroy() {
        this.fragmentSubscription.unsubscribe();
    }

    showContent(anchor: string) {
        if ((this.isFragmentEquals(anchor, this.tenantsTab) && !this.canViewTenants) ||
            (this.isFragmentEquals(anchor, this.employeesTab) && !this.canViewEmployees) ||
            (this.isFragmentEquals(anchor, this.orgStructureTab) && !this.canViewOrgStructure) ||
            (this.isFragmentEquals(anchor, this.usersTab) && !this.canViewUsers) ||
            (this.isFragmentEquals(anchor, this.rolesTab) && !this.canViewRoles)
            )
            return;

        this.tab.show(`#${anchor || this.orgStructureTab}Tab`);
    }


    isFragmentEquals(fragment1: string, fragment2: string) {

        if (fragment1 == null)
            fragment1 = "";

        if (fragment2 == null)
            fragment2 = "";

        return fragment1.toLowerCase() == fragment2.toLowerCase();
    }


    onShowTab(event) {
        let activeTab = event.target.hash.split("#", 2).pop();

        this.isTenantsActivated = activeTab == this.tenantsTab;
        this.isEmployeesActivated = activeTab == this.employeesTab;
        this.isOrgStructureActivated = activeTab == this.orgStructureTab;
        this.isPositionsActivated = activeTab == this.positionsTab;
        this.isUsersActivated = activeTab == this.usersTab;
        this.isRolesActivated = activeTab == this.rolesTab;
    }

    userHasPermission(permissions: PermissionValues): boolean {
      return this.accountService.userHasPermission(permissions);
    }

    get canViewUsers() {
      return this.userHasPermission(Permission.viewGlobalUsersPermission) ||
        this.userHasPermission(Permission.viewTenantUsersPermission);
    }

    get canViewRoles() {
      return this.userHasPermission(Permission.viewGlobalRolesPermission) ||
        this.userHasPermission(Permission.viewTenantRolesPermission);
    }

    get canViewPositions() {
      return this.userHasPermission(Permission.viewTenantOrgStructurePermission) ||
        this.userHasPermission(Permission.manageTenantOrgStructurePermission);
    }

    get canViewOrgStructure() {
        return this.userHasPermission(Permission.viewTenantOrgStructurePermission) ||
            this.userHasPermission(Permission.manageTenantOrgStructurePermission);
    }

    get canViewEmployees() {
        return this.userHasPermission(Permission.viewTenantOrgStructurePermission) ||
             this.userHasPermission(Permission.manageTenantOrgStructurePermission);
    }

    get canViewTenants() {
      return this.userHasPermission(Permission.viewTenantsPermission);
    }

}
