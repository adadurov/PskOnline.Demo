import { Tenant } from './tenant.model'
import { TenantCreateAdmin } from './tenant-create-admin.model'

export class TenantCreate {
  constructor() {
    this.tenantDetails = new Tenant();
    this.adminUserDetails = new TenantCreateAdmin();
  }

  public tenantDetails: Tenant;

  public adminUserDetails: TenantCreateAdmin;

}
