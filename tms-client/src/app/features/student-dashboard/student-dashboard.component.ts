
import { Component, signal, computed, inject } from '@angular/core';
import { Course } from '../../models/course.model';
import { CourseList } from '../../ui/course-list/course-list';
import { rxResource } from '@angular/core/rxjs-interop';
import { CourseService } from '../../services/course';
import { EnrollmentList } from '../enrollment-list/enrollment-list';
import { DashboardSummary } from '../dashboard-summary/dashboard-summary';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CourseList,
    EnrollmentList,
    DashboardSummary
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss'
})
export class StudentDashboardComponent {

  private api = inject(CourseService);

  studentName = signal('Liya Kebede');

  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120
      ? 'Eligible for Graduation'
      : 'In Progress'
  );

  coursesResource = rxResource({
    stream: () => this.api.getAll()
  });

  registerForClass() {
    this.earnedCredits.update(c => c + 3);
  }

  selectedCourse = signal<Course | null>(null);

  handleEnroll(course: Course) {
    this.selectedCourse.set(course);
    console.log('Enrollment requested for:', course.title);
  }

}

