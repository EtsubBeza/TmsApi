import { Component } from '@angular/core';
import { CourseCard } from '../course-card/course-card';
import { Course } from '../../models/course.model';

@Component({
  selector: 'tms-course-list',
  standalone: true,
  imports: [CourseCard],
  templateUrl: './course-list.html',
  styleUrl: './course-list.scss'
})
export class CourseList {

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

}