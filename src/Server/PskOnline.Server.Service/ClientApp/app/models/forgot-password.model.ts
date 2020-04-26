
export class ForgotPassword {
  constructor(userNameOrEmail?: string) {
      this.userNameOrEmail = userNameOrEmail;
    }

    userNameOrEmail: string;
}