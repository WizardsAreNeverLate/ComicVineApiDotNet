"""Generates docs/openapi.json for the Comic Vine API.

The official docs list field names only, so this script holds those lists plus the corrections found by probing
the live API (type prefixes, series_list, plain-id video types, nullable envelope fields). Edit the tables here,
then run from the repo root:

    python tools/generate_openapi.py docs/openapi.json

and regenerate the Kiota client (see README). Output is deterministic.
"""
import json, sys

out = sys.argv[1]

F = {
 "character": "aliases api_detail_url birth character_enemies character_friends count_of_issue_appearances creators date_added date_last_updated deck description first_appeared_in_issue gender id image issue_credits issues_died_in movies name origin powers publisher real_name site_detail_url story_arc_credits team_enemies team_friends teams volume_credits",
 "chat": "api_detail_url channel_name deck image password site_detail_url title",
 "concept": "aliases api_detail_url count_of_issue_appearances date_added date_last_updated deck description first_appeared_in_issue id image issue_credits movies name site_detail_url start_year volume_credits",
 "episode": "aliases api_detail_url character_credits characters_died_in concept_credits air_date date_added date_last_updated deck description first_appearance_characters first_appearance_concepts first_appearance_locations first_appearance_objects first_appearance_storyarcs first_appearance_teams has_staff_review id image episode_number location_credits name object_credits person_credits site_detail_url story_arc_credits team_credits series",
 "issue": "aliases api_detail_url character_credits characters_died_in concept_credits cover_date date_added date_last_updated deck description disbanded_teams first_appearance_characters first_appearance_concepts first_appearance_locations first_appearance_objects first_appearance_storyarcs first_appearance_teams has_staff_review id image issue_number location_credits name object_credits person_credits site_detail_url store_date story_arc_credits team_credits teams_disbanded_in volume",
 "location": "aliases api_detail_url count_of_issue_appearances date_added date_last_updated deck description first_appeared_in_issue id image issue_credits movies name site_detail_url start_year story_arc_credits volume_credits",
 "movie": "api_detail_url box_office_revenue budget characters concepts date_added date_last_updated deck description distributor has_staff_review id image locations name producers rating release_date runtime site_detail_url studios teams things total_revenue writers",
 "object": "aliases api_detail_url count_of_issue_appearances date_added date_last_updated deck description first_appeared_in_issue id image issue_credits movies name site_detail_url start_year story_arc_credits volume_credits",
 "origin": "api_detail_url character_set id name profiles site_detail_url",
 "person": "aliases api_detail_url birth count_of_issue_appearances country created_characters date_added date_last_updated death deck description email gender hometown id image issue_credits name site_detail_url story_arc_credits volume_credits website",
 "power": "aliases api_detail_url characters date_added date_last_updated description id name site_detail_url",
 "promo": "api_detail_url date_added deck id image link name resource_type user",
 "publisher": "aliases api_detail_url characters date_added date_last_updated deck description id image location_address location_city location_state name site_detail_url story_arcs teams volumes",
 "series": "aliases api_detail_url character_credits count_of_episodes date_added date_last_updated deck description first_episode id image last_episode location_credits name publisher site_detail_url start_year",
 "story_arc": "aliases api_detail_url count_of_issue_appearances date_added date_last_updated deck description first_appeared_in_issue id image issues movies name publisher site_detail_url",
 "team": "aliases api_detail_url character_enemies character_friends characters count_of_issue_appearances count_of_team_members date_added date_last_updated deck description disbanded_in_issues first_appeared_in_issue id image issue_credits issues_disbanded_in movies name publisher site_detail_url story_arc_credits volume_credits",
 "video": "api_detail_url deck hd_url high_url id image length_seconds low_url name publish_date site_detail_url url user",
 "video_type": "api_detail_url deck id name site_detail_url",
 "video_category": "api_detail_url deck id name site_detail_url",
 "volume": "aliases api_detail_url character_credits concept_credits count_of_issues date_added date_last_updated deck description first_issue id image last_issue location_credits name object_credits person_credits publisher site_detail_url start_year team_credits",
 "types": "detail_resource_name id list_resource_name",
}
F["series_list"] = F["series"]

PLURAL = {"person": "people", "series": "series", "video_category": "video_categories"}
# Type id prefixes used in detail URLs: /{resource}/{prefix}-{id}/
PREFIX = {"issue": 4000, "character": 4005, "publisher": 4010, "concept": 4015, "location": 4020,
          "movie": 4025, "origin": 4030, "power": 4035, "person": 4040, "story_arc": 4045,
          "volume": 4050, "object": 4055, "team": 4060, "episode": 4070, "series": 4075,
          "video": 2300}
NOLIST = {"series_list", "types"}
STR_DATE = {"date_added", "date_last_updated", "cover_date", "store_date", "air_date", "release_date", "publish_date"}
INT = {"id", "count_of_issue_appearances", "count_of_episodes", "count_of_issues", "count_of_team_members", "length_seconds", "runtime_x"}
BOOL = {"has_staff_review"}
OBJ = {"image", "publisher", "volume", "series", "origin", "first_appeared_in_issue", "first_episode", "last_episode", "first_issue", "last_issue"}
ENUMS = {"gender": ["Male", "Female", "Other"]}

def ref(n): return {"$ref": f"#/components/schemas/{n}"}

def ftype(name):
    if name == "image": return ref("Image")
    if name in OBJ: return ref("ResourceRef")
    if name in STR_DATE: return {"type": "string", "description": "Datetime, `YYYY-MM-DD HH:MM:SS` (movie release_date/person birth may be date-only)."}
    if name in INT: return {"type": "integer"}
    if name in BOOL: return {"type": "boolean"}
    if name == "gender": return {"type": "integer", "description": "1 = Male, 2 = Female, 0 = Other (docs list Male/Female/Other)."}
    if name in ("start_year", "issue_number", "episode_number", "rating", "budget", "box_office_revenue", "total_revenue", "runtime"):
        return {"type": "string", "nullable": True}
    if name.endswith("_credits") or name in {"character_enemies","character_friends","characters","creators","issues_died_in","movies","powers","team_enemies","team_friends","teams","characters_died_in","disbanded_teams","disbanded_in_issues","issues_disbanded_in","teams_disbanded_in","issues","concepts","locations","producers","studios","things","writers","created_characters","profiles","story_arcs","volumes","character_set","first_appearance_characters","first_appearance_concepts","first_appearance_locations","first_appearance_objects","first_appearance_storyarcs","first_appearance_teams"}:
        return {"type": "array", "items": ref("ResourceRef")}
    return {"type": "string", "nullable": True}

def schema(r):
    props = {f: ftype(f) for f in F[r].split()}
    if r == "chat" or r == "types": pass
    return {"type": "object", "properties": props}

def cap(r): return "".join(p.capitalize() for p in r.split("_"))

schemas = {
 "Image": {"type": "object", "properties": {k: {"type": "string"} for k in
     ["icon_url","medium_url","screen_url","screen_large_url","small_url","super_url","thumb_url","tiny_url","original_url"]} | {"image_tags": {"type": "string"}}},
 "ResourceRef": {"type": "object", "description": "Abbreviated related resource.",
     "properties": {"id": {"type": "integer"}, "name": {"type": "string", "nullable": True}, "api_detail_url": {"type": "string"},
                    "site_detail_url": {"type": "string"}, "issue_number": {"type": "string", "nullable": True}, "count": {"type": "string"}, "role": {"type": "string"}}},
 "Envelope": {"type": "object", "required": ["status_code", "error"], "properties": {
     "status_code": {"type": "integer", "enum": [1,100,101,102,103,104,105],
        "description": "1 OK, 100 Invalid API Key, 101 Object Not Found, 102 Error in URL Format, 103 jsonp requires json_callback, 104 Filter Error, 105 Subscriber-only video"},
     "error": {"type": "string"}, "limit": {"type": "integer", "nullable": True}, "offset": {"type": "integer", "nullable": True},
     "number_of_page_results": {"type": "integer"}, "number_of_total_results": {"type": "integer"},
     "version": {"type": "string"}}},
}
for r in F: schemas[cap(r)] = schema(r)
del schemas["SeriesList"]
schemas["SearchResult"] = {"allOf": [ref("Resource"), {}]} if False else {"type": "object", "description": "Union of fields of the searched resource types, plus `resource_type`.",
    "additionalProperties": True, "properties": {"resource_type": {"type": "string"}, "id": {"type": "integer"}, "name": {"type": "string"}}}

def env(item, many):
    return {"allOf": [ref("Envelope"), {"type": "object", "properties": {"results": ({"type": "array", "items": item} if many else item)}}]}

params = {
 "ApiKey": {"name": "api_key", "in": "query", "schema": {"type": "string"}, "description": "Your Comic Vine API key. Set by the client auth provider; do not pass it yourself."},
 "Format": {"name": "format", "in": "query", "schema": {"type": "string"}, "description": "xml, json or jsonp. The client auth provider always sets json."},
 "JsonCallback": {"name": "json_callback", "in": "query", "schema": {"type": "string"}},
 "FieldList": {"name": "field_list", "in": "query", "style": "form", "explode": False, "schema": {"type": "array", "items": {"type": "string"}}, "description": "Comma-delimited fields to return."},
 "Limit": {"name": "limit", "in": "query", "schema": {"type": "integer", "default": 100, "maximum": 100, "minimum": 1}},
 "Offset": {"name": "offset", "in": "query", "schema": {"type": "integer", "default": 0, "minimum": 0}},
 "Sort": {"name": "sort", "in": "query", "schema": {"type": "string", "pattern": "^[a-z_]+:(asc|desc)$"}, "description": "`field:asc|desc`."},
 "Filter": {"name": "filter", "in": "query", "schema": {"type": "string"}, "description": "`field:value`, comma-separated for several. Date ranges: `field:start|end`."},
}
P = lambda n: {"$ref": f"#/components/parameters/{n}"}
common = [P("ApiKey"), P("Format"), P("FieldList")]
listp = common + [P("Limit"), P("Offset"), P("Sort"), P("Filter")]
errs = {"200": None, "401": {"description": "Invalid API key (status_code 100)"}, "404": {"description": "Object not found (status_code 101)"}}

def resp(item, many, desc):
    return {"200": {"description": desc, "content": {"application/json": {"schema": env(item, many)}, "application/xml": {"schema": env(item, many)}}},
            "default": {"description": "Error; see `status_code` in the envelope.", "content": {"application/json": {"schema": ref("Envelope")}}}}

paths = {}
for r in F:
    if r in ("types", "series_list"): continue
    plural = PLURAL.get(r, r + "s")
    C = cap(r)
    if r != "series":
        paths[f"/{plural}/"] = {"get": {"operationId": f"list{C}" + ("List" if plural == r else ""), "tags": [C], "summary": f"List {plural.replace('_',' ')}",
            "parameters": listp, "responses": resp(ref(C), True, f"List of {plural}")}}
    if r in PREFIX:
        paths[f"/{r}/{{guid}}/"] = {"get": {"operationId": f"get{C}", "tags": [C], "summary": f"Get a {r.replace('_',' ')}",
            "parameters": [{"name": "guid", "in": "path", "required": True, "schema": {"type": "string", "pattern": r"^\d+-\d+$", "example": f"{PREFIX[r]}-1"},
                            "description": f"Type-prefixed id: `{PREFIX[r]}-<id>`. Prefix is not in the docs and was inferred; the `guid`/`api_detail_url` returned by list calls is authoritative."}] + common,
            "responses": resp(ref(C), False, f"A {r}")}}
    else:
        paths[f"/{r}/{{id}}/"] = {"get": {"operationId": f"get{C}", "tags": [C], "summary": f"Get a {r.replace('_',' ')}",
            "parameters": [{"name": "id", "in": "path", "required": True, "schema": {"type": "integer"}}] + common, "responses": resp(ref(C), False, f"A {r}")}}

paths["/series_list/"] = {"get": {"operationId": "listSeriesList", "tags": ["Series"], "summary": "List series (series_list resource)",
    "parameters": listp, "responses": resp(ref("Series"), True, "List of series")}}
paths["/types/"] = {"get": {"operationId": "listTypes", "tags": ["Types"], "summary": "List resource types", "parameters": common,
    "responses": resp(ref("Types"), True, "Resource types")}}
paths["/search/"] = {"get": {"operationId": "search", "tags": ["Search"], "summary": "Search across resources",
    "parameters": common + [
        {"name": "query", "in": "query", "required": True, "schema": {"type": "string"}},
        {"name": "resources", "in": "query", "style": "form", "explode": False, "schema": {"type": "array", "items": {"type": "string",
            "enum": ["character","concept","origin","object","location","issue","story_arc","volume","publisher","person","team","video"]}}},
        {"name": "limit", "in": "query", "schema": {"type": "integer", "default": 10, "maximum": 100}}, P("Offset")],
    "responses": resp(ref("SearchResult"), True, "Search results")}}
paths["/series/{id}/"] = paths.pop("/series/{guid}/")
paths["/series/{id}/"]["get"]["parameters"][0]["name"] = "id"

spec = {"openapi": "3.0.3",
 "info": {"title": "Comic Vine API", "version": "1.0.0",
          "description": "The generated client sets `api_key` and `format=json` through its auth provider. Unofficial OpenAPI description derived from https://comicvine.gamespot.com/api/documentation. The docs give field names but not types, so types are inferred; most scalar fields are nullable strings. Live behavior: an unknown detail id returns HTTP 200 with status_code 101 and an empty results array, while an invalid key returns HTTP 401. Detail-URL type prefixes (e.g. 4005 for character) are not in the docs but were verified against the live API (video_type and video_category use plain ids). Comic Vine requires a descriptive User-Agent header."},
 "servers": [{"url": "https://comicvine.gamespot.com/api"}],
 "tags": [{"name": cap(r)} for r in F if r != "series_list"],
 "paths": paths,
 "components": {"schemas": schemas, "parameters": params,
                "securitySchemes": {"ApiKeyQuery": {"type": "apiKey", "in": "query", "name": "api_key"}}},
 "security": [{"ApiKeyQuery": []}]}
json.dump(spec, open(out, "w"), indent=2)
