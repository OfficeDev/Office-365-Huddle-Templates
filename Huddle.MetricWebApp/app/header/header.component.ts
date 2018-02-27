/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs/Rx';
import { IssueState } from '../shared/models/issueState';
import { IssueStateViewModel } from '../shared/models/issueState.viewmodel';
import { IssueViewModel } from '../issueList/issue.viewmodel';
import { Issue } from '../shared/models/issue';
import { Metric } from '../shared/models/metric';
import { QueryResult } from '../shared/models/queryResult';
import { Constants } from '../shared/constants';
import { QueryService } from '../services/query.service';
import { DataService } from '../services/data.service';
import { CommonUtil } from '../utils/commonUtil';
import { AllowClick } from '../shared/models/allowClick';
import { WeekDay } from '../shared/models/weekDay';
import { DateHelper } from '../utils/dateHelper';

@Component({
    templateUrl: './header.component.html',
    selector: 'header',
    styleUrls: ['./header.component.css', '../shared/shared.css']
})

export class HeaderComponent implements OnInit {
    @Input('displayTitle') displayTitle: string;
    @Output() filterIssueState: EventEmitter<IssueStateViewModel> = new EventEmitter<IssueStateViewModel>();
    @Output() selectQuery: EventEmitter<QueryResult> = new EventEmitter<QueryResult>();

    teamId: string;
    issueStates = new Array<IssueStateViewModel>();
    selectedIssueState: IssueStateViewModel;
    selectedQueryResult: QueryResult;

    allowWeekClick: AllowClick = new AllowClick(true);

    constructor(private queryService: QueryService, private dataService: DataService) {
        this.initIssueStates();
    }

    ngOnInit(): void {
        this.initTeamId();
    }

    initIssueStates() {
        let issueActive = new IssueStateViewModel();
        issueActive.title = IssueState[IssueState.active];
        issueActive.value = IssueState.active;
        this.issueStates.push(issueActive);

        let issueClosed = new IssueStateViewModel();
        issueClosed.title = IssueState[IssueState.closed];
        issueClosed.value = IssueState.closed;
        this.issueStates.push(issueClosed);

        this.selectedIssueState = issueActive;
    }

    initTeamId() {
        this.teamId = CommonUtil.getTeamId();
    }

    changeIssueFilter(issueState) {
        this.filterIssueState.emit(issueState);
    }

    suggestFormatter(data: Metric): string {
        return `${data.name}`;
    }

    suggestedQueryResultList = (keyword: any): Observable<QueryResult[]> => {
        keyword = keyword.trim();
        if (keyword && keyword.length >= Constants.suggestCharNum) {
            return this.queryService.searchQuery(this.selectedIssueState.value, keyword, this.teamId);
        } else {
            return Observable.of([]);
        }
    }

    selectQueryItem(selected: QueryResult) {
        this.selectQuery.emit(selected);
    }

    checkAllowWeekClick(event: boolean) {
        return this.allowWeekClick;
    }
}
