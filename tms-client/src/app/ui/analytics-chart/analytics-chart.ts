import { Component, computed, input } from '@angular/core';
import { Enrollment } from '../../models/enrollment';

@Component({
  selector: 'tms-analytics-chart',
  standalone: true,
  templateUrl: './analytics-chart.html',
  styleUrl: './analytics-chart.scss'
})
export class AnalyticsChartComponent {

  data = input.required<Enrollment[]>();

  approvedHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status === 'Approved')
      .length;

    return Math.max(20, count * 3);
  });

  pendingHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status === 'Pending')
      .length;

    return Math.max(20, count * 3);
  });

  rejectedHeight = computed(() => {
    const count = this.data()
      .filter(e => e.status === 'Rejected')
      .length;

    return Math.max(20, count * 3);
  });
}