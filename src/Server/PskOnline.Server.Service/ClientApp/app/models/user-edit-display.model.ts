
import { UserEdit } from './user-edit.model';
import { UserRoleDetail } from './user-role-detail.model';


export class UserEditDisplay extends UserEdit {
    constructor(currentPassword?: string, newPassword?: string, confirmPassword?: string) {
        super();

    }

  public index: Number;

}