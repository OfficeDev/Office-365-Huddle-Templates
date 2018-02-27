/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { WeekDay } from '../shared/models/weekDay';
import { WeekInputViewModel } from '../shared/models/weekInputViewModel';
import { MetricValue } from '../shared/models/metricValue';
import { Issue } from '../shared/models/issue';
import { ReasonValue } from '../shared/models/reasonValue';
import { Reason } from '../shared/models/reason';
import { Metric } from '../shared/models/metric';

declare var moment: any;

export class DateHelper {

    public static getStartAndEndDayOfWeek(date?: Date) {
        var now = date ? new Date(date) : new Date();
        now.setHours(0, 0, 0, 0);

        var firstDay = new Date(now);
        firstDay.setDate(firstDay.getDate() - firstDay.getDay());

        var endDay = new Date(now);
        endDay.setDate(endDay.getDate() - endDay.getDay() + 6);

        var result = new WeekDay();
        result.startDay = firstDay;
        result.endDay = endDay;
        return result;
    }

    public static getDaysofWeek(weekDay: WeekDay): Array<Date> {
        let result = new Array<Date>();
        for (let i = 0; i < 7; i++) {
            let startDay = new Date(weekDay.startDay);
            let current = startDay.setDate(startDay.getDate() + i);
            result.push(new Date(current));
        }
        return result;
    }

    public static getStartDateString(weekDay: WeekDay): string {
        let startDate = weekDay.startDay;
        return this.LocalToUTC(startDate).substring(0, 10);
    }

    public static getWeekInputViewModel(isMetricValue: boolean, weekDay: WeekDay, metric: Metric, reason: Reason): WeekInputViewModel {
        let aWeekInputViewModel = new WeekInputViewModel();
        aWeekInputViewModel.isMetricValue = isMetricValue;
        aWeekInputViewModel.weekDay = weekDay;
        let daysOfWeek = this.getDaysofWeek(weekDay);
        let i = 0;
        while (i < aWeekInputViewModel.inputLength) {
            let inputDate = daysOfWeek[i];
            if (isMetricValue) {
                let metricValue = new MetricValue();
                metricValue.inputDate = inputDate;
                metricValue.metric = metric;
                aWeekInputViewModel.metricValueArray[i] = metricValue;
            } else {
                let reasonMetric = new ReasonValue();
                reasonMetric.inputDate = inputDate;
                reasonMetric.reason = reason;
                aWeekInputViewModel.reasonValueArray[i] = reasonMetric;
            }
            i++;
        }
        return aWeekInputViewModel;
    }

    public static UTCToLocal(utcTime: Date): Date {
        let utcString = '';
        if ((typeof utcTime) == 'string')
            utcString = utcTime.toString();
        else
            utcString = utcTime.toISOString();
        return moment.utc(utcString).toDate() as Date;
    }

    public static LocalToUTC(date?: Date): string {
        return date.toISOString();
    }

    public static getDatesIntervalStr(weekDay: WeekDay): string {
        let result = '';
        if (weekDay.startDay.getFullYear() != weekDay.endDay.getFullYear()) {
            let format1 = 'MMM DD, YYYY';
            return moment(weekDay.startDay).format(format1) + ' - ' + moment(weekDay.endDay).format(format1);
        } else if (weekDay.startDay.getMonth() != weekDay.endDay.getMonth()) {
            let format2 = 'MMM DD';
            return moment(weekDay.startDay).format(format2) + ' - ' + moment(weekDay.endDay).format(format2) + ', ' + moment(weekDay.startDay).format('YYYY');
        } else {
            return moment(weekDay.startDay).format('MMM') + ' ' + moment(weekDay.startDay).format('DD') + ' - ' + moment(weekDay.endDay).format('DD') + ', ' + moment(weekDay.startDay).format('YYYY');
        }
    }

    public static isDateEqual(metricDateLocal: Date, metricDateFromBackend: Date): boolean {
        let backendDate = new Date(metricDateFromBackend.toString());
        let localDate = new Date(DateHelper.LocalToUTC(metricDateLocal));
        return backendDate.getFullYear() == localDate.getFullYear()
            && backendDate.getMonth() == localDate.getMonth()
            && backendDate.getDate() == localDate.getDate();
    }
}
