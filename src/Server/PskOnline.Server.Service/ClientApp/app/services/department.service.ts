import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from "@angular/router";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { AuthService } from './auth.service';
import { Department } from '../models/department.model';
import { DepartmentEndpoint } from './department-endpoint.service';
import { Employee } from '../models/employee.model';
import { DepartmentWorkplace } from '../models/department-workplace.model';
import { DepartmentWorkplaceCredentials } from '../models/department-workplace-credentials.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';


@Injectable()
export class DepartmentService {

  constructor(private router: Router, private http: HttpClient, private authService: AuthService,
    private departmentEndpoint: DepartmentEndpoint) {

  }

  getDepartment(departmentId?: string): Observable<Department> {
    return this.departmentEndpoint.getDepartmentEndpoint(departmentId);
  }

  getDepartments(page?: number, pageSize?: number): Observable<Department[]> {

    return this.departmentEndpoint.getDepartments(page, pageSize);
  }

  newDepartment(department: Department): Observable<CreatedWithGuid> {
    return this.departmentEndpoint.getNewDepartmentEndpoint(department);
  }

  newCredentials(departmentId: string, workplaceDescriptor: DepartmentWorkplace): Observable<DepartmentWorkplaceCredentials> {
    return this.departmentEndpoint.postNewWorkplace(departmentId, workplaceDescriptor);
  }

  getEmployeesInDepartment(departmentId: string): Observable<Employee[]> {
    return this.departmentEndpoint.getEmployeesInDepartment(departmentId);
  }

  updateDepartment(department: Department) {
    return this.departmentEndpoint.getUpdateDepartmentEndpoint(department, department.id);
  }

  deleteDepartment(departmentOrDepartmentId: string | Department): Observable<Department> {

    if (typeof departmentOrDepartmentId === 'string' || departmentOrDepartmentId instanceof String) {
      return this.departmentEndpoint.getDeleteDepartmentEndpoint(<string>departmentOrDepartmentId);
    }
    else {

      if (departmentOrDepartmentId.id) {
        return this.deleteDepartment(departmentOrDepartmentId.id);
      }
      else {
        ;
      }
    }
  }
}