import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

import { Course } from '../models/course.model';


interface PagedResponse {
  items: Course[];
  totalCount: number;
  page: number;
  pageSize: number;
}


@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private http = inject(HttpClient);

  private baseUrl =
    'http://localhost:5000/api/courses';


  getAll(page = 1, pageSize = 50) {

    return this.http
      .get<PagedResponse>(this.baseUrl, {
        params: {
          page: page.toString(),
          pageSize: pageSize.toString()
        }
      })
      .pipe(
        map(response => response.items)
      );

  }


  getById(id: number) {

    return this.http.get<Course>(
      `${this.baseUrl}/${id}`
    );

  }

}