import { Injectable, Inject } from '@angular/core';
import { getHost } from '../app.module';

@Injectable()
export class WindowLocationService {

  constructor() { }

  // returns hostName:port
  getHost(): string {
    return getHost();
  }

}
