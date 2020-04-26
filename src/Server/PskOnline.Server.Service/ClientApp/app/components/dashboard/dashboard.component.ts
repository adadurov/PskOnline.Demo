
import { Component } from '@angular/core';
import { fadeInOut } from '../../services/animations';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.css'],
    animations: [fadeInOut]
})
export class DashboardComponent {

  isTenantUser: boolean;
  isHostUser: boolean;

  constructor(private authService: AuthService) {
    var tenantId = this.authService.lastLoggedInUser.tenantId;
    this.isHostUser = tenantId == "00000000-0000-0000-0000-000000000000";
    this.isTenantUser = !this.isHostUser;

  }
}
