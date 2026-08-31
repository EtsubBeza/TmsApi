import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStore,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';

import {
  rxMethod
} from '@ngrx/signals/rxjs-interop';

import {
  pipe,
  switchMap,
  tap,
  catchError,
  EMPTY
} from 'rxjs';

import { EnrollmentService } from '../services/enrollment';
import { Enrollment } from '../models/enrollment';

export const EnrollmentStore = signalStore(
  { providedIn: 'root' },

  withState({
    enrollments: [] as Enrollment[],
    isLoading: false,
    error: null as string | null
  }),

  withComputed((store) => ({
    pendingCount: computed(() =>
      store.enrollments()
        .filter(e => e.status === 'Pending')
        .length
    )
  })),

  withMethods((store, api = inject(EnrollmentService)) => ({

    loadEnrollments: rxMethod<void>(
      pipe(
        tap(() => {
          patchState(store, {
            isLoading: true,
            error: null
          });
        }),

        switchMap(() =>
          api.getAll().pipe(

            tap((enrollments) => {
              patchState(store, {
                enrollments,
                isLoading: false
              });
            }),

            catchError((err) => {
              patchState(store, {
                isLoading: false,
                error:
                  err.message ??
                  'Failed to load enrollments.'
              });

              return EMPTY;
            })
          )
        )
      )
    ),

    approveEnrollment: rxMethod<string>(
  pipe(
    tap(() => {
      patchState(store, {
        error: null
      });
    }),

    switchMap((enrollmentId) =>
      api.approve(enrollmentId).pipe(

        tap(() => {
          patchState(store, {
            enrollments: store.enrollments().map(enrollment =>
              enrollment.id === enrollmentId
                ? { ...enrollment, status: 'Approved' }
                : enrollment
            )
          });
        }),

        catchError((err) => {
          patchState(store, {
            error:
              err.message ??
              'Failed to approve enrollment.'
          });

          return EMPTY;
        })
      )
    )
  )
)
  }))
);