import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Course } from '../../models/course.model';

@Component({
  selector: 'tms-course-detail',
  standalone: true,
  imports: [],
  templateUrl: './course-detail.html',
  styleUrl: './course-detail.scss'
})
export class CourseDetail {

  private route = inject(ActivatedRoute);

  courseId = Number(this.route.snapshot.paramMap.get('id'));

  courses: Course[] = [
    {
      id: 1,
      code: 'CSE-101',
      title: 'Advanced Java Services',
      maxCapacity: 30,
      enrollmentCount: 12
    },
    {
      id: 2,
      code: 'CSE-201',
      title: 'Database Systems',
      maxCapacity: 40,
      enrollmentCount: 35
    },
    {
      id: 3,
      code: 'AI-201',
      title: 'Artificial Intelligence',
      maxCapacity: 25,
      enrollmentCount: 25
    }
  ];

  course = this.courses.find(
    c => c.id === this.courseId
  );

}