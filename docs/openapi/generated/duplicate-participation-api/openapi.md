<!-- Generator: Widdershins v4.0.1 -->

<h1 id="duplicate-participation-api">Duplicate Participation API v1.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API for the Duplicate Participation system where matching and lookups will occur

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

> A request with values for all fields

```json
{
  "query": {
    "first": "string",
    "middle": "string",
    "last": "string",
    "ssn": "000-00-0000",
    "dob": "1970-01-01"
  }
}
```

> Example responses

> A query returning a single match

```json
{
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
      ]
    }
  ]
}
```

> A query returning no matches

```json
{
  "lookup_id": null,
  "matches": []
}
```

> A query returning multiple matches

```json
{
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
      ]
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
      "benefits_end_month": null
    }
  ]
}
```

<h3 id="query-for-matches-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Matching PII records, if any exist|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request. Missing one of the required properties in the request body.|None|

<h3 id="query-for-matches-responseschema">Response Schema</h3>

Status Code **200**

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» lookup_id|string¦null|false|none|the identifier of the match request|
|» matches|[object]|false|none|none|
|»» first|string|false|none|First name|
|»» middle|string|false|none|Middle name|
|»» last|string|true|none|Last name|
|»» ssn|string|true|none|Social Security number|
|»» dob|string(date)|true|none|Date of birth|
|»» state|string|false|none|State/territory two-letter postal abbreviation|
|»» state_abbr|string|false|none|State/territory two-letter postal abbreviation. Deprecated, superseded by `state`.|
|»» exception|string|false|none|Placeholder for value indicating special processing instructions|
|»» case_id|string|false|none|Participant's state-specific case identifier|
|»» participant_id|string|false|none|Participant's state-specific identifier. Must not be social security number or any personal identifiable information.|
|»» benefits_end_month|string|false|none|Participant's ending benefits month|
|»» recent_benefit_months|[string]|false|none|List of up to the last 3 months that participant received benefits, in descending order. Each month is formatted as ISO 8601 year and month. Does not include current benefit month.|

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

*get the original match data related to a Lookup ID*

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
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|original active match data|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Not Found|None|

<h3 id="get-lookups-by-id-responseschema">Response Schema</h3>

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

