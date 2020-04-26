import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { Employee } from '../models/employee.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';


@Injectable()
export class EmployeeEndpoint extends EndpointFactory {

  private readonly _employeeUrl: string = "/api/Employee";

  get employeeUrl() { return this.configurations.baseUrl + this._employeeUrl; }


  constructor(http: HttpClient, configurations: ConfigurationService, injector: Injector) {

    super(http, configurations, injector);
  }


  getEmployeeEndpoint(employeeId: string): Observable<Employee> {
    let endpointUrl = `${this.employeeUrl}/${employeeId}`;

    return this.http.get<Employee>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getEmployeeEndpoint(employeeId));
      });
  }


  getEmployees(page?: number, pageSize?: number): Observable<Employee[]> {
    let endpointUrl = page && pageSize ? `${this.employeeUrl}/${page}/${pageSize}` : this.employeeUrl;

    return this.http.get<Employee[]>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getEmployees(page, pageSize));
      });
  }


  getNewEmployeeEndpoint(employeeObject: Employee): Observable<CreatedWithGuid> {

    return this.http.post<CreatedWithGuid>(this.employeeUrl, JSON.stringify(employeeObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getNewEmployeeEndpoint(employeeObject));
      });
  }

  getUpdateEmployeeEndpoint(employeeObject: any, employeeId: string): Observable<any> {
    let endpointUrl = employeeId ? `${this.employeeUrl}/${employeeId}` : null;

    return this.http.put(endpointUrl, JSON.stringify(employeeObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUpdateEmployeeEndpoint(employeeObject, employeeId));
      });
  }

  getPatchUpdateEmployeeEndpoint<T>(patch: {}, employeeId: string): Observable<T>
  getPatchUpdateEmployeeEndpoint<T>(value: any, op: string, path: string, employeeId: string, from?: any): Observable<T>
  getPatchUpdateEmployeeEndpoint<T>(valueOrPatch: any, employeeId: string, opOrEmployeeId?: string, path?: string, from?: any): Observable<T> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = `${this.employeeUrl}/${employeeId}`;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrEmployeeId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrEmployeeId }];
    }
    else {
      endpointUrl = `${this.employeeUrl}/${opOrEmployeeId}`;
      patchDocument = valueOrPatch;
    }

    return this.http.patch<T>(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPatchUpdateEmployeeEndpoint(valueOrPatch, opOrEmployeeId, path, from, employeeId));
      });
  }

  getBlockEmployeeEndpoint<T>(employeeId: string): Observable<T> {
    let endpointUrl = `${this.employeeUrl}/${employeeId}/block`;

    return this.http.put<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getBlockEmployeeEndpoint(employeeId));
      });
  }

  getUnblockEmployeeEndpoint<T>(employeeId: string): Observable<T> {
    let endpointUrl = `${this.employeeUrl}/${employeeId}/unblock`;

    return this.http.put<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUnblockEmployeeEndpoint(employeeId));
      });
  }

  getDeleteEmployeeEndpoint<T>(employeeId: string): Observable<T> {
    let endpointUrl = `${this.employeeUrl}/${employeeId}`;

    return this.http.delete<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDeleteEmployeeEndpoint(employeeId));
      });
  }
}