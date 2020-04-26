import { Component, OnInit, OnDestroy, Input } from "@angular/core";
import { Subject } from "rxjs";
import { takeUntil } from 'rxjs/operators';

import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { Utilities } from '../../services/utilities';
import { AccountService } from "../../services/account.service";
import { ActivatedRoute } from "@angular/router";

@Component({
  selector: "app-forgot-password",
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent implements OnInit, OnDestroy {
  ngUnsubscribe: Subject<any> = new Subject<any>();

  state: string = "forgot";
  userNameOrEmail: string = "";

  isLoading = false;
  formResetToggle = true;
  modalClosedCallback: () => void;
  loginStatusSubscription: any;

  @Input()
  isModal = false;

  constructor(
    private alertService: AlertService,
    private accountService: AccountService,
    private translationService: AppTranslationService,
    private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.params
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(params => {

        this.state = "forgot";

        if (params) {
          this.userNameOrEmail = params['nameOrEmail'];
        }
        else {
          this.userNameOrEmail = "";
        }
      });
  }

  ngOnDestroy() {
    // End all subscriptions listening to ngUnsubscribe
    // to avoid memory leaks.
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  showErrorAlert(caption: string, message: string) {
    this.alertService.showMessage(caption, message, MessageSeverity.error);
  }

  closeModal() {
    if (this.modalClosedCallback) {
      this.modalClosedCallback();
    }
  }

  requestPasswordReset() {
    this.isLoading = true;
    this.alertService.startDelayedMessage("", this.translationService.getTranslation("forgotPassword.requestingReset"));

    this.accountService.resetPasswordStart(this.userNameOrEmail)
      .subscribe(
        user => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;
            this.reset();
            this.closeModal();
            this.state = "request_success";
//            this.routerService.navigate(["/pwd-reset-requested"]);
          }, 500);
        },
        error => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();

            this.isLoading = false;
            this.reset();
            this.closeModal();
            this.state = "request_error";
//            this.routerService.navigate(["/pwd-reset-request-error"]);
          }, 500);
        });
  }

  reset() {
    this.formResetToggle = false;

    setTimeout(() => {
      this.formResetToggle = true;
    });
  }

  showEmailAlert() {
    this.showErrorAlert('Username or email is required', 'Please enter your account username or email');
  }

}
