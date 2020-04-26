import { UserRoleDetail } from "./user-role-detail.model";

export class User {
  // Note: Using only optional constructor properties without backing store disables typescript's type checking for the type
  constructor(id?: string, userName?: string, firstName?: string, lastName?: string, patronymic?: string, email?: string, jobTitle?: string, phoneNumber?: string, tenantId?:string, roles?: UserRoleDetail[]) {

        this.id = id;
        this.userName = userName;
        this.firstName = firstName;
        this.lastName = lastName;
        this.patronymic = patronymic;
        this.email = email;
        this.jobTitle = jobTitle;
        this.phoneNumber = phoneNumber;
        this.tenantId = tenantId;
        this.roles = roles;
    }

    get friendlyName(): string {
        let name = this.fullUserName || this.userName;

        if (this.jobTitle)
            name = this.jobTitle + " " + name;

        return name;
    }

    get fullUserName(): string {
      let parts = [this.lastName, this.firstName, this.patronymic];
      return parts.join(' ');
    }

    public id: string;
    public userName: string;
    public firstName: string;
    public lastName: string;
    public patronymic: string;
    public email: string;
    public jobTitle: string;
    public phoneNumber: string;
    public isEnabled: boolean;
    public isLockedOut: boolean;
    public isDepartmentSpecialUser: string;
    public tenantId: string;
    public roles: UserRoleDetail[];
}