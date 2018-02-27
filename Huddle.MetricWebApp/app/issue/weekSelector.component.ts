/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { DateHelper } from '../utils/dateHelper';
import { AllowIssueClick } from '../shared/models/allowIssueClick';
import { WeekDay } from '../shared/models/weekDay';
import { WeekSelectorService } from '../services/weekSelector.service';

@Component({
    templateUrl: './weekSelector.component.html',
    selector: 'week-selector',
    styleUrls: ['./weekSelector.component.css', '../shared/shared.css']
})

export class WeekSelectorComponent implements OnInit {
    @Input('allowClick') allowClick: AllowIssueClick;
    @Output() afterCheckAllowClick: EventEmitter<boolean> = new EventEmitter<boolean>();

    weekDisplay: string;
    clickPrevious: boolean;
    clickNext: boolean;
    datesIntervalStr: string;

    constructor(private weekSelectorService: WeekSelectorService) {
    }

    ngOnInit(): void {
        this.datesIntervalStr = DateHelper.getDatesIntervalStr(this.currentWeek());
        this.subscribeWeekSelector();
    }

    subscribeWeekSelector() {
        this.weekSelectorService.selectWeek.subscribe(weekDay => {
            this.datesIntervalStr = DateHelper.getDatesIntervalStr(this.currentWeek());
        });
    }

    currentWeek(): WeekDay {
        return this.weekSelectorService.getCurrentWeek();
    }

    clickPreviousWeek() {
        this.clickPrevious = true;
        this.afterCheckAllowClick.emit(false);
        if (!this.allowClick.allowClick)
            return;
        this.weekSelectorService.gotoPreviousWeek();
    }

    clickNextWeek(): void {
        this.clickNext = true;
        this.afterCheckAllowClick.emit(false);
        if (!this.allowClick.allowClick)
            return;
        this.weekSelectorService.gotoNextWeek();
    }

    changeWeek() {
    }
}
