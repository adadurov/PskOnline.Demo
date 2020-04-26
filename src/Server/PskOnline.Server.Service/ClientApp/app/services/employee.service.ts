import { Injectable } from '@angular/core';
import { Router, NavigationExtras } from "@angular/router";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import 'rxjs/add/observable/forkJoin';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/map';

import { EmployeeEndpoint } from './employee-endpoint.service';
import { AuthService } from './auth.service';
import { Employee } from '../models/employee.model';
import { CreatedWithGuid } from '../models/created-with-guid.model';


@Injectable()
export class EmployeeService {

  constructor(private router: Router, private http: HttpClient, private authService: AuthService,
    private employeeEndpoint: EmployeeEndpoint) {

  }

  getEmployee(employeeId?: string) {
    return this.employeeEndpoint.getEmployeeEndpoint(employeeId);
  }

  getEmployees(page?: number, pageSize?: number) {

    return this.employeeEndpoint.getEmployees(page, pageSize);
  }

  newEmployee(employee: Employee): Observable<CreatedWithGuid> {
    return this.employeeEndpoint.getNewEmployeeEndpoint(employee);
  }

  updateEmployee(employee: Employee) {
    return this.employeeEndpoint.getUpdateEmployeeEndpoint(employee, employee.id);
  }

  deleteEmployee(employeeOrEmployeeId: string | Employee): Observable<Employee> {

    if (typeof employeeOrEmployeeId === 'string' || employeeOrEmployeeId instanceof String) {
      return this.employeeEndpoint.getDeleteEmployeeEndpoint<Employee>(<string>employeeOrEmployeeId);
    }
    else {

      if (employeeOrEmployeeId.id) {
        return this.deleteEmployee(employeeOrEmployeeId.id);
      }
      else {
        ;
      }
    }
  }
}