
<h1 id="duplicate-participation-api">Duplicate Participation API v2.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API where matching will occur

Base URLs:

* <a href="/match/v2">/match/v2</a>

# Authentication

* API Key (ApiKeyAuth)
    - Parameter Name: **Ocp-Apim-Subscription-Key**, in: header. 

<h1 id="duplicate-participation-api-match">Match</h1>

## Find matches

<a id="opIdFind matches"></a>

> Code samples

```shell
# You can also use wget
curl -X POST /match/v2/find_matches \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json' \
  -H 'From: string' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`POST /find_matches`

*Search for all matching participant records using de-identified data*

Searches all state databases for any participant records that are an exact match to the `lds_hash` of persons provided in the request.

> Body parameter

> An example request to query a single person, with values for all fields

```json
{
  "data": [
    {
      "lds_hash": "eaa834c957213fbf958a5965c46fa50939299165803cd8043e7b1b0ec07882dbd5921bce7a5fb45510670b46c1bf8591bf2f3d28d329e9207b7b6d6abaca5458"
    }
  ]
}
```

<h3 id="find-matches-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|From|header|string|false|As in the HTTP/1.1 RFC, used for logging purposes as a means for identifying the source of invalid or unwanted requests. The interpretation of this field is that the request is being performed on behalf of the state government-affiliated person whose email address (or username) is specified here. It is not used for authentication or authorization.|
|data|body|object|true|none|
|» lds_hash|body|object|true|SHA-512 digest of participant's last name, DoB, and SSN. See docs/pprl.md for details|

> Example responses

> A query for a single person returning a single match

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "BCD2345",
            "state": "ea",
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
    ],
    "errors": []
  }
}
```

> A query for a single person returning no matches

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": []
      }
    ],
    "errors": []
  }
}
```

> A query for one person returning multiple matches

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "XYZ9876",
            "state": "eb",
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
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for two persons returning one match for each person

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      },
      {
        "index": 1,
        "matches": [
          {
            "match_id": "BCD2345",
            "state": "ea",
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
    ],
    "errors": []
  }
}
```

> A query for two persons returning no matches for one person and a match for the other

```json
{
  "data": {
    "results": [
      {
        "index": 0,
        "matches": []
      },
      {
        "index": 1,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": []
  }
}
```

> A query for two persons returning a successful result for one person and an error for the other person

```json
{
  "data": {
    "results": [
      {
        "index": 1,
        "matches": [
          {
            "match_id": "4567CDF",
            "state": "ec",
            "case_id": "string",
            "participant_id": "string",
            "benefits_end_month": null,
            "protect_location": null
          }
        ]
      }
    ],
    "errors": [
      {
        "index": 0,
        "code": "XYZ",
        "title": "Internal Server Exception",
        "detail": "Unexpected Server Error. Please try again."
      }
    ]
  }
}
```

> An example response for an invalid request

```json
{
  "errors": [
    {
      "status": "400",
      "code": "XYZ",
      "title": "Bad Request",
      "detail": "Request payload exceeds maxiumum count"
    }
  ]
}
```

> 400 Response

```json
{
  "errors": [
    {
      "status": "string",
      "code": "string",
      "title": "string",
      "detail": "string"
    }
  ]
}
```

<h3 id="find-matches-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Successful response. Returns match response items.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request. Request body does not match the required format.|Inline|

<h3 id="find-matches-responseschema">Response Schema</h3>

Status Code **200**

*Match response Success*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload.|
|»» results|array|true|none|Array of query results. For every person provided in the request, a result is returned, even if no matches are found. If a query fails, the failure data will be in the errors array.|
|»»» index|integer|true|none|The index of the person that the result corresponds to, starting from 0. Index is derived from the implicit order of persons provided in the request.|
|»»» matches|array|true|none|none|
|»»»» match_id|string|true|none|Unique identifier for the match|
|»»»» state|string|true|none|State/territory two-letter postal abbreviation|
|»»»» case_id|string|true|none|Participant's state-specific case identifier. Can be the same for multiple participants.|
|»»»» participant_id|string|true|none|Participant's state-specific identifier. Is unique to the participant. Must not be social security number or any PII.|
|»»»» benefits_end_month|string|false|none|Participant's ending benefits month|
|»»»» recent_benefit_months|array|false|none|List of up to the last 3 months that participant received benefits, in descending order. Each month is formatted as ISO 8601 year and month. Does not include current benefit month.|
|»»»» protect_location|boolean|false|none|Location protection flag for vulnerable individuals. True values indicate that the individual’s location must be protected from disclosure to avoid harm to the individual. Apply the same protections to true and null values.|
|»» errors|array|true|none|Array of error objects corresponding to a person in the request. If a query for a single person fails, the failure data will display here. Note that a single person in a request could have multiple error items.|
|»»» index|integer|true|none|The index of the person that the result corresponds to, starting from 0. Index is derived from the implicit order of persons provided in the request.|
|»»» code|string|false|none|The application-specific error code|
|»»» title|string|false|none|The short, human-readable summary of the error, consistent across all occurrences of the error|
|»»» detail|string|false|none|The human-readable explanation specific to this occurrence of the error|

Status Code **400**

*Http Errors*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» errors|array|false|none|Holds HTTP and other top-level errors. Either an errors or data property will be present in the top-level response, but not both.|
|»» status|string|true|none|The HTTP status code|
|»» code|string|false|none|The application-specific error code|
|»» title|string|false|none|The short, human-readable summary of the error, consistent across all occurrences of the error|
|»» detail|string|false|none|The human-readable explanation specific to this occurrence of the error|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

## Get match record

<a id="opIdGet match record"></a>

> Code samples

```shell
# You can also use wget
curl -X GET /match/v2/matches/{match_id} \
  -H 'Accept: application/json' \
  -H 'Ocp-Apim-Subscription-Key: API_KEY'

```

`GET /matches/{match_id}`

*Get a specific match record based on Match ID*

Retrieves a specific match record based on Match ID, if exists

> Example responses

> A match record is returned

```json
{
  "data": {
    "created_at": "2021-04-12T23:20:50.52Z",
    "match_id": "ABC1234",
    "initiator": "eb",
    "states": [
      "ea",
      "eb"
    ],
    "hash": "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec",
    "hash_type": "ldshash",
    "input": {
      "lds_hash": "a3cab51dd68da2ac3e5508c8b0ee514ada03b9f166f7035b4ac26d9c56aa7bf9d6271e44c0064337a01b558ff63fd282de14eead7e8d5a613898b700589bcdec"
    },
    "output": {
      "match_id": "BCD2345",
      "state": "ea",
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
    "invalid": false,
    "status": "open"
  }
}
```

> A match record is not found

```json
{
  "errors": [
    {
      "status": "404",
      "code": "XYZ",
      "title": "Not Found",
      "detail": "Match record not found"
    }
  ]
}
```

<h3 id="get-match-record-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Success|Inline|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Match not found|Inline|

<h3 id="get-match-record-responseschema">Response Schema</h3>

Status Code **200**

*Match Record response - success*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» data|object|false|none|The response payload representing a match record. Either an errors or data property will be present in the response, but not both.|
|»» created_at|string|true|none|Match record's creation date/time|
|»» match_id|string|true|none|Match record's human-readable unique identifier|
|»» initiator|string|true|none|Match record's initiating state or territory|
|»» states|array|true|none|List of states/territories involved in match|
|»» hash|string|true|none|Value of hash used to identify match. See docs/pprl.md for details|
|»» hash_type|string|true|none|Type of hash used to identify match|
|»» input|object|false|none|none|
|»»» lds_hash|string|true|none|SHA-512 digest of participant's last name, DoB, and SSN. See docs/pprl.md for details|
|»» output|object|true|none|none|
|»»» match_id|string|true|none|Unique identifier for the match|
|»»» state|string|true|none|State/territory two-letter postal abbreviation|
|»»» case_id|string|true|none|Participant's state-specific case identifier. Can be the same for multiple participants.|
|»»» participant_id|string|true|none|Participant's state-specific identifier. Is unique to the participant. Must not be social security number or any PII.|
|»»» benefits_end_month|string|false|none|Participant's ending benefits month|
|»»» recent_benefit_months|array|false|none|List of up to the last 3 months that participant received benefits, in descending order. Each month is formatted as ISO 8601 year and month. Does not include current benefit month.|
|»»» protect_location|boolean|false|none|Location protection flag for vulnerable individuals. True values indicate that the individual’s location must be protected from disclosure to avoid harm to the individual. Apply the same protections to true and null values.|
|»» invalid|boolean|true|none|Indicator used for designating match as invalid|
|»» status|any|true|none|Match record's status|

#### Enumerated Values

|Property|Value|
|---|---|
|hash_type|ldshash|
|status|open|
|status|closed|

Status Code **404**

*Http Errors*

|Name|Type|Required|Restrictions|Description|
|---|---|---|---|---|
|» errors|array|false|none|Holds HTTP and other top-level errors. Either an errors or data property will be present in the top-level response, but not both.|
|»» status|string|true|none|The HTTP status code|
|»» code|string|false|none|The application-specific error code|
|»» title|string|false|none|The short, human-readable summary of the error, consistent across all occurrences of the error|
|»» detail|string|false|none|The human-readable explanation specific to this occurrence of the error|

<aside class="warning">
To perform this operation, you must be authenticated by means of one of the following methods:
ApiKeyAuth
</aside>

