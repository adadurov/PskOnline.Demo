export class Employee {

  constructor() {
  }

  public get fullName(): string {
    var names = [this.lastName, this.firstName, this.patronymic];
    return names.join(" ");
  }

  public id: string;

  public gender: number;

  public birthDate: Date;

  public userId: string;

  public firstName: string;

  public lastName: string;

  public patronymic: string;

  public externalId: string;

  public positionId: string;

  public departmentId: string;

  public branchOfficeId: string;
}
