/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { FormControl, FormGroup, Validators, NgForm } from '@angular/forms';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { Router } from '@angular/router';
import { Metric } from '../shared/models/metric';
import { MetricService } from '../services/metric.service';
import { Constants } from '../shared/constants';
import { IssueService } from '../services/issue.service';
import { Issue } from '../shared/models/issue';

@Component({
    templateUrl: './addMetric.component.html',
    selector: 'add-metric',
    styleUrls: ['./addMetric.component.css', '../shared/shared.css']
})

export class AddMetricComponent implements OnInit, AfterViewChecked {
    @Output() afterAddedIssue: EventEmitter<Metric> = new EventEmitter<Metric>();
    @Output() onClosed: EventEmitter<Metric> = new EventEmitter<Metric>();
    @Input() issueId: string;

    isSaving = false;
    isShown = false;
    toAddMetric = new Metric();
    issue = new Issue();

    @ViewChild('metricForm') private metricForm: NgForm;

    constructor(private metricService: MetricService, private issueService: IssueService, private router: Router) {
    }

    ngOnInit(): void {
    }

    open(issueId): void {
        this.isShown = true;
        this.toAddMetric = new Metric();
        if (issueId > 0) {
            this.issueService.getIssueById(issueId)
                .subscribe(issue => {
                    this.issue.id = issue.id;
                    this.issue.name = issue.name;
                    this.toAddMetric.issue = issue;
                });
        }
        this.toAddMetric.valueType = "Numbers";
    }

    close(): void {
        this.isShown = false;
        this.isSaving = false;
        this.onClosed.emit(this.toAddMetric);
    }

    updateValueType(valueType): void {
        this.toAddMetric.valueType = valueType;
    }

    saveMetric(): void {
        this.isSaving = true;
        this.metricService.addMetric(this.toAddMetric)
            .subscribe(result => {
                this.toAddMetric = result;
                this.close();
            });
    }

    ngAfterViewChecked() {
    }
}
