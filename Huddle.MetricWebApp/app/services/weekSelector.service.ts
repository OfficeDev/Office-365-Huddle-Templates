/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Injectable, Output, EventEmitter } from '@angular/core';
import { WeekDay } from '../shared/models/weekDay';
import { DateHelper } from '../utils/dateHelper';

@Injectable()
export class WeekSelectorService {
    @Output() selectWeek: EventEmitter<WeekDay> = new EventEmitter<WeekDay>();
    @Output() selectClick: EventEmitter<boolean> = new EventEmitter<boolean>();

    currentWeek: WeekDay;
    currentWeekDays: Array<Date>;
    isAllowMetricClick: boolean = true;
    isAllowReasonClick: boolean = true;
    clickPrevious: boolean;
    clickNext: boolean;

    getCurrentWeek() {
        if (this.currentWeek != null)
            return this.currentWeek;
        this.currentWeek = DateHelper.getStartAndEndDayOfWeek();
        return this.currentWeek;
    }

    getCurrentWeekDays() {
        if (this.currentWeekDays != null)
            return this.currentWeekDays;
        this.currentWeekDays = DateHelper.getDaysofWeek(this.getCurrentWeek());
        return this.currentWeekDays;
    }

    allowSelectForReason() {
        this.isAllowReasonClick = true;
    }

    denySelectForReason() {
        this.isAllowReasonClick = false;
    }

    allowSelectForMetric() {
        this.isAllowMetricClick = true;
    }

    denySelectForMetric() {
        this.isAllowMetricClick = false;
    }

    allowSelect() {
        this.allowSelectForMetric();
        this.allowSelectForReason();
    }

    isAllowClick() {
        return this.isAllowMetricClick && this.isAllowReasonClick;
    }

    gotoPreviousWeek() {
        this.clickPrevious = true;
        this.selectClick.emit(true);
        if (this.isAllowClick() === false)
            return;
        let dayBeforeWeek = this.currentWeek.startDay.setDate(this.currentWeek.startDay.getDate() - 2);
        this.currentWeek = DateHelper.getStartAndEndDayOfWeek(new Date(dayBeforeWeek));
        this.currentWeekDays = DateHelper.getDaysofWeek(this.currentWeek);
        this.selectWeek.emit(this.currentWeek);
    }

    gotoNextWeek() {
        this.clickNext = true;
        this.selectClick.emit(true);
        if (this.isAllowClick() === false)
            return;
        let dayAfterWeek = this.currentWeek.endDay.setDate(this.currentWeek.endDay.getDate() + 1);
        this.currentWeek = DateHelper.getStartAndEndDayOfWeek(new Date(dayAfterWeek));
        this.currentWeekDays = DateHelper.getDaysofWeek(this.currentWeek);
        this.selectWeek.emit(this.currentWeek);
    }

    continueChangeWeek() {
        this.allowSelect();
        if (this.clickNext == true)
            this.gotoNextWeek();
        if (this.clickPrevious == true)
            this.gotoPreviousWeek();
        this.resetClickState();
    }

    resetClickState() {
        this.clickNext = false;
        this.clickPrevious = false;
    }
}
