/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, Input, Output, EventEmitter, ViewChild, ViewChildren, QueryList, ChangeDetectorRef } from '@angular/core';
import { Observable } from 'rxjs/Rx';
import { Router, ActivatedRoute } from '@angular/router';
import { CookieService } from '../services/cookie.service';
import { IssueService } from '../services/issue.service';
import { Issue } from '../shared/models/issue';
import { Metric } from '../shared/models/metric';
import { Reason } from '../shared/models/reason';
import { Category } from '../shared/models/category';
import { IssueState } from '../shared/models/issueState';
import { IssueViewModel } from './issue.viewmodel';
import { AllowIssueClick } from '../shared/models/allowIssueClick';
import { IssueStateViewModel } from '../shared/models/issueState.viewmodel';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { AddIssueComponent } from '../issue/addIssue.component';
import { EditIssueComponent } from '../issue/editIssue.component';
import { HeaderComponent } from '../header/header.component';
import { QueryResult } from '../shared/models/queryResult';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';
import { MetricListComponent } from '../metric/metricList.component';
import { WeekSelectorService } from '../services/weekSelector.service';
import { WeekDay } from '../shared/models/weekDay';
import { MetricValueService } from '../services/metricValue.service';
import { CommonConfirmComponent } from '../confirm/common.confirm.component';

declare var microsoftTeams: any;
declare var jQuery: any;

@Component({
    templateUrl: './issueList.component.html',
    selector: 'issue-list',
    styleUrls: ['./issueList.component.css', '../shared/shared.css']
})

export class IssueListComponent implements OnInit, AfterViewChecked {
    @Input('allowClick') allowClick: AllowIssueClick;
    @Output() afterCheckAllowClick: EventEmitter<boolean> = new EventEmitter<boolean>();

    issueArray = new Array<IssueViewModel>();
    selectedIssueState = new IssueStateViewModel();
    isCreateBtnVisible = true;
    selectedIssue: IssueViewModel;
    teamId = Constants.teamId;
    isNewIssueButtonClicked: boolean;
    toEditIssue: Issue;
    isRequestCompleted: boolean;
    enable: boolean;
    toExpandIssue: IssueViewModel;
    isDefaultExpanded: boolean = false;
    toSelectedQuery: QueryResult;

    @ViewChild(AddIssueComponent) private addIssue: AddIssueComponent;
    @ViewChild(EditIssueComponent) private editIssue: EditIssueComponent;
    @ViewChild(HeaderComponent) private header: HeaderComponent;
    @ViewChildren('metricLists') metricLists: QueryList<MetricListComponent>;
    @ViewChild('modalAddIssue') modalAddIssue: ModalComponent;
    @ViewChild('modalEditIssue') modalEditIssue: ModalComponent;
    @ViewChild(CommonConfirmComponent) confirmPopup: CommonConfirmComponent;
    @ViewChild('expandConfirm') confirmExpandPopup: CommonConfirmComponent;
    @ViewChild('filterConfirm') confirmFilterPopup: CommonConfirmComponent;
    @ViewChild('queryConfirm') confirmQueryPopup: CommonConfirmComponent;

    constructor(private issueService: IssueService, private router: Router, private activateRoute: ActivatedRoute, private cookieService: CookieService, private cdRef: ChangeDetectorRef, private weekSelectorService: WeekSelectorService, private metricValueService: MetricValueService) {
    }

    ngOnInit(): void {
        if (CommonUtil.isInMsTeam()) {
            this.initTeamContext();
        } else {
            this.initTeamContext();
        }
        this.subscribeWeekSelector();
        this.isNewIssueButtonClicked = false;
        this.isRequestCompleted = false;
    }

    ngAfterViewChecked() {
        if (this.isDefaultExpanded === false)
            this.expandDefaultIssue();
        this.cdRef.detectChanges();
    }

    initIssues() {
        this.initIssueStates();
    }

    isMetricUrl(): boolean {
        return location.pathname.indexOf(Constants.route.metricIssue) >= 0;
    }

    getIssueId(): number {
        let issueIdStr = this.cookieService.get("issueId");
        if (issueIdStr != '')
            return parseInt(issueIdStr);
        return 0;
    }

    initTeamContext() {
        this.teamId = CommonUtil.getTeamId();
        this.initIssues();
    }

    initIssueStates() {
        this.selectedIssueState = this.header.selectedIssueState;
        let issueId = this.getIssueId();
        if (issueId > 0) {
            this.issueService.getIssueById(issueId)
                .subscribe(issue => {
                    this.selectedIssue = new IssueViewModel();
                    this.selectedIssue.Issue = issue;
                    this.selectedIssueState = this.selectedIssueState;
                    this.doFilterIssues(this.selectedIssueState.value);
                });
        } else {
            this.selectedIssueState = this.selectedIssueState;
            this.doFilterIssues(this.selectedIssueState.value);
        }
    }

    doNavigateIssue(item?: IssueViewModel) {
        if (item && item.Issue) {
            this.cookieService.put("issueId", item.Issue.id.toString());
        }
    }

    doNavigate() {
        if (!this.isNewIssueButtonClicked)
            this.doNavigateIssue(this.selectedIssue);
        else
            this.doNavigateIssue();
    }

    doFilterIssues(state: number) {
        this.issueService.filterIssueList(state, this.teamId)
            .subscribe(resp => {
                this.issueArray = resp.map((issue, index) => {
                    let issueModel = new IssueViewModel();
                    issueModel.Issue = issue;
                    issueModel.IsSelected = false;
                    return issueModel;
                });
                this.isRequestCompleted = true;
            });
    }

    createIssue() {
        this.isNewIssueButtonClicked = true;
        this.afterCheckAllowClick.emit(false);
        if (this.allowClick.allowClick) {
            this.addIssue.open();
        }
    }

    updateIssue(updatedIssue: Issue) {
        let findResult = this.issueArray.filter(issue => issue.Issue.id == updatedIssue.id);
        if (findResult.length == 0) return;

        findResult[0].Issue.name = updatedIssue.name;
        findResult[0].Issue.metric = updatedIssue.metric;
    }

    afterFilterIssue(issueState: IssueStateViewModel) {
        if (this.detectInputValueChange()) {
            this.selectedIssueState = issueState;
            this.confirmFilterPopup.open();
            return;
        } else {
            this.doFilterIssues(issueState.value);
        }
    }

    afterQuerySelected(selectedItem: QueryResult) {
        if (this.detectInputValueChange()) {
            if (typeof selectedItem !== 'string') {
                this.toSelectedQuery = selectedItem;
                this.confirmQueryPopup.open();
            }
        } else {
            this.doSelectQueryResult(selectedItem);
        }
    }

    doSelectQueryResult(selectedItem: QueryResult) {
        let selectedIssue: IssueViewModel;
        if (selectedItem['category'] !== undefined) { //issue
            let searchedIssues = this.issueArray.filter((issue, index) => {
                return issue.Issue.id == selectedItem.id;
            });
            if (searchedIssues.length > 0) {
                selectedIssue = searchedIssues[0];
            }
        } else if (selectedItem['issue'] !== undefined) { //metric
            this.issueArray.forEach(issue => {
                if (issue.Issue.id == selectedItem['issue'].id) {
                    selectedIssue = issue;
                }
            });
        } else { //reason
            this.issueArray.forEach(issue => {
                if (issue.Issue.id == selectedItem['metric'].issue.id) {
                    selectedIssue = issue;
                }
            });
        }
        if (selectedIssue != null) {
            this.expandIssueWithoutCheck(selectedIssue);
            this.scrollScreen();
        }
    }

    afterSaveChangesCancelQuery() {
        let self = this;
        setTimeout(function () {
            self.doSelectQueryResult(self.toSelectedQuery);
        }, 1500);
    }

    afterSaveChangesConfirmQuery() {
        this.saveIssueClick(this.selectedIssue)
            .subscribe(results => {
                this.doSelectQueryResult(this.toSelectedQuery);
            });
    }
    
    addIssueClick() {
        this.modalAddIssue.open();
    }

    closed() {
    }

    dismissed() {
    }

    opened() {
        this.addIssue.open();
    }

    editIssueOpened() {
        this.editIssue.open(this.toEditIssue.id);
    }
    
    onSwitch(a: any) {
        console.log(a);
    }

    checkAllowWeekClick(event: boolean) {
        this.header.checkAllowWeekClick(event);
    }

    expandDefaultIssue() {
        if (this.issueArray.length === 0)
            return;
        if (this.issueArray.filter(issue => issue.Expanded == true).length === 0) {
            this.expandIssue(this.issueArray[0]);
            this.isDefaultExpanded = true;
        }
    }

    hideIssueRelatedMetricList(issue: IssueViewModel) {
        let metricList = this.getRelatedMetricList(issue);
        if (metricList !== null)
            metricList.hide();
    }

    showIssueRelatedMetricList(issue: IssueViewModel) {
        let currentMetricList = this.getRelatedMetricList(issue);
        if (currentMetricList !== null)
            currentMetricList.show();
    }

    expandIssueClick(issue: IssueViewModel) {
        if (issue.Expanded) {
            issue.Expanded = false;
            this.hideIssueRelatedMetricList(issue);
        } else {
            this.expandIssue(issue);
        }
    }

    expandIssue(issue: IssueViewModel) {
        if (this.detectInputValueChange()) {
            this.toExpandIssue = issue;
            this.confirmExpandPopup.open();
            return;
        } else {
            this.expandIssueWithoutCheck(issue);
        }
    }

    expandIssueWithoutCheck(issue: IssueViewModel) {
        this.issueArray.forEach(issue => {
            issue.Expanded = false;
            this.hideIssueRelatedMetricList(issue);
        });
        issue.Expanded = true;
        this.selectedIssue = issue;
        this.showIssueRelatedMetricList(issue);
    }

    detectInputValueChange() {
        if (this.selectedIssue === null || this.selectedIssue === undefined)
            return false;
        let currentMetricList = this.getRelatedMetricList(this.selectedIssue);
        if (currentMetricList === null || currentMetricList === undefined)
            return false;
        let isMetricValueChanged = currentMetricList.isInputValueChanged();
        if (currentMetricList.reasonLists.length == 0)
            return isMetricValueChanged;
        let isReasonValueChanged = currentMetricList.reasonLists
            .map(reasonList => reasonList.isInputValueChanged())
            .reduce((x, y) => x || y);
        return isMetricValueChanged || isReasonValueChanged;
    }

    afterSaveChangesCancelExpand() {
        let self = this;
        setTimeout(function () {
            self.expandIssueWithoutCheck(self.toExpandIssue);
        }, 1500);
    }

    afterSaveChangesCancelFilter() {
        let self = this;
        setTimeout(function () {
            self.doFilterIssues(self.selectedIssueState.value);
        }, 1500);
    }

    afterSaveChangesExpandConfirm() {
        this.saveIssueClick(this.selectedIssue)
            .subscribe(results => {
                this.expandIssueWithoutCheck(this.toExpandIssue);
            });
    }

    afterSaveChangesFilterConfirm() {
        this.saveIssueClick(this.selectedIssue)
            .subscribe(results => {
                this.doFilterIssues(this.selectedIssueState.value);
            });
    }

    getRelatedMetricList(issue: IssueViewModel) {
        let result = this.metricLists.filter(metricList => metricList.currentIssue.id == issue.Issue.id);
        if (result.length > 0)
            return result[0];
        return null;
    }

    scrollScreen(): void {
        setTimeout(function () {
            let expandedIssue = jQuery("div.issue-section.expanded");
            if (expandedIssue.offset()) {
                let top = expandedIssue.offset().top;
                jQuery("html").animate({ scrollTop: top }, 600);
            }
        }, 1500);
    }

    subscribeWeekSelector() {
        this.weekSelectorService.selectWeek.subscribe(weekDay => {
        });
        this.weekSelectorService.selectClick.subscribe(checkClick => {
            let self = this;
            setTimeout(function () {
                if (self.weekSelectorService.isAllowClick() === false) {
                    self.confirmPopup.open();
                }
            }, 1000);
        });
    }

    afterSaveChangesCancel() {
        let self = this;
        setTimeout(function () {
            self.weekSelectorService.continueChangeWeek();
        }, 1500);
    }

    afterSaveChangesConfirm() {
        this.saveIssueClick(this.selectedIssue)
            .subscribe(results => {
                this.weekSelectorService.continueChangeWeek();
            });
    }

    editIssueClick(issue: IssueViewModel) {
        this.toEditIssue = issue.Issue;
        this.modalEditIssue.open();
    }

    saveIssueClick(issue: IssueViewModel) {
        let currentMetricList = this.getRelatedMetricList(issue);
        return currentMetricList.updateMetricValues();
    }

    afterCloseNewIssue(toAddIssue: Issue) {
        this.modalAddIssue.close();
        if (!toAddIssue.id || toAddIssue.id <= 0)
            return;
        let newIssueModel = new IssueViewModel();
        newIssueModel.IsSelected = false;
        newIssueModel.IssueState = IssueState.active.toString();
        newIssueModel.Issue = toAddIssue;
        this.issueArray.push(newIssueModel);
    }

    afterCloseEditIssue(toEditIssue: Issue) {
        this.modalEditIssue.close();
        this.issueArray.forEach((issue, index) => {
            if (issue.Issue.id == toEditIssue.id) {
                if (issue.Issue.issueState != toEditIssue.issueState) {
                    this.issueArray.splice(index, 1);
                    return;
                }
                issue.Issue.category = toEditIssue.category;
                issue.Issue.issueState = toEditIssue.issueState;
                issue.Issue.name = toEditIssue.name;
                issue.Issue.owner = toEditIssue.owner;
                return;
            }
        });
    }

    afterDeleteIssue(deletedIssue: Issue) {
        this.modalEditIssue.close();
        this.issueArray.forEach((issue, index) => {
            if (issue.Issue.id == deletedIssue.id) {
                this.issueArray.splice(index, 1);
                return;
            }
        });
    }
}
