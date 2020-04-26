
import { Component, OnInit, OnDestroy, Input, ViewChild } from "@angular/core";

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AlertService, MessageSeverity, DialogType } from '../../services/alert.service';
import { AppTranslationService } from '../../services/app-translation.service';
import { Utilities } from '../../services/utilities';
import { ResetPassword } from '../../models/reset-password.model';
import { AccountService } from "../../services/account.service";
import { ActivatedRoute } from "@angular/router";

export type PwdResetStates =
  "reset_password" | "link_not_valid" | "success" | "error";

@Component({
  selector: "app-pwd-reset",
  templateUrl: './pwd-reset.component.html',
  styleUrls: []
})
export class PwdResetComponent implements OnInit, OnDestroy {
  ngUnsubscribe: Subject<any> = new Subject<any>();
  mode: PwdResetStates = "reset_password";
  errorDetail: string = "";

  resetPasswordModel: ResetPassword = null;
  userName: string = "";

  newPassword1: string = "";
  newPassword2: string = "";

  isLoading = false;
  formResetToggle = true;
  modalClosedCallback: () => void;
  resetStatusSubscription: any;

  @Input()
  isModal = false;

  @ViewChild('f')
  private form;

  constructor(
    private alertService: AlertService,
    private accountService: AccountService,
    private translationService: AppTranslationService,
    private route: ActivatedRoute) {
  }

  ngOnInit() {
    this.route.queryParams
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(params => {

        // if we didn't receive any parameters, we can't do anything
        if (!params) {
          this.mode = "link_not_valid";
          return;
        }

        var token = params['token'];
        var userName = params['username'];

        this.resetPasswordModel = new ResetPassword(userName, token, "");
        this.userName = userName;

        if (!(token !== undefined && token !== "" && userName !== undefined && userName !== "")) {
          // hide 'enter password' section and show 'link is not valid' section
          this.mode = "link_not_valid";
        }
      });
  }

  ngOnDestroy() {
    if (this.resetStatusSubscription)
      this.resetStatusSubscription.unsubscribe();

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

  doResetPassword() {
    this.isLoading = true;
    this.alertService.startDelayedMessage("", this.translationService.getTranslation("login.attemptingLogin"));
    this.resetPasswordModel.newPassword = this.newPassword1;
    this.accountService.doResetPassword(this.resetPasswordModel)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        _ => {
          setTimeout(() => {
            this.alertService.stopLoadingMessage();
            this.isLoading = false;
            this.resetForm();

            this.confirmSuccess();
          }, 500);
        },
        error => {

          this.alertService.stopLoadingMessage();

          if (Utilities.checkNoNetwork(error)) {
            this.alertService.showStickyMessage(Utilities.noNetworkMessageCaption, Utilities.noNetworkMessageDetail, MessageSeverity.error, error);
          }
          else {
            if (error.status === 400) {
              // password doesn't match the password complexity requirements
              this.showErrorAlert("Пароль не изменен", "Новый пароль не соответствует требованиям к надежности пароля.");
            }
            else if (error.status === 403) {
              // token invalid or expired
              this.toFatalError("Пароль не изменен", "Код для сброса пароля недействителен.");
            }
          }

          setTimeout(() => {
            this.isLoading = false;
          }, 500);
        });
  }

  resetForm() {
    this.formResetToggle = false;
//    this.form.reset();
    setTimeout(() => {
      this.formResetToggle = true;
    });
  }

  confirmSuccess() {
    this.mode = "success";
  }

  toFatalError(title: string, errorDetail: string) {
    this.showErrorAlert(title, errorDetail);
    this.errorDetail = errorDetail;
    this.mode = "error";
  }

  showEmailAlert() {
    this.showErrorAlert('Email is required', 'Please enter a valid email');
  }

  showPasswordAlert() {
    this.showErrorAlert('Password is required', 'Please enter a valid password');
  }
}
