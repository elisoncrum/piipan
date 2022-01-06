
<h1 id="match-collaboration-api">Match Collaboration API v1.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

Internal API for resolving matching participant records

Base URLs:

* <a href="/v1">/v1</a>

# Authentication

- HTTP Authentication, scheme: bearer

<h1 id="match-collaboration-api-collaboration">Collaboration</h1>

## Get match record

<a id="opIdGet match record"></a>

> Code samples

```shell
# You can also use wget
curl -X GET /v1/matches/{match_id} \
  -H 'Accept: application/json' \
  -H 'Authorization: Bearer {access-token}'

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
|»» input|object|false|none|Original request data from real-time match request|
|»»» lds_hash|string|true|none|SHA-512 digest of participant's last name, DoB, and SSN. See docs/pprl.md for details|
|»» output|object|true|none|Original response data from real-time match request|
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
BearerAuth
</aside>

