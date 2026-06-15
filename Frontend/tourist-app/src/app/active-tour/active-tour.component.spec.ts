import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActiveTourComponent } from './active-tour.component';

describe('ActiveTourComponent', () => {
  let component: ActiveTourComponent;
  let fixture: ComponentFixture<ActiveTourComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ActiveTourComponent]
    });
    fixture = TestBed.createComponent(ActiveTourComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
