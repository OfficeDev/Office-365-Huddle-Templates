/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { Router } from '@angular/router';
import { Constants } from '../shared/constants';
import { IssueService } from '../services/issue.service';
import { ReasonService } from '../services/reason.service';
import { MetricService } from '../services/metric.service';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';

@Component({
    templateUrl: './common.confirm.component.html',
    selector: 'common-confirm-popup',
    styleUrls: ['./common.confirm.component.css', '../shared/shared.css']
})

export class CommonConfirmComponent implements OnInit {
    @Input('title') title: string;
    @Input('message') message: string;
    @Input('cancelTxt') cancelTxt: string = "cancel";
    @Input('confirmTxt') confirmTxt: string = "confirm";

    @Output() onCancled: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Output() onConfirmed: EventEmitter<boolean> = new EventEmitter<boolean>();

    @ViewChild('modalConfirm') modalConfirm: ModalComponent;

    constructor(private issueService: IssueService, private metricService: MetricService, private reasonService: ReasonService, private router: Router) {
    }

    ngOnInit(): void {
    }

    open(): void {
        this.modalConfirm.open();
    }

    close(): void {
        this.modalConfirm.close();
    }

    cancel(): void {
        this.onCancled.emit(true);
        this.close();
    }

    confirm(): void {
        this.onConfirmed.emit(true);
        this.close();
    }
}
