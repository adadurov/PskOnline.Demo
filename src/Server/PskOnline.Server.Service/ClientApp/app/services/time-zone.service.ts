import { TimeZoneView } from "../components/viewmodels/time-zone-view.model";

export class TimeZoneService {

  public GetTimeZones(): TimeZoneView[] {
    return TIMEZONES;
  }
}

export const TIMEZONES: TimeZoneView[] = [
  {
    id : "MSK-1",
    displayName : "Калининград (MSK-1, UTC+02:00)"
  },
  {
    id : "MSK",
    displayName : "Москва (MSK+0, UTC+03:00)"
  },
  {
    id: "MSK+1",
    displayName : "Самара (MSK+1, UTC+04:00)"
  },
  {
    id : "MSK+2",
    displayName : "Екатеринбург (MSK+2, UTC+05:00)"
  },
  {
    id : "MSK+3",
    displayName : "Омск (MSK+3, UTC+06:00)"
  },
  {
    id : "MSK+4",
    displayName : "Красноярск (MSK+4, UTC+07:00)"
  },
  {
    id : "MSK+5",
    displayName : "Иркутск (MSK+5, UTC+08:00)"
  },
  {
    id : "MSK+6",
    displayName : "Якутск (MSK+6, UTC+09:00)"
  },
  {
    id : "MSK+7",
    displayName : "Владивосток (MSK+7, UTC+10:00)"
  },
  {
    id : "MSK+8",
    displayName : "Магадан (MSK+8, UTC+11:00)"
  },
  {
    id : "MSK+9",
    displayName : "Камчатка (MSK+9, UTC+12:00)"
  }
];
