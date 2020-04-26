import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from "@angular/router";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { TenantEndpoint } from './tenant-endpoint.service';
import { AuthService } from './auth.service';
import { Tenant } from '../models/tenant.model';
import { TenantCreate } from '../models/tenant-create.model';
import { TenantOperationsSummary } from '../models/tenant-operations-summary.model';
import { TenantSharedInfo } from '../models/tenant-shared-info.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';


@Injectable()
export class TenantService {

  constructor(private router: Router, private http: HttpClient, private authService: AuthService,
    private tenantEndpoint: TenantEndpoint) {

  }

  getTenant(tenantId?: string): Observable<Tenant> {
    return this.tenantEndpoint.getTenantEndpoint(tenantId);
  }

  getTenantOperationsSummary(tenantId: string): Observable<TenantOperationsSummary> {
    return this.tenantEndpoint.getTenantOperationsSummary(tenantId);
  }

  getTenantSharedInfo(tenantId: string): Observable<TenantSharedInfo> {
    return this.tenantEndpoint.getTenantSharedInfo(tenantId);
  }

  unblockTenant(tenantId?: string) {
    return this.tenantEndpoint.getUnblockTenantEndpoint<any>(tenantId);
  }

  getTenants(page?: number, pageSize?: number): Observable<Tenant[]> {

    return this.tenantEndpoint.getTenants(page, pageSize);
  }

  newTenant(tenant: TenantCreate): Observable<CreatedWithGuid> {
    return this.tenantEndpoint.getNewTenantEndpoint(tenant);
  }

  updateTenant(tenant: Tenant) {
    return this.tenantEndpoint.getUpdateTenantEndpoint(tenant, tenant.id);
  }

  deleteTenant(tenantOrTenantId: string | Tenant): Observable<Tenant> {

    if (typeof tenantOrTenantId === 'string' || tenantOrTenantId instanceof String) {
      return this.tenantEndpoint.getDeleteTenantEndpoint<Tenant>(<string>tenantOrTenantId);
    }
    else {

      if (tenantOrTenantId.id) {
        return this.deleteTenant(tenantOrTenantId.id);
      }
      else {
        ;
      }
    }
  }
}