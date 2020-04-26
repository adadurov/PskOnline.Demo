import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { BranchOffice } from '../models/branch-office.model';


@Injectable()
export class BranchOfficeEndpoint extends EndpointFactory {

  private readonly _url: string = "/api/BranchOffice";

  get fullUrl() { return this.configurations.baseUrl + this._url; }

  constructor(
    http: HttpClient,
    configurations: ConfigurationService,
    injector: Injector) {

    super(http, configurations, injector);
  }

  getBranchOfficeEndpoint<T>(branchOfficeId: string): Observable<T> {
    let endpointUrl = `${this.fullUrl}/${branchOfficeId}`;

    return this.http.get<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getBranchOfficeEndpoint(branchOfficeId));
      });
  }

  getBranchOffices<T>(page?: number, pageSize?: number): Observable<T> {
    let endpointUrl = page && pageSize ? `${this.fullUrl}/${page}/${pageSize}` : this.fullUrl;

    return this.http.get<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getBranchOffices(page, pageSize));
      });
  }

  getNewBranchOfficeEndpoint<T>(branchOfficeObject: BranchOffice): Observable<T> {

    return this.http.post<T>(this.fullUrl, JSON.stringify(branchOfficeObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getNewBranchOfficeEndpoint(branchOfficeObject));
      });
  }

  getUpdateBranchOfficeEndpoint<T>(branchOfficeObject: any, branchOfficeId: string): Observable<T> {
    let endpointUrl = branchOfficeId ? `${this.fullUrl}/${branchOfficeId}` : null;

    return this.http.put<T>(endpointUrl, JSON.stringify(branchOfficeObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUpdateBranchOfficeEndpoint(branchOfficeObject, branchOfficeId));
      });
  }

  getPatchUpdateBranchOfficeEndpoint<T>(patch: {}, branchOfficeId: string): Observable<T>
  getPatchUpdateBranchOfficeEndpoint<T>(value: any, op: string, path: string, branchOfficeId: string, from?: any): Observable<T>
  getPatchUpdateBranchOfficeEndpoint<T>(valueOrPatch: any, branchOfficeId: string, opOrBranchOfficeId?: string, path?: string, from?: any): Observable<T> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = `${this.fullUrl}/${branchOfficeId}`;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrBranchOfficeId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrBranchOfficeId }];
    }
    else {
      endpointUrl = `${this.fullUrl}/${opOrBranchOfficeId}`;
      patchDocument = valueOrPatch;
    }

    return this.http.patch<T>(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPatchUpdateBranchOfficeEndpoint(valueOrPatch, opOrBranchOfficeId, path, from, branchOfficeId));
      });
  }

  getDeleteBranchOfficeEndpoint<T>(branchOfficeId: string): Observable<T> {
    let endpointUrl = `${this.fullUrl}/${branchOfficeId}`;

    return this.http.delete<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDeleteBranchOfficeEndpoint(branchOfficeId));
      });
  }
}