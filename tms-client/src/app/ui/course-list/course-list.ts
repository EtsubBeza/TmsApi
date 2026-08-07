import { Component, input } from '@angular/core';
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

  courses = input<Course[]>([]);

}