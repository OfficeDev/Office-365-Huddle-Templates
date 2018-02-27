/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, Input, Output, EventEmitter, ViewChild, ViewChildren, QueryList } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CookieService } from '../services/cookie.service';
import { MetricService } from '../services/metric.service';
import { Issue } from '../shared/models/issue';
import { Metric } from '../shared/models/metric';
import { Reason } from '../shared/models/reason';
import { IssueState } from '../shared/models/issueState';
import { IssueViewModel } from '../issueList/issue.viewmodel';
import { MetricViewModel } from './metric.viewmodel';
import { WeekDay } from '../shared/models/weekDay';
import { WeekInputViewModel } from '../shared/models/weekInputViewModel';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';
import { ReasonListComponent } from '../reason/reasonList.component';
import { DateHelper } from '../utils/dateHelper';
import { AddMetricComponent } from './addMetric.component';
import { EditMetricComponent } from './editMetric.component';
import { WeekSelectorService } from '../services/weekSelector.service';
import { MetricValueService } from '../services/metricValue.service';
import { State } from '../shared/models/state';
import { Observable, ReplaySubject } from 'rxjs/Rx';
import { MetricValue } from '../shared/models/metricValue';
import { ReasonValue } from '../shared/models/reasonValue';
declare var microsoftTeams: any;

@Component({
    templateUrl: './metricList.component.html',
    selector: 'metric-list',
    styleUrls: ['./metricList.component.css', '../shared/shared.css']
})

export class MetricListComponent implements OnInit {
    @Input('currentIssue') currentIssue: Issue;

    metricArray = new Array<MetricViewModel>();
    teamId = Constants.teamId;
    isHidden: boolean = true;
    currentIssueId: number = 0;
    currentMetricId: number = 0;
    currentMetric: Metric = null;
    priviousMetricStatus: State;
    currentWeekDays = new Array<Date>();
    selectWeekDay: WeekDay;
    weekInputviewModel: WeekInputViewModel;
    metricWeekInputViewModelArray = new Array<WeekInputViewModel>();
    currentMetricValues: string;

    @ViewChildren('reasonLists') reasonLists: QueryList<ReasonListComponent>;
    @ViewChild('modalAddMetric') modalAddMetric: ModalComponent;
    @ViewChild(AddMetricComponent) addMetricPopUp: AddMetricComponent;
    @ViewChild('modalEditMetric') modalEditMetric: ModalComponent;
    @ViewChild(EditMetricComponent) editMetricPopUp: EditMetricComponent;

    constructor(private metricService: MetricService, private router: Router, private activateRoute: ActivatedRoute, private cookieService: CookieService, private weekSelectorService: WeekSelectorService, private metricValueService: MetricValueService) {
    }

    ngOnInit(): void {
        this.initTeamContext();
        this.subscribeWeekSelector();
        this.currentWeekDays = this.weekSelectorService.getCurrentWeekDays();
    }

    show() {
        this.isHidden = false;
        this.currentIssueId = this.currentIssue.id;
        this.metricService.getMetricsByIssueId(this.currentIssue.id)
            .subscribe(resp => {
                this.metricArray = resp.map((metric, index) => {
                    metric.issue = this.currentIssue;
                    let metricViewModel = new MetricViewModel();
                    metricViewModel.metric = metric;
                    metricViewModel.expanded = false;
                    return metricViewModel;
                });
                this.rebuildWeekInputViewModel();
            });
    }

    hide() {
        this.isHidden = true;
    }

    initTeamContext() {
        this.teamId = CommonUtil.getTeamId();
    }

    reduiceActiveMetricCount() {
        this.currentIssue.activeMetricCount--;
        if (this.currentIssue.activeMetricCount < 0) {
            this.currentIssue.activeMetricCount = 0;
        }
    }

    onSwitch(id: number) {
        this.metricService.updateMetricStatus(id);
    }

    closed() {
    }

    getDisplayTargetGoal(metric: MetricViewModel) {
        let displayValType = CommonUtil.getDisplayValueType(metric.metric.valueType);
        if (metric.metric.valueType === Constants.valueTypes.dollars.val)
            return displayValType + metric.metric.targetGoal;
        return metric.metric.targetGoal + displayValType;
    }

    getDisplayValueType(metric: Metric) {
        return CommonUtil.getDisplayValueType(metric.valueType);
    }

    expandMetricClick(metric: MetricViewModel) {
        let currentReasonList = this.getRelatedReasonList(metric);
        if (metric.expanded === true) {
            metric.expanded = false;
            if (currentReasonList)
                currentReasonList.hide();
        } else {
            this.metricArray.forEach(metric => {
                metric.expanded = false;
                let metricList = this.getRelatedReasonList(metric);
                if (metricList !== null)
                    metricList.hide();
            });
            metric.expanded = true;
            if (currentReasonList !== null)
                currentReasonList.show();
        }
    }

    getRelatedReasonList(metric: MetricViewModel) {
        let result = this.reasonLists.filter(reasonList => reasonList.currentMetric.id == metric.metric.id);
        if (result.length > 0)
            return result[0];
        return null;
    }
    
    rebuildWeekInputViewModel() {
        this.selectWeekDay = this.weekSelectorService.getCurrentWeek();
        if (this.metricArray.length > 0) {
            this.metricWeekInputViewModelArray = this.metricArray.map(metricView => {
                return DateHelper.getWeekInputViewModel(true, this.selectWeekDay, metricView.metric, null);
            });
        }
        this.getMetricValues();
    }

    getMetricValues() {
        this.metricWeekInputViewModelArray.forEach(metricWeekVM => metricWeekVM.metricValueArray.forEach(metricVal => {
            metricVal.metricValues = null;
            metricVal.id = 0;
        }));
        let self = this;
        this.metricValueService.getMetricAndReasonValues(this.metricArray.map(metric => metric.metric.id), [], this.selectWeekDay)
            .subscribe(resp => {
                //refill metric values with xhr result
                resp.forEach(weekInputWM => {
                    if (weekInputWM.metricValueArray.length > 0) {
                        let targetMetricWeekInputVM = self.metricWeekInputViewModelArray.find(metricWeekInputVm => metricWeekInputVm.metricValueArray[0].metric.id == weekInputWM.metricValueArray[0].metric.id);
                        if (targetMetricWeekInputVM) {
                            targetMetricWeekInputVM.metricValueArray.forEach((metricValue, index) => {
                                let backendMetricValue = weekInputWM.metricValueArray.find(mv => DateHelper.isDateEqual(metricValue.inputDate, mv.inputDate));
                                if (backendMetricValue) {
                                    metricValue.metricValues = backendMetricValue.metricValues;
                                    metricValue.id = backendMetricValue.id;
                                    metricValue.inputDate = DateHelper.UTCToLocal(metricValue.inputDate);
                                }
                            });
                        }
                    }
                });
                this.currentMetricValues = this.recalcMetricValues();
            });
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
            this.weekSelectorService.denySelectForMetric();
        else
            this.weekSelectorService.allowSelectForMetric();
    }

    isInputValueChanged() {
        if (this.currentMetricValues)
            return this.currentMetricValues !== this.recalcMetricValues();
        return false;
    }

    removeAllUpdatedFlags() {
        this.metricWeekInputViewModelArray.forEach(vm => {
            vm.metricValueArray.forEach(mv => mv.isUpdated = false);
        });
    }

    updateMetricValues() {
        let activeObject: ReplaySubject<boolean> = new ReplaySubject(1);
        this.metricValueService.updateMetricAndReasonValues(
            this.metricWeekInputViewModelArray.map(metricWeekVM => metricWeekVM.metricValueArray),
            [],
            this.currentMetricValues)
            .subscribe(resp => {
                this.assignCreatedMetricValues(resp, this.metricWeekInputViewModelArray);
                this.removeAllUpdatedFlags();
                this.currentMetricValues = this.recalcMetricValues();
                this.weekSelectorService.allowSelectForMetric();
                let updatedReasonList = this.reasonLists.map(reasonList => reasonList.updateReasonValues());
                Observable.combineLatest(updatedReasonList)
                    .subscribe(resp => {
                        this.reasonLists.forEach(rl => {
                            this.assignCreatedReasonValues(resp, rl.reasonWeekInputViewModelArray);
                            rl.removeAllUpdatedFlags();
                            rl.currentReasonValues = rl.recalcMetricValues();
                            this.weekSelectorService.allowSelectForReason();
                        });
                        activeObject.next(true);
                    });
            });
        return activeObject;
    }

    assignCreatedMetricValues(resp: any, metricWeekInputModelArray: WeekInputViewModel[]) {
        let respMetricValues: Array<MetricValue[]> = resp['metricValues'] as Array<MetricValue[]>;
        this.metricWeekInputViewModelArray.forEach(metricWeekInputVM => {
            metricWeekInputVM.metricValueArray.forEach(metricVal => {
                respMetricValues.forEach(metricArray => {
                    metricArray.forEach(respMetricVal => {
                        if (respMetricVal.metric.id === metricVal.metric.id) {
                            let respDate = new Date(respMetricVal.inputDate);
                            if (respDate.getMonth() === metricVal.inputDate.getMonth() && respDate.getDate() === metricVal.inputDate.getDate() && metricVal.id === 0)
                                metricVal.id = respMetricVal.id;
                        }
                    });
                });
            });
        });
    }

    assignCreatedReasonValues(resp: any, reasonWeekInputModelArray: WeekInputViewModel[]) {
        let respReason = resp.filter(r => r !== undefined);
        if (respReason.length === 0)
            return;
        let respReasonValues: Array<ReasonValue[]> = respReason[0]['reasonValues'] as Array<ReasonValue[]>;
        reasonWeekInputModelArray.forEach(reasonWeekInputVM => {
            reasonWeekInputVM.reasonValueArray.forEach(reasonVal => {
                respReasonValues.forEach(reasonArray => {
                    reasonArray.forEach(respReasonVal => {
                        if (respReasonVal.reason.id === reasonVal.reason.id) {
                            let respDate = new Date(respReasonVal.inputDate);
                            if (respDate.getMonth() === reasonVal.inputDate.getMonth() && respDate.getDate() === reasonVal.inputDate.getDate() && reasonVal.id === 0)
                                reasonVal.id = respReasonVal.id;
                        }
                    });
                });
            });
        });
    }
    
    recalcMetricValues() {
        return this.metricValueService.getMetricReasonValuesJSON(this.metricWeekInputViewModelArray, []);
    }

    editMetricClick(metric: Metric) {
        this.currentMetricId = metric.id;
        this.priviousMetricStatus = metric.metricState;
        this.currentMetric = metric;
        this.modalEditMetric.open();
    }

    addMetricClick() {
        this.modalAddMetric.open();
    }

    opened() {
        this.addMetricPopUp.open(this.currentIssue.id);
    }

    editMetricOpened() {
        this.editMetricPopUp.open(this.currentIssue, this.currentMetric);
    }

    afterCloseNewMetric(toAddMetric: Metric) {
        this.modalAddMetric.close();
        if (!toAddMetric.id)
            return;
        let viewMode = new MetricViewModel();
        viewMode.metric = toAddMetric;
        viewMode.expanded = false;
        this.metricArray.push(viewMode);
        this.rebuildWeekInputViewModel();
        this.currentIssue.activeMetricCount++;
    }

    afterCloseEditMetric(toEditMetric: Metric) {
        this.modalEditMetric.close();
        this.metricArray.forEach(metric => {
            if (metric.metric.id == toEditMetric.id) {
                metric.metric = toEditMetric;
                return;
            }
        });
    }

    afterDeleteMetric(deletedMetric: Metric) {
        this.modalEditMetric.close();
        this.metricArray.forEach((metric, index) => {
            if (metric.metric.id == deletedMetric.id) {
                this.metricArray.splice(index, 1);
                return;
            }
        });
        if (deletedMetric.metricState == State.active && this.currentIssue.activeMetricCount > 0) {
            this.reduiceActiveMetricCount();
        }
    }
}
