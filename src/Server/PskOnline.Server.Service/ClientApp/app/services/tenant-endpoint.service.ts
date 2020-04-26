import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { TenantCreate } from '../models/tenant-create.model';
import { Tenant } from '../models/tenant.model';
import { TenantOperationsSummary } from '../models/tenant-operations-summary.model';
import { TenantSharedInfo } from '../models/tenant-shared-info.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';


@Injectable()
export class TenantEndpoint extends EndpointFactory {

  private readonly _tenantUrl: string = "/api/tenant";

  get tenantUrl() { return this.configurations.baseUrl + this._tenantUrl; }


  constructor(http: HttpClient, configurations: ConfigurationService, injector: Injector) {

    super(http, configurations, injector);
  }
  
  getTenantEndpoint(tenantId: string): Observable<Tenant> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}`;

    return this.http.get<Tenant>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getTenantEndpoint(tenantId));
      });
  }
  
  getTenantSharedInfo(tenantId: string): Observable<TenantSharedInfo> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}/shared-info`;

    return this.http.get<TenantSharedInfo>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getTenantSharedInfo(tenantId));
      });
  }

  getTenantOperationsSummary(tenantId: string): Observable<TenantOperationsSummary> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}/operations-summary`;

    return this.http.get<TenantOperationsSummary>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getTenantOperationsSummary(tenantId));
      });
  }

  getTenants(page?: number, pageSize?: number): Observable<Tenant[]> {
    let endpointUrl = page && pageSize ? `${this.tenantUrl}/${page}/${pageSize}` : this.tenantUrl;

    return this.http.get<Tenant[]>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getTenants(page, pageSize));
      });
  }

  getNewTenantEndpoint(tenantObject: TenantCreate): Observable<CreatedWithGuid> {

    return this.http.post<CreatedWithGuid>(this.tenantUrl, JSON.stringify(tenantObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getNewTenantEndpoint(tenantObject));
      });
  }

  getUpdateTenantEndpoint<T>(tenantObject: any, tenantId: string): Observable<T> {
    let endpointUrl = tenantId ? `${this.tenantUrl}/${tenantId}` : null;

    return this.http.put<T>(endpointUrl, JSON.stringify(tenantObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUpdateTenantEndpoint(tenantObject, tenantId));
      });
  }

  getPatchUpdateTenantEndpoint<T>(patch: {}, tenantId: string): Observable<T>
  getPatchUpdateTenantEndpoint<T>(value: any, op: string, path: string, tenantId: string, from?: any): Observable<T>
  getPatchUpdateTenantEndpoint<T>(valueOrPatch: any, tenantId: string, opOrTenantId?: string, path?: string, from?: any): Observable<T> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = `${this.tenantUrl}/${tenantId}`;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrTenantId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrTenantId }];
    }
    else {
      endpointUrl = `${this.tenantUrl}/${opOrTenantId}`;
      patchDocument = valueOrPatch;
    }

    return this.http.patch<T>(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPatchUpdateTenantEndpoint(valueOrPatch, opOrTenantId, path, from, tenantId));
      });
  }

  getBlockTenantEndpoint<T>(tenantId: string): Observable<T> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}/block`;

    return this.http.put<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getBlockTenantEndpoint(tenantId));
      });
  }

  getUnblockTenantEndpoint<T>(tenantId: string): Observable<T> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}/unblock`;

    return this.http.put<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUnblockTenantEndpoint(tenantId));
      });
  }

  getDeleteTenantEndpoint<T>(tenantId: string): Observable<T> {
    let endpointUrl = `${this.tenantUrl}/${tenantId}`;

    return this.http.delete<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDeleteTenantEndpoint(tenantId));
      });
  }
}