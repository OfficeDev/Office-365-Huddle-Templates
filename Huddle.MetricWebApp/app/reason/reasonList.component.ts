/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CookieService } from '../services/cookie.service';
import { ReasonService } from '../services/reason.service';
import { MetricService } from '../services/metric.service';
import { Reason } from '../shared/models/reason';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { State, TrackFrequency } from '../shared/models/state';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';
import { NewReasonComponent } from './newReason.component';
import { EditReasonComponent } from './editReason.component';
import { Metric } from '../shared/models/metric';
import { WeekSelectorService } from '../services/weekSelector.service';
import { WeekDay } from '../shared/models/weekDay';
import { WeekInputViewModel } from '../shared/models/weekInputViewModel';
import { MetricValueService } from '../services/metricValue.service';
import { DateHelper } from '../utils/dateHelper';
import { Observable } from 'rxjs/Observable';

declare var microsoftTeams: any;

@Component({
    templateUrl: './reasonList.component.html',
    selector: 'reason-list',
    styleUrls: ['./reasonList.component.css', '../shared/shared.css']
})

export class ReasonListComponent implements OnInit {
    @Input('currentMetric') currentMetric: Metric;

    reasonsArray = new Array<Reason>();
    metricId = 1;
    reasonToEdit: Reason = null;
    addReasonType: string;
    isHidden: boolean = true;
    reasonWeelyArray = new Array<Reason>();
    reasonDailyArray = new Array<Reason>();
    currentWeekDays = new Array<Date>();
    selectWeekDay: WeekDay;
    weekInputviewModel: WeekInputViewModel;
    currentReasonValues: string;
    isInited: boolean = false;
    reasonWeekInputViewModelArray = new Array<WeekInputViewModel>();
    reasonDailyWeekInputViewModelArray = new Array<WeekInputViewModel>();
    reasonWeeklyWeekInputViewModelArray = new Array<WeekInputViewModel>();

    @ViewChild('modalAddReason') modalAddReason: ModalComponent;
    @ViewChild(NewReasonComponent) addReasonPopUp: NewReasonComponent;
    @ViewChild('modalEditReason') modalEditReason: EditReasonComponent;
    @ViewChild(EditReasonComponent) editReasonPopUp: EditReasonComponent;

    constructor(private reasonService: ReasonService, private metricServics: MetricService, private router: Router, private activateRoute: ActivatedRoute, private cookieService: CookieService, private weekSelectorService: WeekSelectorService, private metricValueService: MetricValueService) {
    }

    ngOnInit(): void {
        this.subscribeWeekSelector();
    }

    initReasons() {
        this.reasonService.getReasonsByMetric(this.currentMetric.id)
            .subscribe(reasons => {
                this.filterReasons(reasons);
                this.rebuildWeekInputViewModel();
            });
    }

    removeAllUpdatedFlags() {
        this.reasonWeekInputViewModelArray.forEach(vm => {
            vm.reasonValueArray.forEach(rv => rv.isUpdated = false);
        });
    }

    filterReasons(reasons) {
        this.reasonDailyArray = [];
        this.reasonWeelyArray = [];
        for (var i = 0; i < reasons.length; i++) {
            if (reasons[i].trackingFrequency == TrackFrequency.daily) {
                this.reasonDailyArray.push(reasons[i]);
            } else {
                this.reasonWeelyArray.push(reasons[i]);
            }
        }
    }
    
    addReasonClick(reasonType: string) {
        this.addReasonType = reasonType;
        this.modalAddReason.open();
    }

    editReasonClick(reason: Reason) {
        this.reasonToEdit = reason;
        this.modalEditReason.open();
    }

    closed() {
    }

    dismissed() {
    }

    opened() {
        this.addReasonPopUp.iniControls(this.addReasonType);
    }

    editReasonOpened() {
        this.editReasonPopUp.iniControls(this.reasonToEdit);
    }

    getDisplayValueType(reason: Reason) {
        return CommonUtil.getDisplayValueType(reason.valueType);
    }
    
    onSwitch(id: any) {
        this.reasonService.updateReasonStatus(id);
        for (var i = 0; i < this.reasonWeelyArray.length; i++) {
            if (this.reasonWeelyArray[i].id == id) {
                var reasonState = this.reasonWeelyArray[i].reasonState;
                if (reasonState == State.active)
                    this.reasonWeelyArray[i].reasonState = State.closed;
                else
                    this.reasonWeelyArray[i].reasonState = State.active;
            }
        }
        for (var i = 0; i < this.reasonDailyArray.length; i++) {
            if (this.reasonDailyArray[i].id == id) {
                var reasonState = this.reasonDailyArray[i].reasonState;
                if (reasonState == State.active)
                    this.reasonDailyArray[i].reasonState = State.closed;
                else
                    this.reasonDailyArray[i].reasonState = State.active;
            }
        }
    }

    show() {
        this.isHidden = false;
        if (this.isInited === false)
            this.initReasons();
    }

    hide() {
        this.isHidden = true;
    }

    afterCloseNewReason(toAddReason: Reason) {
        this.modalAddReason.close();
        this.initReasons();
    }

    afterCloseEditReason(toAddReason: Reason) {
        this.modalEditReason.close();
        this.initReasons();
    }

    subscribeWeekSelector() {
        this.weekSelectorService.selectWeek.subscribe(weekDay => {
            this.currentWeekDays = this.weekSelectorService.getCurrentWeekDays();
            this.rebuildWeekInputViewModel();
        });
        this.weekSelectorService.selectClick.subscribe(checkClick => {
        });
    }

    afterInputValueChange(val: number) {
        if (this.isInputValueChanged())
            this.weekSelectorService.denySelectForReason();
        else
            this.weekSelectorService.allowSelectForReason();
    }

    isInputValueChanged() {
        if (this.currentReasonValues)
            return this.currentReasonValues !== this.recalcMetricValues();
        return false;
    }

    rebuildWeekInputViewModel() {
        this.selectWeekDay = this.weekSelectorService.getCurrentWeek();
        if (this.reasonDailyArray.length > 0) {
            this.reasonDailyWeekInputViewModelArray = this.reasonDailyArray.map(reasonView => {
                let weekInputViewModel = DateHelper.getWeekInputViewModel(false, this.selectWeekDay, null, reasonView);
                weekInputViewModel.isWeeklyReason = false;
                return weekInputViewModel;
            });
        }
        if (this.reasonWeelyArray.length > 0) {
            this.reasonWeeklyWeekInputViewModelArray = this.reasonWeelyArray.map(reasonView => {
                let weekInputViewModel = DateHelper.getWeekInputViewModel(false, this.selectWeekDay, null, reasonView);
                weekInputViewModel.isWeeklyReason = true;
                return weekInputViewModel;
            });
        }
        this.reasonWeekInputViewModelArray = this.reasonDailyWeekInputViewModelArray.concat(this.reasonWeeklyWeekInputViewModelArray);
        this.getMetricValues();
    }

    getMetricValues() {
        this.reasonWeekInputViewModelArray.forEach(reasonWeekVM => reasonWeekVM.reasonValueArray.forEach(reasonVal => {
            reasonVal.reasonMetricValues = null;
            reasonVal.id = 0;
        }));
        let self = this;
        let reasonsArray = this.reasonDailyArray.concat(this.reasonWeelyArray);
        this.metricValueService.getMetricAndReasonValues([], reasonsArray.map(reason => reason.id), this.selectWeekDay)
            .subscribe(resp => {
                //refill reason values with xhr result
                resp.forEach(weekInputWM => {
                    if (weekInputWM.reasonValueArray.length > 0) {
                        let targetReasonWeekInputVM = self.reasonWeekInputViewModelArray.find(reasonWeekInputVm => reasonWeekInputVm.reasonValueArray[0].reason.id == weekInputWM.reasonValueArray[0].reason.id);
                        if (targetReasonWeekInputVM) {
                            targetReasonWeekInputVM.reasonValueArray.forEach((reasonValue, index) => {
                                let backendReasonValue = weekInputWM.reasonValueArray.find(rv => DateHelper.isDateEqual(reasonValue.inputDate, rv.inputDate));
                                if (backendReasonValue) {
                                    reasonValue.reasonMetricValues = backendReasonValue.reasonMetricValues;
                                    reasonValue.id = backendReasonValue.id;
                                    reasonValue.inputDate = DateHelper.UTCToLocal(reasonValue.inputDate);
                                }
                            });
                        }
                    }
                });
                this.currentReasonValues = this.recalcMetricValues();
                this.isInited = true;
            });
    }

    updateReasonValues(): Observable<boolean> {
        if (this.reasonWeekInputViewModelArray.length == 0)
            return new Observable(observer => { observer.next(); });
        return this.metricValueService.updateMetricAndReasonValues(
            [],
            this.reasonWeekInputViewModelArray.map(reasonWeekVM => reasonWeekVM.reasonValueArray),
            this.currentReasonValues
        );
    }

    recalcMetricValues() {
        return this.metricValueService.getMetricReasonValuesJSON([], this.reasonWeekInputViewModelArray);
    }
}
