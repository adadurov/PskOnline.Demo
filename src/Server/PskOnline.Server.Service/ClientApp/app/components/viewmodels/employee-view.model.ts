import { Employee } from "../../models/employee.model";

export class EmployeeView extends Employee
{
  constructor(data: Employee)
  {
    super();
    Object.assign(this, data);
  }

  public positionName: string;
}