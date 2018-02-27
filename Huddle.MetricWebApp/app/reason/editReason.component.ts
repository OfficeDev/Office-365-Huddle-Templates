/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Reason } from '../shared/models/reason';
import { Metric } from '../shared/models/metric';
import { ReasonService } from '../services/reason.service';
import { MetricService } from '../services/metric.service';
import { IssueService } from '../services/issue.service';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { State } from '../shared/models/state';
import { FormControl, FormGroup, Validators, NgForm } from '@angular/forms';
import { ConfirmComponent } from '../confirm/confirm.component';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';

@Component({
    templateUrl: './editReason.component.html',
    selector: 'edit-reason',
    styleUrls: ['./editReason.component.css', '../shared/shared.css']
})

export class EditReasonComponent implements OnInit, AfterViewChecked {
    @Input() relatedMetric: Metric;
    @Output() onClosed: EventEmitter<Reason> = new EventEmitter<Reason>();

    toEditReason = new Reason();
    deleteTitle: string = '';

    @ViewChild(ConfirmComponent) private deletePopupComponent: ConfirmComponent;
    @ViewChild('modalDelete') modalDeletePopupContainer: ModalComponent;
    @ViewChild('reasonForm') private reasonForm: NgForm;

    constructor(private reasonService: ReasonService, private metricService: MetricService, private router: Router) {
    }

    ngOnInit(): void {
    }

    iniControls(reason: Reason): void {
        this.toEditReason = reason;
        this.toEditReason.metric = this.relatedMetric;
    }

    clearData(): void {
    }

    open(): void {
    }

    close(): void {
        this.onClosed.emit(this.toEditReason);
    }
    
    saveReason(): void {
        this.toEditReason.reasonState = ((this.toEditReason.reasonState.toLocaleString() === 'false' || this.toEditReason.reasonState.toLocaleString() === '0') ? State.closed : State.active);
        this.reasonService.editReason(this.toEditReason).subscribe(result => {
            this.close();
        });
    }
    
    delete(): void {
        this.modalDeletePopupContainer.open();
    }

    deleteOpened(): void {
        this.deleteTitle = 'DELETE REASON: "' + this.toEditReason.name + '"';
        this.deletePopupComponent.open(Constants.reasonListName, this.toEditReason.id);
    }

    deleteDismissed(): void {
    }

    afterCloseDelete(closeParent): void {
        this.modalDeletePopupContainer.close();
        if (closeParent) {
            this.close();
        }
    }

    ngAfterViewChecked() {
    }
}
