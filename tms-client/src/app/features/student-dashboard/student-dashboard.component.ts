import { Component, computed, signal } from '@angular/core';
import { Course } from '../../models/course.model';
import { CourseList } from '../../ui/course-list/course-list';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [
    CourseList
  ],
  templateUrl: './student-dashboard.component.html',
  styleUrl: './student-dashboard.component.scss'
})
export class StudentDashboardComponent {

  studentName = signal('Liya Kebede');

  earnedCredits = signal(45);

  graduationStatus = computed(() =>
    this.earnedCredits() >= 120
      ? 'Eligible for Graduation'
      : 'In Progress'
  );

  registerForClass() {
    this.earnedCredits.update(c => c + 3);
  }

  selectedCourse = signal<Course | null>(null);

  handleEnroll(course: Course) {
    this.selectedCourse.set(course);
    console.log('Enrollment requested for:', course.title);
  }

}