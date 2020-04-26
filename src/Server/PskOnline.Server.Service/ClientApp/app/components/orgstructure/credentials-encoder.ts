import { DeptWorkplaces } from "../viewmodels/dept-workplaces.model";
import { DepartmentWorkplaceCredentials } from "../../models/department-workplace-credentials.model";
import { WindowLocationService } from "../../services/window-location.service";

export class CredentialsEncoder {
  public EncodeMultiple(
      locationProvider: WindowLocationService,
      opCred: DepartmentWorkplaceCredentials,
      audCred: DepartmentWorkplaceCredentials): DeptWorkplaces {

    const host = locationProvider.getHost();
    const rootUrl = `https://${host}/nss-dashboard/`;
    return {
      opClientId: opCred.clientId,
      opClientSecret: opCred.clientSecret,
      opUrl: rootUrl + `?client_id=${encodeURIComponent(opCred.clientId)}&client_secret=${encodeURIComponent(opCred.clientSecret)}`,
      opAccessKey: this.base64Encode(opCred.clientId, opCred.clientSecret),
      audClientId: audCred.clientId,
      audClientSecret: audCred.clientSecret,
      audUrl: rootUrl + `?client_id=${encodeURIComponent(audCred.clientId)}&client_secret=${encodeURIComponent(audCred.clientSecret)}`,
      audAccessKey: this.base64Encode(audCred.clientId, audCred.clientSecret)
    };
  }

  base64Encode(clientId: string, clientSecret: string): string {
    var combined = {
      typ: 'ccr', // ccr is for C-lient Cr-edentials
      cid: clientId,
      csec: clientSecret
    };
    return EncodeDecode.b64EncodeUnicode(JSON.stringify(combined));
  }

  public DecodeSingle(combinedEncoded: string): any {
    var combined = EncodeDecode.b64EncodeUnicode(combinedEncoded);
    var decLogin;
    var decPwd;
    return {
      login: decLogin,
      password: decPwd
    };
  }

}

class EncodeDecode {

  static b64EncodeUnicode(str: any) {
    // first we use encodeURIComponent to get percent-encoded UTF-8,
    // then we convert the percent encodings into raw bytes which
    // can be fed into btoa.
    return btoa(encodeURIComponent(str).replace(/%([0-9A-F]{2})/g,
      // function toSolidBytes(match, p1) {
      (match, p1) => {
        // console.debug('match: ' + match);
        return String.fromCharCode(("0x" + p1) as any);
      }));
  }

  static b64DecodeUnicode(str) {
    // Going backwards: from bytestream, to percent-encoding, to original string.
    return decodeURIComponent(atob(str).split('').map(function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
  }

}
