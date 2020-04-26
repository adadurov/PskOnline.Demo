import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

import { EndpointFactory } from './endpoint-factory.service';
import { ConfigurationService } from './configuration.service';
import { Position } from '../models/position.model';


@Injectable()
export class PositionEndpoint extends EndpointFactory {

  private readonly _url: string = "/api/Position";

  get fullUrl() { return this.configurations.baseUrl + this._url; }


  constructor(http: HttpClient, configurations: ConfigurationService, injector: Injector) {

    super(http, configurations, injector);
  }


  getPositionEndpoint<T>(positionId: string): Observable<T> {
    let endpointUrl = `${this.fullUrl}/${positionId}`;

    return this.http.get<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPositionEndpoint(positionId));
      });
  }


  getPositions<T>(page?: number, pageSize?: number): Observable<T> {
    let endpointUrl = page && pageSize ? `${this.fullUrl}/${page}/${pageSize}` : this.fullUrl;

    return this.http.get<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPositions(page, pageSize));
      });
  }


  getNewPositionEndpoint<T>(positionObject: Position): Observable<T> {

    return this.http.post<T>(this.fullUrl, JSON.stringify(positionObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getNewPositionEndpoint(positionObject));
      });
  }

  getUpdatePositionEndpoint<T>(positionObject: any, positionId: string): Observable<T> {
    let endpointUrl = positionId ? `${this.fullUrl}/${positionId}` : null;

    return this.http.put<T>(endpointUrl, JSON.stringify(positionObject), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getUpdatePositionEndpoint(positionObject, positionId));
      });
  }

  getPatchUpdatePositionEndpoint<T>(patch: {}, positionId: string): Observable<T>
  getPatchUpdatePositionEndpoint<T>(value: any, op: string, path: string, positionId: string, from?: any): Observable<T>
  getPatchUpdatePositionEndpoint<T>(valueOrPatch: any, positionId: string, opOrPositionId?: string, path?: string, from?: any): Observable<T> {
    let endpointUrl: string;
    let patchDocument: {};

    if (path) {
      endpointUrl = `${this.fullUrl}/${positionId}`;
      patchDocument = from ?
        [{ "value": valueOrPatch, "path": path, "op": opOrPositionId, "from": from }] :
        [{ "value": valueOrPatch, "path": path, "op": opOrPositionId }];
    }
    else {
      endpointUrl = `${this.fullUrl}/${opOrPositionId}`;
      patchDocument = valueOrPatch;
    }

    return this.http.patch<T>(endpointUrl, JSON.stringify(patchDocument), this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getPatchUpdatePositionEndpoint(valueOrPatch, opOrPositionId, path, from, positionId));
      });
  }

  getDeletePositionEndpoint<T>(positionId: string): Observable<T> {
    let endpointUrl = `${this.fullUrl}/${positionId}`;

    return this.http.delete<T>(endpointUrl, this.getRequestHeaders())
      .catch(error => {
        return this.handleError(error, () => this.getDeletePositionEndpoint(positionId));
      });
  }
}