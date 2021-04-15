<!-- Generator: Widdershins v4.0.1 -->

<h1 id="duplicate-participation-api">Duplicate Participation API v1.0.0</h1>

> Scroll down for code samples, example requests and responses. Select a language for code samples from the tabs above or the mobile navigation menu.

The API for the Duplicate Participation system where bulk upload, matching, and lookups will occur

Base URLs:

* <a href="/v1">/v1</a>

<h1 id="duplicate-participation-api-match">Match</h1>

## post__query

> Code samples

```shell
# You can also use wget
curl -X POST /v1/query \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json'

```

`POST /query`

*Search for all matching PII records*

Queries all state databases for any PII records that are an exact match to the full name, date of birth, and social security number in the request body's `query` property.

> Body parameter

```json
{
  "query": {
    "first": "string",
    "middle": "string",
    "last": "string",
    "ssn": "string",
    "dob": "2019-08-24"
  }
}
```

<h3 id="post__query-parameters">Parameters</h3>

|Name|In|Type|Required|Description|
|---|---|---|---|---|
|body|body|object|true|none|

> Example responses

> 200 Response

```json
{
  "lookup_id": "string",
  "matches": [
    {
      "first": "string",
      "middle": "string",
      "last": "string",
      "ssn": "string",
      "dob": "2019-08-24",
      "state_name": "string",
      "state_abbr": "string",
      "exception": "string"
    }
  ]
}
```

<h3 id="post__query-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Matching PII records, if any exist|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request. Missing one of the required properties in the request body.|None|

<h3 id="post__query-responseschema">Response Schema</h3>

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
|»» state_name|string|false|none|Full state/territory name|
|»» state_abbr|string|false|none|State/territory two-letter postal abbreviation|
|»» exception|string|false|none|Placeholder for value indicating special processing instructions|

<aside class="success">
This operation does not require authentication
</aside>

<h1 id="duplicate-participation-api-lookup">Lookup</h1>

## get__lookup_ids_{id}

> Code samples

```shell
# You can also use wget
curl -X GET /v1/lookup_ids/{id} \
  -H 'Accept: application/json'

```

`GET /lookup_ids/{id}`

*get the original match data related to a Lookup ID*

User can provide a Lookup ID and receive the match data associated with it

> Example responses

> 200 Response

```json
{
  "data": {
    "first": "string",
    "middle": "string",
    "last": "string",
    "ssn": "string",
    "dob": "2019-08-24"
  }
}
```

<h3 id="get__lookup_ids_{id}-responses">Responses</h3>

|Status|Meaning|Description|Schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|original active match data|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Bad request|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Not Found|None|

<h3 id="get__lookup_ids_{id}-responseschema">Response Schema</h3>

<aside class="success">
This operation does not require authentication
</aside>

