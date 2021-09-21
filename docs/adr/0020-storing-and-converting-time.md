# 20. Storing and Converting Time

Date: 2021-09-16

## Status

Accepted

## Context

We use Postgresql for our databases. Postgres has two ways of storing times and timestamps: `without time zone` (default) and `with time zone`. There's no right one to use, but they [convert timezones differently](https://www.postgresql.org/docs/11/datatype-datetime.html) to and from the database, and the fact that `without time zone` is the default suggests this is Postgres' general preference.

Being consistent with how we store times throughout our databases should reduce complexity and cognitive overhead when working with times. Our users will be spread across all states and territories, so it's important to derive local timezones as easily and unambiguously as possible when displaying to users. So we should pick one data type or the other, but not use both.

### Converting to local time in Dotnet Apps

A `DateTime` in Dotnet has a `Kind` [property](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.kind?view=netcore-3.1) that indicates whether the time is based on local time, UTC, or neither. When a new `DateTime` instance doesn't explicitly specify a `Kind`, the default is `Unspecified` (neither).

Datetimes with unspecified Kinds can cause ambiguity when trying to convert times elsewhere, so it's best to specify Kind whenever possible.

## Decision

In Postgres, we will store `time` and `timestamp` values in the default manner `without time zone`. All times stored are assumed to be UTC. This means times should be converted to UTC before they are saved.

Over API's, all times will be in UTC.

When it's converted to a local timezone, a timestamp should travel as little as possible through an application to avoid possible downstream reconversion. For web apps, this usually means keeping the timestamp in UTC and converting it to local time only when rendering it as html.

When creating new DateTime instances in Dotnet, specify the Kind property.

## Consequences

- Being more explicit when creating DateTimes throughout the system
