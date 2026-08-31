import {
  AfterViewInit,
  Component,
  ViewChild,
  inject
} from '@angular/core';

import { MatTableDataSource } from '@angular/material/table';
import { MatTableModule } from '@angular/material/table';
import { MatSort, MatSortModule } from '@angular/material/sort';
import {
  MatPaginator,
  MatPaginatorModule
} from '@angular/material/paginator';

import { EnrollmentStore } from '../../store/enrollment-store';

@Component({
  selector: 'app-enrollment-list',
  standalone: true,
  imports: [
    MatTableModule,
    MatSortModule,
    MatPaginatorModule
  ],
  templateUrl: './enrollment-list.html',
  styleUrl: './enrollment-list.scss'
})
export class EnrollmentList implements AfterViewInit {

  store = inject(EnrollmentStore);

  displayedColumns: string[] = [
    'studentName',
    'courseName',
    'status',
    'actions'
  ];

  dataSource = new MatTableDataSource(
    this.store.enrollments()
  );

  @ViewChild(MatSort)
  sort!: MatSort;

  @ViewChild(MatPaginator)
  paginator!: MatPaginator;

  ngOnInit(): void {
    this.store.loadEnrollments();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
  }
}