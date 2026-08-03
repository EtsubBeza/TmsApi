import { Routes } from '@angular/router';

export const routes: Routes = [

  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/student-dashboard/student-dashboard.component').then(
        m => m.StudentDashboardComponent
      ),
  },

  {
    path: 'courses',
    loadComponent: () =>
      import('./ui/course-list/course-list').then(
        m => m.CourseList
      ),
  },

  {
    path: 'courses/:id',
    loadComponent: () =>
      import('./features/course-detail/course-detail').then(
        m => m.CourseDetail
      ),
  },

  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

];