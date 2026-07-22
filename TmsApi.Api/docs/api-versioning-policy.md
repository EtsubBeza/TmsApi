# TMS API Versioning Policy

## Purpose

The TMS API uses versioning to allow improvements without breaking existing
clients. Breaking changes are introduced only through a new API version.

The API uses URL segment versioning:

/api/v1/courses
/api/v2/courses

---

## Breaking Changes

The following changes require a new API version:

- Removing an existing response field.
- Renaming an existing field.
- Changing the meaning or data type of a field.
- Changing HTTP status codes clients depend on.
- Tightening validation rules that reject previously valid requests.
- Changing default behavior such as sorting or filtering.
- Removing or changing an existing endpoint.

---

## Non-Breaking Changes

The following changes can be released without creating a new version:

- Adding a new optional response field.
- Adding a new endpoint.
- Adding a new optional query parameter.
- Adding additional supported values without changing existing behavior.
- Improving internal implementation details.

---

## Sunset Policy

When a new API version is released, previous versions remain available for a
minimum of six months.

During the migration period, deprecated versions return:

- Deprecation header
- Sunset header
- Link header pointing to the successor version

This allows clients enough time to migrate safely.

---

## Communication Process

When an API version is deprecated:

- A CHANGELOG entry is created.
- Teams using API keys are notified.
- Migration instructions are provided.
- A calendar reminder is created for the shutdown date.

---

## Version Skipping

Clients are not required to migrate through every version.

For example:

V1 → V3

is allowed if V3 provides the required migration path.

Each API version is treated as an independent contract.