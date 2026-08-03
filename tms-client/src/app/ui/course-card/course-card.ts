import { Component, input, output } from '@angular/core';
import { Router } from '@angular/router';
import { Course } from '../../models/course.model';


@Component({
  selector: 'tms-course-card',
  standalone: true,
  imports: [],
  templateUrl: './course-card.html',
  styleUrl: './course-card.scss'
})
export class CourseCard {

  course = input.required<Course>();

  enrollClicked = output<Course>();

  constructor(private router: Router) {}

  viewDetails() {
    this.router.navigate(['/courses', this.course().id]);
  }

}