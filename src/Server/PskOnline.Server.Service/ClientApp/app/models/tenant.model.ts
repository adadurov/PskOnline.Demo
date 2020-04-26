import { ServiceDetails } from './service-details.model'
import { ContactInfo } from './contact-info.model'

export class Tenant {
  constructor() {
    this.serviceDetails = new ServiceDetails();
    this.primaryContact = new ContactInfo();
  }
  public id: string;

  public name: string;

  public comment: string;

  public slug: string;

  public isLockedOut: boolean;

  public serviceDetails: ServiceDetails;

  public primaryContact: ContactInfo;
}
