/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Component, OnInit, AfterViewChecked, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { FormControl, FormGroup, Validators, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { Issue } from '../shared/models/issue';
import { User } from '../shared/models/user';
import { IssueState } from '../shared/models/issueState';
import { Category } from '../shared/models/category';
import { Reason } from '../shared/models/reason';
import { AllowIssueClick } from '../shared/models/allowIssueClick';
import { IssueService } from '../services/issue.service';
import { ReasonService } from '../services/reason.service';
import { Constants } from '../shared/constants';
import { CommonUtil } from '../utils/commonUtil';
import { IssueListComponent } from '../issueList/issueList.component';
import { State } from '../shared/models/state';
import { ConfirmComponent } from '../confirm/confirm.component';
import { ModalComponent } from 'ng2-bs3-modal/ng2-bs3-modal';

declare var fabric: any;
declare var jQuery: any;

@Component({
    templateUrl: './editIssue.component.html',
    selector: 'edit-issue',
    styleUrls: ['./editIssue.component.css', '../shared/shared.css']
})

export class EditIssueComponent implements OnInit, AfterViewChecked {
    @Output() afterAddedIssue: EventEmitter<Issue> = new EventEmitter<Issue>();
    @Output() onClosed: EventEmitter<Issue> = new EventEmitter<Issue>();
    @Output() onDeleted: EventEmitter<Issue> = new EventEmitter<Issue>();

    @ViewChild('issueForm') private issueForm: NgForm;
    @ViewChild(ConfirmComponent) private deletePopupComponent: ConfirmComponent;
    @ViewChild('modalDelete') modalDeletePopupContainer: ModalComponent;

    selectedCategory = '';
    selectedUser = '';
    categories = new Array<Category>();
    users = new Array<User>();
    isSaving = false;
    teamId = '1';
    isShown = false;
    toEditIssue = new Issue();
    deleteTitle: string = '';

    constructor(private issueService: IssueService, private reasonService: ReasonService, private router: Router) {
    }

    ngOnInit(): void {
    }

    open(issueId): void {
        this.initTeamContext();
        jQuery("div.add-issue-dialog").find("li.ms-Dropdown-item").removeClass('is-selected');
        jQuery("div.add-issue-dialog").find('span.ms-Dropdown-title').html('');
        this.initIssue(issueId);
        this.isShown = true;
    }

    close(): void {
        this.isShown = false;
        this.isSaving = false;
        this.onClosed.emit(this.toEditIssue);
    }

    initIssue(issueId) {
        if (issueId > 0) {
            this.issueService.getIssueById(issueId)
                .subscribe(issue => {
                    this.toEditIssue.id = issue.id;
                    this.toEditIssue.name = issue.name;
                    this.toEditIssue.category = issue.category;
                    this.toEditIssue.owner = issue.owner;
                    this.toEditIssue.issueState = issue.issueState;
                    this.initUsers();
                    this.initCategories();
                });
        }
    }

    initUsers() {
        this.issueService.getUsers(this.teamId)
            .subscribe(resp => {
                this.users = resp;
                if (this.users.length > 0) {
                    for (var i = 0; i < this.users.length; i++) {
                        if (this.users[i].name == this.toEditIssue.owner) {
                            this.selectedUser = this.users[i].mail;
                            break;
                        }
                    }
                }
            });
    }

    initCategories() {
        this.issueService.getCategories()
            .subscribe(resp => {
                this.categories = resp;
                for (var i = 0; i < this.categories.length; i++) {
                    if (this.categories[i].name == this.toEditIssue.category) {
                        this.selectedCategory = this.categories[i].name;
                        break;
                    }
                }
            });
    }

    initTeamContext() {
        this.teamId = CommonUtil.getTeamId();
    }

    selectCategory(categoryName) {
        this.selectedCategory = categoryName;
    }

    selectUser(user) {
        this.selectedUser = user;
    }

    getCategoryByName(categoryName: string) {
        let filterResult = this.categories.filter(category => category.name == categoryName);
        if (filterResult.length > 0)
            return filterResult[0];
        return new Category(-1, '');
    }

    isSaveDisabled(): boolean {
        return !this.toEditIssue.name || !this.toEditIssue.metric || this.isSaving;
    }

    saveIssue(): void {
        this.isSaving = true;
        this.toEditIssue.owner = this.selectedUser;
        this.toEditIssue.category = this.getCategoryByName(this.selectedCategory);
        this.toEditIssue.issueState = ((this.toEditIssue.issueState.toLocaleString() === '0' || this.toEditIssue.issueState.toLocaleString() === 'false') ? IssueState.closed : IssueState.active);
        this.issueService.editIssue(this.toEditIssue)
            .subscribe(result => {
                if (result && result > 0) {
                    this.close();
                }
            });
    }
    
    delete(): void {
        this.modalDeletePopupContainer.open();
    }

    deleteOpened(): void {
        this.deleteTitle = 'DELETE ISSUE: "' + this.toEditIssue.name + '"';
        this.deletePopupComponent.open(Constants.issueListName, this.toEditIssue.id);
    }

    deleteDismissed(): void {
    }

    afterCloseDelete(closeParent): void {
        this.modalDeletePopupContainer.close();
        if (closeParent) {
            this.onDeleted.emit(this.toEditIssue);
            this.close();
        }
    }

    ngAfterViewChecked() {
    }
}
