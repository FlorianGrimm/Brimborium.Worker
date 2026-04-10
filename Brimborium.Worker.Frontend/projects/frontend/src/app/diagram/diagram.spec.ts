import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Diagram } from './diagram';

describe('Diagram', () => {
  let component: Diagram;
  let fixture: ComponentFixture<Diagram>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Diagram],
    }).compileComponents();

    fixture = TestBed.createComponent(Diagram);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
