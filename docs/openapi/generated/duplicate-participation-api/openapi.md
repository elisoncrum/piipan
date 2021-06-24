<!-- Generator: Widdershins v4.0.1 -->

<h1 id="duplicate-participation-api">Duplicate Participation API v1.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API where matching and lookups will occur

Base URLs:

* <a href="/v1">/v1</a>

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="duplicate-participation-api-match">Match</h1>

## Query for Matches

<a id="opIdQuery for Matches"></a>

> Code samples

```shell
# You can also use wget
curl -X POST /v1/query \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`POST /query`

*Search for all matching PII records*

Queries all state databases for any PII records that are an exact match to the last name, date of birth, and social security number in the request body's `query` property.

> Body parameter

> An example request to query a single individual, with values for all fields

```json
{
  "query": [
    {
      "first": "string",
      "middle": "string",
      "last": "string",
      "ssn": "000-00-0000",
      "dob": "1970-01-01"
    }
  ]
}
```

<h3 id="query-for-matches-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|query|body|[[#/paths/~1query/post/requestBody/content/application~1json/schema/properties/query/items](#schema#/paths/~1query/post/requestbody/content/application~1json/schema/properties/query/items)]|true|none|

> Example responses

> A query for a single individual returning a single match

```json
{
  "data": [
    {
      "index": 0,
      "lookup_id": "string",
      "matches": [
        {
          "first": "string",
          "middle": "string",
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "ea",
          "state_abbr": "ea",
          "exception": "string",
          "case_id": "string",
          "participant_id": "string",
          "benefits_end_month": "2021-01",
          "recent_benefit_months": [
            "2021-05",
            "2021-04",
            "2021-03"
          ],
          "protect_location": true
        }
      ]
    }
  ]
}
```

> A query for a single individual returning no matches

```json
{
  "data": [
    {
      "index": 0,
      "lookup_id": null,
      "matches": []
    }
  ]
}
```

> A query for one individual returning multiple matches

```json
{
  "data": [
    {
      "index": 0,
      "lookup_id": "string",
      "matches": [
        {
          "first": "string",
          "middle": "string",
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "eb",
          "state_abbr": "eb",
          "exception": "string",
          "case_id": "string",
          "participant_id": "string",
          "benefits_end_month": "2021-01",
          "recent_benefit_months": [
            "2021-05",
            "2021-04",
            "2021-03"
          ],
          "protect_location": true
        },
        {
          "first": null,
          "middle": null,
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "ec",
          "state_abbr": "ec",
          "exception": null,
          "case_id": "string",
          "participant_id": null,
          "benefits_end_month": null,
          "protect_location": null
        }
      ]
    }
  ]
}
```

> A query for two individuals returning one match for each individual

```json
{
  "data": [
    {
      "index": 0,
      "lookup_id": "string",
      "matches": [
        {
          "first": null,
          "middle": null,
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "ec",
          "state_abbr": "ec",
          "exception": null,
          "case_id": "string",
          "participant_id": null,
          "benefits_end_month": null,
          "protect_location": null
        }
      ]
    },
    {
      "index": 1,
      "lookup_id": "string",
      "matches": [
        {
          "first": null,
          "middle": null,
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "ec",
          "state_abbr": "ec",
          "exception": null,
          "case_id": "string",
          "participant_id": null,
          "benefits_end_month": null,
          "protect_location": null
        }
      ]
    }
  ]
}
```

> A query for two individuals returning no matches for one individual and a match for the other

```json
{
  "data": [
    {
      "index": 0,
      "lookup_id": null,
      "matches": []
    },
    {
      "index": 1,
      "lookup_id": "string",
      "matches": [
        {
          "first": null,
          "middle": null,
          "last": "string",
          "ssn": "000-00-0000",
          "dob": "1970-01-01",
          "state": "ec",
          "state_abbr": "ec",
          "exception": null,
          "case_id": "string",
          "participant_id": null,
          "benefits_end_month": null,
          "protect_location": null
        }
      ]
    }
  ]
}
```

<h3 id="query-for-matches-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Successful response. Returns match response items.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request. Missing one of the required properties in the request body.|None|

<h3 id="query-for-matches-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|array|true|none|Array of match query response items. For every match query item provided in the request, a match response item is returned, even if no matches are found.|
|»» index|integer|true|none|The index of the request item that the response item corresponds to, starting from 0. Index is derived from the implicit order of match query request items provided in the request.|
|»» lookup_id|string¦null|false|none|The identifier of the match request item, if a match is present. This ID can be used for looking up the PII of the original match request item.|
|»» matches|[object]|false|none|none|
|»»» first|string|false|none|First name|
|»»» middle|string|false|none|Middle name|
|»»» last|string|true|none|Last name|
|»»» ssn|string|true|none|Social Security number|
|»»» dob|string(date)|true|none|Date of birth|
|»»» state|string|false|none|State/territory two-letter postal abbreviation|
|»»» state_abbr|string|false|none|State/territory two-letter postal abbreviation. Deprecated, superseded by `state`.|
|»»» exception|string|false|none|Placeholder for value indicating special processing instructions|
|»»» case_id|string|false|none|Participant's state-specific case identifier. Can be the same for multiple participants.|
|»»» participant_id|string|false|none|Participant's state-specific identifier. Is unique to the participant. Must not be social security number or any PII.|
|»»» benefits_end_month|string|false|none|Participant's ending benefits month|
|»»» recent_benefit_months|[string]|false|none|List of up to the last 3 months that participant received benefits, in descending order. Each month is formatted as ISO 8601 year and month. Does not include current benefit month.|
|»»» protect_location|boolean¦null|false|none|Location protection flag for vulnerable individuals. True values indicate that the individual’s location must be protected from disclosure to avoid harm to the individual. Apply the same protections to true and null values.|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

<h1 id="duplicate-participation-api-lookup">Lookup</h1>

## Get Lookups by ID

<a id="opIdGet Lookups by ID"></a>

> Code samples

```shell
# You can also use wget
curl -X GET /v1/lookup_ids/{id} \
  -H 'Accept: application/json' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`GET /lookup_ids/{id}`

*Get the original match data related to a Lookup ID*

User can provide a Lookup ID and receive the match data associated with it

> Example responses

> A response showing a query with values for all fields

```json
{
  "data": {
    "first": "string",
    "middle": "string",
    "last": "string",
    "ssn": "000-00-0000",
    "dob": "1970-01-01"
  }
}
```

> A response showing a query with values for only required fields

```json
{
  "data": {
    "first": "string",
    "last": "string",
    "ssn": "000-00-0000",
    "dob": "1970-01-01"
  }
}
```

<h3 id="get-lookups-by-id-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Successful response. Returns original match query request item.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Not found|None|

<h3 id="get-lookups-by-id-responseschema">Response Schema</h3>

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

