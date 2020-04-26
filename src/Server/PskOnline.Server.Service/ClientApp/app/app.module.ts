
import { NgModule, ErrorHandler } from "@angular/core";
import { RouterModule } from "@angular/router";
import { FormsModule } from "@angular/forms";
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';

import 'bootstrap';
import { TranslateModule, TranslateLoader } from "@ngx-translate/core";
import { NgxDatatableModule } from '@swimlane/ngx-datatable';
import { ToastyModule } from 'ng2-toasty';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TooltipModule } from "ngx-bootstrap/tooltip";
import { PopoverModule } from "ngx-bootstrap/popover";
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { CarouselModule } from 'ngx-bootstrap/carousel';

import { AppRoutingModule } from './app-routing.module';
import { AppErrorHandler } from './app-error.handler';
import { AppTitleService } from './services/app-title.service';
import { AppTranslationService, TranslateLanguageLoader } from './services/app-translation.service';
import { ConfigurationService } from './services/configuration.service';
import { AlertService } from './services/alert.service';
import { LocalStoreManager } from './services/local-store-manager.service';
import { EndpointFactory } from './services/endpoint-factory.service';
import { AccountService } from './services/account.service';
import { AccountEndpoint } from './services/account-endpoint.service';
import { TenantService } from './services/tenant.service';
import { TenantEndpoint } from './services/tenant-endpoint.service';
import { EmployeeService } from './services/employee.service';
import { EmployeeEndpoint } from './services/employee-endpoint.service';
import { PositionService } from './services/position.service';
import { PositionEndpoint } from './services/position-endpoint.service';
import { DepartmentService } from './services/department.service';
import { DepartmentEndpoint } from './services/department-endpoint.service';

import { BranchOfficeService } from './services/branch-office.service';
import { BranchOfficeEndpoint } from './services/branch-office-endpoint.service';

import { EqualValidator } from './directives/equal-validator.directive';
import { LastElementDirective } from './directives/last-element.directive';
import { AutofocusDirective } from './directives/autofocus.directive';
import { BootstrapTabDirective } from './directives/bootstrap-tab.directive';
import { BootstrapToggleDirective } from './directives/bootstrap-toggle.directive';
import { BootstrapSelectDirective } from './directives/bootstrap-select.directive';
import { BootstrapDatepickerDirective } from './directives/bootstrap-datepicker.directive';
import { GroupByPipe } from './pipes/group-by.pipe';

import { AppComponent } from "./components/app.component";
import { LoginComponent } from "./components/login/login.component";

import { OrgStructureManagementComponent } from "./components/orgstructure/orgstructure-management.component";
import { EmployeesManagementComponent } from "./components/employees/employees-management.component";
import { EmployeeInfoComponent } from "./components/employees/employee-info.component";
import { PositionsManagementComponent } from "./components/positions/positions-management.component";
import { PositionInfoComponent } from "./components/positions/position-info.component";

import { SettingsComponent } from "./components/settings/settings.component";
import { ConfigurationComponent } from "./components/configuration/configuration.component";
import { NotFoundComponent } from "./components/not-found/not-found.component";
import { TenantCreateComponent } from "./components/tenants/tenant-create.component";
import { TenantsManagementComponent } from "./components/tenants/tenants-management.component";
import { TenantInfoComponent } from "./components/tenants/tenant-info.component";
import { DepartmentCreateComponent } from "./components/department/department-create.component";
import { BranchOfficeInfoComponent } from "./components/branch-office/branch-office-info.component";
import { UserInfoComponent } from "./components/controls/user-info.component";
import { UserPreferencesComponent } from "./components/controls/user-preferences.component";
import { UsersManagementComponent } from "./components/controls/users-management.component";
import { RolesManagementComponent } from "./components/controls/roles-management.component";
import { RoleEditorComponent } from "./components/controls/role-editor.component";
import { ForgotPasswordComponent } from "./components/forgot-password/forgot-password.component";

import { SearchBoxComponent } from "./components/controls/search-box.component";
import { TenantDashboardComponent } from "./components/tenantdashboard/tenant-dashboard.component";
import { DashboardComponent } from "./components/dashboard/dashboard.component";

import { TimeZoneService } from "./services/time-zone.service";
import { TreeModule } from 'angular-tree-component';
import { WindowLocationService } from "./services/window-location.service";
import { PwdResetComponent } from "./components/pwd-reset/pwd-reset.component";


export function getHost() {
  return window.location.host;
}

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}


@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    AppRoutingModule,
    TreeModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useClass: TranslateLanguageLoader
      }
    }),
    NgxDatatableModule,
    ToastyModule.forRoot(),
    TooltipModule.forRoot(),
    PopoverModule.forRoot(),
    BsDropdownModule.forRoot(),
    CarouselModule.forRoot(),
    ModalModule.forRoot()
  ],
  declarations: [
    AppComponent,
    LoginComponent, ForgotPasswordComponent,
    PwdResetComponent,
    TenantsManagementComponent,
    TenantInfoComponent, TenantCreateComponent,
    OrgStructureManagementComponent,
    DepartmentCreateComponent, BranchOfficeInfoComponent,
    PositionInfoComponent, PositionsManagementComponent,
    EmployeeInfoComponent, EmployeesManagementComponent,
    SettingsComponent,
    TenantDashboardComponent,
    DashboardComponent,
    ConfigurationComponent,
    UsersManagementComponent, UserInfoComponent, UserPreferencesComponent,
    RolesManagementComponent, RoleEditorComponent,
    NotFoundComponent,
    SearchBoxComponent,
    EqualValidator,
    LastElementDirective,
    AutofocusDirective,
    BootstrapTabDirective,
    BootstrapToggleDirective,
    BootstrapSelectDirective,
    BootstrapDatepickerDirective,
    GroupByPipe
  ],
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl },
    { provide: ErrorHandler, useClass: AppErrorHandler },
    AlertService,
    TimeZoneService,
    ConfigurationService,
    AppTitleService,
    AppTranslationService,
    AccountService,
    AccountEndpoint,
    TenantService,
    TenantEndpoint,
    EmployeeService,
    EmployeeEndpoint,
    PositionService,
    PositionEndpoint,
    DepartmentService,
    DepartmentEndpoint,
    BranchOfficeService,
    BranchOfficeEndpoint,
    LocalStoreManager,
    EndpointFactory,
    WindowLocationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
