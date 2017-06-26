﻿declare let describe, it, beforeEach, expect, spyOn;
import { async, inject, TestBed, ComponentFixture } from '@angular/core/testing';
import { Provider } from "@angular/core";
import { Router, ActivatedRoute, RouterModule, Routes } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { RouterLinkStubDirective } from '../../shared/mock/mock.routerLink';
import { AppModule } from '../../../app/app.module';
import { LeaveReportService } from '../leaveReport.service';
import { MockLeaveReportService } from '../../shared/mock/mock.leaveReport.service';
import { StringConstant } from '../../shared/stringConstant';
import { LeaveReportDetailsComponent } from './leaveReport-Details.component';
import { LoaderService } from '../../shared/loader.service';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { LeaveReport } from '../../leaveReport/leaveReport-List/leaveReport-List.model';
import { LeaveReportDetail } from '../../leaveReport/leaveReport-Details/leaveReport-Details.model';
let promise: TestBed;
let stringConstant = new StringConstant();
import { JsonToPdfService } from '../../shared/jsontopdf.service';

describe('LeaveReport Detials Tests', () => {
    const routes: Routes = [];
    class MockLoaderService { }
    class MockActivatedRoute extends ActivatedRoute {
        constructor() {
            super();
            this.params = Observable.of({ id: "1" });
        }
    };

    beforeEach(async(() => {
        this.promise = TestBed.configureTestingModule({
            declarations: [RouterLinkStubDirective], //Declaration of mock routerLink used on page.
            imports: [AppModule, RouterModule.forRoot(routes, { useHash: true }) //Set LocationStrategy for component. 
            ],
            providers: [
                { provide: ActivatedRoute, useClass: MockActivatedRoute },
                { provide: LeaveReportService, useClass: MockLeaveReportService },
                { provide: StringConstant, useClass: StringConstant },
                { provide: LoaderService, useClass: MockLoaderService }
            ]
        }).compileComponents();
    }));


    it('Shows list of leave reports Details', () => {
        let fixture = TestBed.createComponent(LeaveReportDetailsComponent); //Create instance of component  
        let taskMailDetailsComponent = fixture.componentInstance;
        let result = taskMailDetailsComponent.ngOnInit();
        expect(taskMailDetailsComponent.leaveReportDetail.length).toBe(0);
    });


    it('Downloads report of leave reports on export to pdf', () => {
        let mockLeaveReportDetails = new Array<MockLeaveReportDetails>();
        let mockLeaveReportDetail = new MockLeaveReportDetails();
        mockLeaveReportDetail.EmployeeUserName = stringConstant.userEmail;
        mockLeaveReportDetail.EmployeeName = stringConstant.userName;
        mockLeaveReportDetail.LeaveFrom = stringConstant.leaveDate;
        mockLeaveReportDetails.push(mockLeaveReportDetail);

        let fixture = TestBed.createComponent(LeaveReportDetailsComponent); //Create instance of component  
        let jsPDFMock = fixture.debugElement.injector.get(JsonToPdfService);
        spyOn(jsPDFMock, "exportJsonToPdf").and.callFake(function fake() { });          
        let leaveReportDetailsComponent = fixture.componentInstance;
        leaveReportDetailsComponent.leaveReportDetail = mockLeaveReportDetails;
        leaveReportDetailsComponent.exportDataToPdf();
        console.log(leaveReportDetailsComponent.leaveReportDetail.push());
        expect(leaveReportDetailsComponent.leaveReportDetail.length).toBe(1);
    });


    it('Shows details of leave report for an employee on initialization but there are no leave reports', () => {
        let mockLeaveReportDetails = new Array<MockLeaveReportDetails>();
        let fixture = TestBed.createComponent(LeaveReportDetailsComponent); //Create instance of component            
        let leaveReportDetailsComponent = fixture.componentInstance;
        let leaveReportService = fixture.debugElement.injector.get(LeaveReportService);
        spyOn(leaveReportService, stringConstant.getLeaveReportDetail).and.returnValue(new BehaviorSubject(mockLeaveReportDetails).asObservable());
        let result = leaveReportDetailsComponent.getLeaveReportDetail();
        expect(leaveReportDetailsComponent.leaveReportDetail.length).toBe(0);
    });


    it('Shows details of leave report for an employee on initialization', () => {
        let mockLeaveReportDetails = new Array<MockLeaveReportDetails>();
        let mockLeaveReportDetail = new MockLeaveReportDetails();
        mockLeaveReportDetail.EmployeeUserName = stringConstant.userEmail;
        mockLeaveReportDetail.EmployeeName = stringConstant.userName;
        mockLeaveReportDetail.LeaveFrom = stringConstant.leaveDate;
        mockLeaveReportDetails.push(mockLeaveReportDetail);

        let fixture = TestBed.createComponent(LeaveReportDetailsComponent); //Create instance of component            
        let leaveReportDetailsComponent = fixture.componentInstance;
        let leaveReportService = fixture.debugElement.injector.get(LeaveReportService);
        spyOn(leaveReportService, stringConstant.getLeaveReportDetail).and.returnValue(new BehaviorSubject(mockLeaveReportDetails).asObservable());
        let result = leaveReportDetailsComponent.getLeaveReportDetail();
        expect(leaveReportDetailsComponent.leaveReportDetail.length).toBe(0);

    });


});
class MockLeaveReport extends LeaveReport {
    constructor() {
        super();
    }
}

class MockLeaveReportDetails extends LeaveReportDetail {
    constructor() {
        super();
    }
}