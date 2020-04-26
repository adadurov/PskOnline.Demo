import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { Department } from '../models/department.model';
import { Employee } from '../models/employee.model';
import { DepartmentWorkplace } from '../models/department-workplace.model';
import { DepartmentWorkplaceCredentials } from '../models/department-workplace-credentials.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';

@Injectable()
export class DepartmentEndpoint extends EndpointFactory {

  private readonly _url: string = "/api/Department";

  get fullUrl() { return this.configurations.baseUrl + this._url; }


  constructor(http: HttpClient, configurations: ConfigurationService, injector: Injector) {

    super(http, configurations, injector);
  }

  getDepartmentEndpoint(departmentId: string): Observable<Department> {
    let endpointUrl = `${this.fullUrl}/${departmentId}`;

    return this.http.get<Department>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDepartmentEndpoint(departmentId));
      });
  }

  getEmployeesInDepartment(departmentId: string): Observable<Employee[]> {
    let endpointUrl = `${this.fullUrl}/${departmentId}/employees`;
    return this.http.get<Employee[]>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getEmployeesInDepartment(departmentId));
      });
  }

  getDepartments(page?: number, pageSize?: number): Observable<Department[]> {
    let endpointUrl = page && pageSize ? `${this.fullUrl}/${page}/${pageSize}` : this.fullUrl;

    return this.http.get<Department[]>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDepartments(page, pageSize));
      });
  }

  getNewDepartmentEndpoint(departmentObject: Department): Observable<CreatedWithGuid> {
    return this.http.post<CreatedWithGuid>(this.fullUrl, JSON.stringify(departmentObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getNewDepartmentEndpoint(departmentObject));
      });
  }

  postNewWorkplace(departmentId: string, workplaceDescriptor: DepartmentWorkplace): Observable<DepartmentWorkplaceCredentials> {

    let endpointUrl = `${this.fullUrl}/${departmentId}/workplace`;
    return this.http.post<DepartmentWorkplaceCredentials>(endpointUrl, JSON.stringify(workplaceDescriptor), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.postNewWorkplace(departmentId, workplaceDescriptor));
      });
  }

  getUpdateDepartmentEndpoint(departmentObject: any, departmentId: string): Observable<any> {
    let endpointUrl = departmentId ? `${this.fullUrl}/${departmentId}` : null;

    return this.http.put<any>(endpointUrl, JSON.stringify(departmentObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUpdateDepartmentEndpoint(departmentObject, departmentId));
      });
  }

  getPatchUpdateDepartmentEndpoint(patch: {}, departmentId: string): Observable<any>
  getPatchUpdateDepartmentEndpoint(value: any, op: string, path: string, departmentId: string, from?: any): Observable<any>
  getPatchUpdateDepartmentEndpoint(valueOrPatch: any, departmentId: string, opOrDepartmentId?: string, path?: string, from?: any): Observable<any> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = `${this.fullUrl}/${departmentId}`;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrDepartmentId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrDepartmentId }];
    }
    else {
      endpointUrl = `${this.fullUrl}/${opOrDepartmentId}`;
      patchDocument = valueOrPatch;
    }

    return this.http.patch(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPatchUpdateDepartmentEndpoint(valueOrPatch, opOrDepartmentId, path, from, departmentId));
      });
  }

  getDeleteDepartmentEndpoint(departmentId: string): Observable<any> {
    let endpointUrl = `${this.fullUrl}/${departmentId}`;

    return this.http.delete(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDeleteDepartmentEndpoint(departmentId));
      });
  }
}