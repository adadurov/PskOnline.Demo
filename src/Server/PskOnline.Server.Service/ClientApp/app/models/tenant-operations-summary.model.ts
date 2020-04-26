// the model to transfer the summary operations information about the tenant
export class TenantOperationsSummary {
  constructor() {
  }

  public id: string;

  public name: string;

  public serviceExpireDate: Date;

  public serviceMaxUsers: number;

  public serviceMaxEmployees: number;

  public serviceActualUsers: number;

  public serviceActualEmployees: number;

  public branchOfficesCount: number;

  public departmentsCount: number;

  public newInspectionsLast24Hours: number;

  public newInspectionsLastWeek: number;
}
