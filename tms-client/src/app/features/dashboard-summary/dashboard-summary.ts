
import { Component, inject } from '@angular/core';
import { EnrollmentStore } from '../../store/enrollment-store';

@Component({
  selector: 'tms-dashboard-summary',
  standalone: true,
  templateUrl: './dashboard-summary.html',
  styleUrl: './dashboard-summary.scss'
})
export class DashboardSummary {

  store = inject(EnrollmentStore);

}

