import { TestBed, inject } from '@angular/core/testing';

import { WindowLocationService } from './window-location.service';

describe('HostnameProviderService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WindowLocationService]
    });
  });

  it('should be created', inject([WindowLocationService], (service: WindowLocationService) => {
    expect(service).toBeTruthy();
  }));
});
