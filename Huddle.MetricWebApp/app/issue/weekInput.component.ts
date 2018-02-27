/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { WeekDay } from '../shared/models/weekDay';
import { WeekInputViewModel } from '../shared/models/weekInputViewModel';
import { DateHelper } from '../utils/dateHelper';
import { WeekSelectorService } from '../services/weekSelector.service';
import { ModelConverter } from '../utils/modelConverter';
import { MetricValueService } from '../services/metricValue.service';
import { MetricValue } from '../shared/models/metricValue';
import { ReasonValue } from '../shared/models/reasonValue';
import { Constants } from '../shared/constants';

@Component({
    templateUrl: './weekInput.component.html',
    selector: 'week-input',
    styleUrls: ['./weekInput.component.css', '../shared/shared.css']
})

export class WeekInputComponent implements OnInit {
    @Input('rowIndex') rowIndex: number;
    @Input('valueType') valueType: string;
    @Input('weekInputViewModel') weekInputViewModel: WeekInputViewModel;

    @Output() inputValueChange: EventEmitter<number> = new EventEmitter<number>();

    currentWeekDays: Array<Date>;
    errorMessage: string = '';
    currentValues: string;

    constructor(private weekSelectorService: WeekSelectorService, private metricValueService: MetricValueService) {
    }

    ngOnInit(): void {
        this.subscribeWeekSelector();
        this.currentWeekDays = this.weekSelectorService.getCurrentWeekDays();
        this.subscribeGetMetricReasonValues();
        this.subscribeUpdateMetricReasonValues();
    }

    subscribeWeekSelector() {
        this.weekSelectorService.selectWeek.subscribe(weekDay => {
            this.currentWeekDays = this.weekSelectorService.getCurrentWeekDays();
        });
    }

    subscribeGetMetricReasonValues() {
        this.metricValueService.getMetricReasonValuesEvent.subscribe(done => {
            this.currentValues = this.recalcMetricValues();
        });
    }

    subscribeUpdateMetricReasonValues() {
        this.metricValueService.updateMetricReasonValuesEvent.subscribe(done => {
            this.currentValues = this.recalcMetricValues();
        });
    }

    valueChange(valModel: object) {
        let valueUpdated = false;
        this.errorMessage = "";
        if (ModelConverter.isMetricValueFrontend(valModel)) {
            let metricVal = ModelConverter.toMetricValueFrontend(valModel);
            if (metricVal.metricValues.toString() === '')
                metricVal.metricValues = null;

            let checkResult = this.checkInput(metricVal.metricValues);
            if (!checkResult)
                return;
            if (metricVal.metricValues.toString() != '-')
                metricVal.metricValues = parseFloat(metricVal.metricValues.toString());
            this.inputValueChange.emit(metricVal.metricValues);
            valueUpdated = (this.currentValues !== this.recalcMetricValues());
        }
        if (ModelConverter.isReasonValueFrontend(valModel)) {
            let reasonVal = ModelConverter.toReasonValueFrontend(valModel);
            if (reasonVal.reasonMetricValues.toString() === '')
                reasonVal.reasonMetricValues = null;

            let checkResult = this.checkInput(reasonVal.reasonMetricValues);
            if (!checkResult)
                return;
            if (reasonVal.reasonMetricValues)
                reasonVal.reasonMetricValues = parseFloat(reasonVal.reasonMetricValues.toString());
            this.inputValueChange.emit(reasonVal.reasonMetricValues);
            valueUpdated = (this.currentValues !== this.recalcMetricValues());
        }
        if (valueUpdated === true) {
            this.weekInputViewModel.metricValueArray.forEach(metricVal => metricVal.isUpdated = false);
            this.weekInputViewModel.reasonValueArray.forEach(reasonVal => reasonVal.isUpdated = false);
            let updatedMetrics: Array<MetricValue[]> = this.metricValueService.getToPostMetricVals([this.weekInputViewModel.metricValueArray], this.currentValues);
            let updatedReasons: Array<ReasonValue[]> = this.metricValueService.getToPostReasonVals([this.weekInputViewModel.reasonValueArray], this.currentValues);
            if (updatedMetrics.length > 0 && updatedMetrics[0].length > 0)
                updatedMetrics[0].forEach(metricVal => metricVal.isUpdated = true);
            if (updatedReasons.length > 0 && updatedReasons[0].length > 0)
                updatedReasons[0].forEach(reasonVal => reasonVal.isUpdated = true);
        } else {
            this.weekInputViewModel.metricValueArray.forEach(metricVal => metricVal.isUpdated = false);
            this.weekInputViewModel.reasonValueArray.forEach(reasonVal => reasonVal.isUpdated = false);
        }
    }

    recalcMetricValues() {
        return this.metricValueService.getMetricReasonValuesJSON([this.weekInputViewModel], [this.weekInputViewModel], ['isUpdated']);
    }

    checkInput(inputValue: any) {
        if (!inputValue) {
            this.errorMessage = '';
            return false;
        }
        let regex = /^(\-|\+)?\d+(\.\d+)?$/;
        let regexp = new RegExp(regex);
        if (!regexp.test(inputValue)) {
            this.errorMessage = Constants.numberRequired;
            return false;
        }
        return true;
    }
}
