/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */

import { Injectable, Inject } from '@angular/core';
import { Observable, ReplaySubject } from 'rxjs/Rx';
import { DataService } from '../services/data.service';
import { Issue } from '../shared/models/issue';
import { User } from '../shared/models/user';
import { IssueViewModel } from '../issueList/issue.viewmodel';
import { Category } from '../shared/models/category';
import { Reason } from '../shared/models/reason';
import { Constants } from '../shared/constants';
import { ModelConverter } from '../utils/modelConverter';
import { DateHelper } from '../utils/dateHelper';
import { HandleError } from '../shared/handleError';

@Injectable()
export class IssueService {

    constructor(private dataService: DataService) { }

    public filterIssueList(state: number, teamId: string): Observable<Issue[]> {
        let activeObject: ReplaySubject<Issue[]> = new ReplaySubject(1);
        this.dataService.getArray<Issue>(Constants.webAPI.issuesFilterUrl + "/" + state + "/" + teamId + '/')
            .subscribe((resp) => {
                let result: Issue[] = [];
                resp.forEach(function (issue, index) {
                    issue.startDate = DateHelper.UTCToLocal(issue.startDate);
                    result.push(issue);
                }, this);
                activeObject.next(result);
            },
            (error) => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public getCategories(): Observable<Array<Category>> {
        let activeObject: ReplaySubject<Category[]> = new ReplaySubject(1);
        this.dataService.getArray<Category>(Constants.webAPI.categoriesUrl + '/')
            .subscribe((resp) => {
                let result: Issue[] = [];
                resp.forEach(function (category, index) {
                    result.push(category);
                }, this);
                activeObject.next(result);
            },
            (error) => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public getUsers(teamId: string): Observable<Array<User>> {
        let activeObject: ReplaySubject<User[]> = new ReplaySubject(1);
        this.dataService.getArray<User>(Constants.webAPI.teamsUrl + '/' + teamId)
            .subscribe((resp) => {
                let result: User[] = [];
                resp.forEach(function (user, index) {
                    result.push(user);
                }, this);
                activeObject.next(result);
            },
            (error) => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public getIssueById(issueId: number): Observable<Issue> {
        let activeObject: ReplaySubject<Issue> = new ReplaySubject(1);
        this.dataService.getObject<Issue>(Constants.webAPI.issuesUrl + "/" + issueId.toString())
            .subscribe((issue) => {
                issue.startDate = DateHelper.UTCToLocal(issue.startDate);
                issue.isMetricEditable = false;
                issue.isNameEditable = false;
                activeObject.next(issue);
            },
            (error) => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public addIssue(issue: Issue, teamId: string): Observable<Issue> {
        let activeObject: ReplaySubject<Issue> = new ReplaySubject(1);
        this.dataService.post(Constants.webAPI.issuesUrl, { issue: ModelConverter.ToIssueBackend(issue), teamId: teamId })
            .subscribe(
            resp => {
                activeObject.next(resp);
            },
            error => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public editIssue(issue: Issue): Observable<number> {
        let activeObject: ReplaySubject<number> = new ReplaySubject(1);
        this.dataService.post(Constants.webAPI.issueEditUrl, { issue: ModelConverter.ToIssueBackend(issue) })
            .subscribe(
            resp => {
                activeObject.next(resp.issueId);
            },
            error => {
                HandleError.handleError(error);
            });
        return activeObject;
    }

    public deleteIssue(id: number): Observable<number> {
        let activeObject: ReplaySubject<number> = new ReplaySubject(1);
        this.dataService.delete(Constants.webAPI.issueDeleteUrl + "/" + id)
            .subscribe(
            resp => {
                activeObject.next(resp.issueId);
            },
            error => {
                HandleError.handleError(error);
            });
        return activeObject;
    }
}
