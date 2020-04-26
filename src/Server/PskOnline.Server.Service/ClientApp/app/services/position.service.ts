import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from "@angular/router";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { PositionEndpoint } from './position-endpoint.service';
import { AuthService } from './auth.service';
import { Position } from '../models/position.model';


@Injectable()
export class PositionService {

  constructor(private router: Router, private http: HttpClient, private authService: AuthService,
    private positionEndpoint: PositionEndpoint) {

  }

  getPosition(positionId?: string) {
    return this.positionEndpoint.getPositionEndpoint<Position>(positionId);
  }

  getPositions(page?: number, pageSize?: number) {

    return this.positionEndpoint.getPositions<Position[]>(page, pageSize);
  }

  newPosition(position: Position) {
    return this.positionEndpoint.getNewPositionEndpoint<Position>(position);
  }

  updatePosition(position: Position) {
    return this.positionEndpoint.getUpdatePositionEndpoint(position, position.id);
  }

  deletePosition(positionOrPositionId: string | Position): Observable<Position> {

    if (typeof positionOrPositionId === 'string' || positionOrPositionId instanceof String) {
      return this.positionEndpoint.getDeletePositionEndpoint<Position>(<string>positionOrPositionId);
    }
    else {

      if (positionOrPositionId.id) {
        return this.deletePosition(positionOrPositionId.id);
      }
      else {
        ;
      }
    }
  }
}