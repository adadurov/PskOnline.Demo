import { Gender } from '../../models/enums';

export class GenderView {
  constructor(genderCode: Gender, genderName: string) {
    this.code = genderCode;
    this.name = genderName;
  }
  public code: number;
  public name: string;
}