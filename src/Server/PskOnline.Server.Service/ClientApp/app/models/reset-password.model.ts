
export class ResetPassword {
  constructor(userName: string, token: string, newPassword:string) {
      this.userName = userName;
      this.token = token;
      this.newPassword = newPassword;
    }

    userName: string;
    token: string;
    newPassword: string;
}
