import { Routes } from '@angular/router';
import { Diagram } from './diagram/diagram';

export const routes: Routes = [
  { path:"", pathMatch:"full", component: Diagram},
  { path:"ui", pathMatch:"full", component: Diagram}
];
