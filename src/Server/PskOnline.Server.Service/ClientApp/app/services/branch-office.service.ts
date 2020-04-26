import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from "@angular/router";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { BranchOfficeEndpoint } from './branch-office-endpoint.service';
import { AuthService } from './auth.service';
import { BranchOffice} from '../models/branch-office.model';


@Injectable()
export class BranchOfficeService {

  constructor(private router: Router, private http: HttpClient, private authService: AuthService,
    private branchOfficeEndpoint: BranchOfficeEndpoint) {

  }

  getBranchOffice(branchOfficeId?: string) {
    return this.branchOfficeEndpoint.getBranchOfficeEndpoint<BranchOffice>(branchOfficeId);
  }

  getBranchOffices(page?: number, pageSize?: number) {

    return this.branchOfficeEndpoint.getBranchOffices<BranchOffice[]>(page, pageSize);
  }

  // return the object with the assigned id
  newBranchOffice(branchOffice: BranchOffice) {
    return this.branchOfficeEndpoint.getNewBranchOfficeEndpoint<BranchOffice>(branchOffice);
  }

  updateBranchOffice(branchOffice: BranchOffice) {
    return this.branchOfficeEndpoint.getUpdateBranchOfficeEndpoint(branchOffice, branchOffice.id);
  }

  deleteBranchOffice(branchOfficeOrBranchOfficeId: string | BranchOffice): Observable<BranchOffice> {

    if (typeof branchOfficeOrBranchOfficeId === 'string' || branchOfficeOrBranchOfficeId instanceof String) {
      return this.branchOfficeEndpoint.getDeleteBranchOfficeEndpoint<BranchOffice>(<string>branchOfficeOrBranchOfficeId);
    }
    else {

      if (branchOfficeOrBranchOfficeId.id) {
        return this.deleteBranchOffice(branchOfficeOrBranchOfficeId.id);
      }
      else {
        ;
      }
    }
  }
}