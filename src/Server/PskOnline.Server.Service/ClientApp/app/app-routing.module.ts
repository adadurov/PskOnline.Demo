
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { LoginComponent } from "./components/login/login.component";
import { OrgStructureManagementComponent } from "./components/orgstructure/orgstructure-management.component";
import { EmployeesManagementComponent } from "./components/employees/employees-management.component";
import { SettingsComponent } from "./components/settings/settings.component";
import { DashboardComponent } from "./components/dashboard/dashboard.component";
import { ConfigurationComponent } from "./components/configuration/configuration.component";
import { PositionsManagementComponent } from "./components/positions/positions-management.component";
import { NotFoundComponent } from "./components/not-found/not-found.component";
import { AuthService } from './services/auth.service';
import { AuthGuard } from './services/auth-guard.service';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';
import { PwdResetComponent } from './components/pwd-reset/pwd-reset.component';



@NgModule({
  imports: [
    RouterModule.forRoot([
      { path: "", component: DashboardComponent, canActivate: [AuthGuard], data: { title: "Общие сведения" } },
      { path: "login", component: LoginComponent, data: { title: "Вход в систему" } },
      { path: "forgot-password/:nameOrEmail", component: ForgotPasswordComponent, data: { title: "Забытый пароль" } },
      { path: "forgot-password", component: ForgotPasswordComponent, data: { title: "Забытый пароль" } },
      { path: "password-reset", component: PwdResetComponent, data: { title: "Установка нового пароля" } },
      //{ path: "password-reset-completed", component: PwdResetCompletedComponent, data: { title: "Новый пароль установлен" } },
      //{ path: "password-reset-error", component: PwdResetErrorComponent, data: { title: "Ошибка установки нового пароля" } },
      { path: "settings", component: SettingsComponent, canActivate: [AuthGuard], data: { title: "Мой профиль" } },
      { path: "configuration", component: ConfigurationComponent, canActivate: [AuthGuard], data: { title: "Конфигурация" } },
      { path: "home", redirectTo: "/", pathMatch: "full" },
      { path: "**", component: NotFoundComponent, data: { title: "Страница не найдена" } },
    ],
    { onSameUrlNavigation: 'reload' }
    )
  ],
  exports: [
    RouterModule
  ],
  providers: [
    AuthService, AuthGuard
  ]
})
export class AppRoutingModule { }