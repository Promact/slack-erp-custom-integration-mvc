﻿declare var describe, it, beforeEach, expect;
import { async, inject, TestBed, ComponentFixture } from '@angular/core/testing';
import { Provider } from "@angular/core";
import { Router, ActivatedRoute, RouterModule, Routes } from '@angular/router';
import { RouterLinkStubDirective } from '../../shared/mock/mock.routerLink';
import { ScrumModule } from '../scrumReport.module';
import { ScrumProjectDetailComponent } from './scrumProject-Details.component';
import { ScrumReportService } from '../scrumReport.service';
import { TestConnection } from "../../shared/mock/test.connection";
import { MockScrumReportService } from '../../shared/mock/mock.scrumReport.service';
import { Observable } from 'rxjs/Observable';
import { StringConstant } from '../../shared/stringConstant';

let promise: TestBed;


describe('ScrumReport Tests', () => {
    const routes: Routes = [];
    class MockActivatedRoute extends ActivatedRoute {
        constructor() {
            super();
            this.params = Observable.of({ id: "123" });
        }
    }

    beforeEach(async(() => {
        this.promise = TestBed.configureTestingModule({
            declarations: [RouterLinkStubDirective],
            imports: [ScrumModule, RouterModule.forRoot(routes, { useHash: true })],
            providers: [
                { provide: ActivatedRoute, useClass: MockActivatedRoute },
                { provide: ScrumReportService, useClass: MockScrumReportService },
                { provide: StringConstant, useClass: StringConstant },
            ]
        }).compileComponents();
    }));

    it('Shows scrum answers of employees in a project on initialization', () => done => {
        this.promise.then(() => {
            let fixture = TestBed.createComponent(ScrumProjectDetailComponent);
            let scrumProjectDetailsComponent = fixture.componentInstance;
            let result = scrumProjectDetailsComponent.ngOnInit();
            expect(scrumProjectDetailsComponent.employeeScrumAnswers.length).toBe(1);
            done();
        });
    });
});
