# Match Collaboration API

⚠️ _This API is under construction and is not live_

The Match Collaboration API is an Internal API for resolving matching participant records between states/territories.

## Summary

When a match is found in the system, the states/territories involved in the match must communicate with one another to resolve it. Two or more entities may be involved in resolving a match. The match must be given a final disposition.

For example, if a state eligibility worker in Montana queries a participant in their system and finds a matching participant in Iowa, Iowa and Montana will work together to either transfer benefits from one state to another, or cancel benefits entirely.

As shown in the [Duplicate Participation API](../../docs/openapi/generated/duplicate-participation-api/openapi.md), when a match query is performed and results in a match, a match record is created for each matching pair of states/territories. The Match ID's for each match record are returned in the query. An eligibility worker can then use these Match ID's to collaborate with their counterparts from other programs.

The Match Collaboration API will focus on facilitating this collaboration.

Currently, a single Match (and a single Match ID) pertain to a pair of entities: the querying entity (e.g. Montana) and the matching entity (e.g. Iowa). Although improbable, it's possible to have multiple pairs returned in a single match query (e.g. Montana and Iowa, Montana and Louisiana). Each matching pair will need to be resolved separately.

## Schema and Usage

Please refer to our [Openapi documentation](./openapi/collaboration/index.yaml)

## Environment variables

⚠️ _Coming Soon_

## Local development

⚠️ _Coming Soon_

## Unit / integration tests

⚠️ _Coming Soon_

## App deployment

⚠️ _Coming Soon_

## Remote testing

⚠️ _Coming Soon_
