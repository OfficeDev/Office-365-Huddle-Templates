/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { FormControl, FormGroup, Validators, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Metric } from '../shared/models/metric';
import { MetricService } from '../services/metric.service';
import { Constants } from '../shared/constants';
import { IssueService } from '../services/issue.service';
import { Issue } from '../shared/models/issue';
import { State } from '../shared/models/state';
import { ConfirmComponent } from '../confirm/confirm.component';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';

@Component({
    templateUrl: './editMetric.component.html',
    selector: 'edit-metric',
    styleUrls: ['./editMetric.component.css', '../shared/shared.css']
})

export class EditMetricComponent implements OnInit, AfterViewChecked {
    @Output() afterAddedIssue: EventEmitter<Metric> = new EventEmitter<Metric>();
    @Output() onClosed: EventEmitter<Metric> = new EventEmitter<Metric>();
    @Output() onDeleted: EventEmitter<Metric> = new EventEmitter<Metric>();
    @Input() issueId: string;

    isSaving = false;
    isShown = false;
    toEditMetric = new Metric();
    issue = new Issue();
    deleteTitle: string = '';

    @ViewChild(ConfirmComponent) private deletePopupComponent: ConfirmComponent;
    @ViewChild('modalDelete') modalDeletePopupContainer: ModalComponent;
    @ViewChild('metricForm') private metricForm: NgForm;

    constructor(private metricService: MetricService, private issueService: IssueService, private router: Router) {
    }

    ngOnInit(): void {
    }

    open(issue: Issue, metric: Metric): void {
        this.isShown = true;
        this.isSaving = false;
        this.issue = issue;
        this.toEditMetric.issue = issue;
        this.toEditMetric = metric;

    }

    close(): void {
        this.isShown = false;
        this.isSaving = false;
        this.onClosed.emit(this.toEditMetric);
    }

    updateValueType(valueType): void {
        this.toEditMetric.valueType = valueType;
    }

    saveMetric(): void {
        this.isSaving = true;
        this.toEditMetric.metricState = ((this.toEditMetric.metricState.toLocaleString() === 'false' || this.toEditMetric.metricState.toLocaleString() === '0') ? State.closed : State.active);
        this.metricService.editMetric(this.toEditMetric)
            .subscribe(result => {
                this.close();
            });
    }

    delete(): void {
        this.modalDeletePopupContainer.open();
    }

    deleteOpened(): void {
        this.deleteTitle = 'DELETE METRIC: "' + this.toEditMetric.name + '"';
        this.deletePopupComponent.open(Constants.metricListName, this.toEditMetric.id);
    }

    deleteDismissed(): void {
    }

    afterCloseDelete(closeParent): void {
        this.modalDeletePopupContainer.close();
        if (closeParent) {
            this.onDeleted.emit(this.toEditMetric);
            this.close();
        }
    }

    ngAfterViewChecked() {
    }
}
