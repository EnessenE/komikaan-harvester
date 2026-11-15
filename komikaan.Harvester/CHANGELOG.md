# Changelog

## [2.8.2] - 15-11-2026
### Fixed
- Feed downloads now handle server-side compression: the HttpClient uses `AutomaticDecompression = All`, so a forced `Content-Encoding: gzip/br` response no longer ends up saved as a corrupt zip; downloads stream to disk (no full in-memory buffering, no 100s default timeout) and the payload is validated against the PK zip magic with a clear error
- `stop_times.txt` is now treated as required (per the GTFS spec); a feed without it no longer completes the import with no stop_times partition, which would have broken the partition swap in `move_to_new_import`
- Garden notifications are sent after `move_to_new_import` swaps the new stop_times partition live, so `merge_stop` no longer derives `stop_type` from the old partition for brand-new stops

## [2.8.1] - 15-11-2026
### Fixed
- RabbitMQ connections no longer die silently: connect/continuation timeouts were 3 hours (a half-dead connection hung all ack/nack/consume calls without error); now 30s with a 60s heartbeat and automatic + topology recovery
- `DetectorContext` and `GardenerContext` retry connecting until the broker and its exchange/queue are up, instead of failing the hosted service on the first attempt; connection shutdown/recovery is now logged
- `GardenerContext` keeps the connection in a field and publishes garden notifications async with an open-channel guard, instead of sync-over-async fire-and-forget that silently dropped notifications when the channel was down

## [2.8.0] - 14-11-2026
### Fixed
- `RouteMap` mapped `RouteType`, which pushed every value through the ExtendedRouteType conversion (e.g. bus 3 became 700, extended types like 1702 were lost). Now maps the raw `route_type` int so `routes.type` keeps the feed value
- `PSQLStop.StopTypeData` was hardcoded to 7 (funicular) for every stop via a shadowed property; now derives from `location_type` (default 0) as the gardener later overwrites it with the served route type
- `PSQLStopTime.DepartureTimeData` getter checked `ArrivalTime` instead of `DepartureTime`, so >24h departure times were not split into `days_since_start_departure` and would overflow the PG `time` column
- Empty GTFS times (`arrival_time`/`departure_time`) were imported as 00:00:00 instead of NULL; `GtfsTimeConverter` now returns null for empty values
- Failed feed downloads (non-success status, empty body, missing local zip) no longer continue into extraction and crash with "Sequence contains no elements"; they throw a clear error so the import is marked failed
- stop_times partition cleanup never matched anything because the LIKE pattern escaped the trailing `%` wildcard into a literal percent; old partitions are now actually dropped after an import
- DetectorContext now drops (acks) unparseable/empty queue messages instead of redelivering them forever, and closes the RabbitMQ channel/connection on shutdown
- Removed `Directory.CreateDirectory("/app")` and the bogus `File.Delete("\\\\app\\\\gtfs_file.zip")` from the harvest loop; the working dirs are cleared at the start of each import
- stop_times partition names are now built as `<prefix>_<import_id>` with the prefix capped at 25 chars, so the name always fits the 63-byte PG identifier limit. Previously the whole name was truncated to 62 chars; for supplier names whose sanitized form is >=62 chars the import id was cut off entirely, every import produced the same partition name, `CREATE TABLE IF NOT EXISTS` kept the old range and the second import failed with "no partition of relation stop_times found"
- When sanitization changes the supplier name (e.g. "gob.es"), a short SHA-256 hash of the raw name is now part of the partition prefix. Suppliers that sanitize to the same string (e.g. "gob.es" and "gob_es") previously dropped each other's live stop_times partitions during cleanup, losing the other supplier's data until its next import
- Duplicate rows inside one upsert chunk are now filtered by conflict key for all MERGE / `ON CONFLICT DO UPDATE` targets (agencies, routes, calendars, frequencies), not just stops. A single duplicated route_id/service_id/trip_id row previously failed the whole chunk with "ON CONFLICT DO UPDATE command cannot affect row a second time"
- A failing Discord webhook can no longer fail an import or skip DB bookkeeping: notifications in `HarvestingManager` are guarded by try/catch and failure marking (`harvester_mark_supplier_failed`) runs before notifying; `DetectorContext` resets its `importRunning` flag in a `finally` and requeues the message on unexpected errors instead of wedging the consumer forever
- GTFS zips that wrap all txt files in a single top-level folder are now handled by descending into that folder after extraction; previously the import aborted because `routes.txt`/`stops.txt` were not found at the extraction root

### Added
- Import `frequencies.txt` end-to-end (new `PSQLFrequency` adapter + `FrequencyMap` + `upsert_frequencies` call); >24h start/end times are wrapped to time-of-day since the type has no days column

### Changed
- `UpsertEntityAsync` reuses one DB connection per upsert instead of opening one per chunk; partition name is sanitized to `[A-Za-z0-9_]` and supplier names are escaped in SQL literals
- Partition cleanup now matches on the same capped prefix used to build the partition name. Note: partitions created before this change for supplier names containing punctuation (now hash-suffixed) are not matched by the new pattern and will linger as empty tables once; their rows are still removed by `move_to_new_import`
- `StopTimeMap` is now explicitly `ClassMap<PSQLStopTime>`; removed duplicate `shape_id` mapping in `ShapeMap` and the `bikes_allowed` mapping (trips_type has no such column)

### Removed
- Dead code: duplicate `Enums/` folder, unused `ISupplier`, unused `GetTypeMappingsAsync`/`SupplierTypeMapping`, empty `Helpers/` folder include, dead `Mask`/`BikesAllowedData` properties, stale comments

## [2.7.0] - 14-11-2026
### Changed
- Bump all NuGet dependencies to their latest versions (RabbitMQ.Client 7.x required migrating to the async channel API; CsvHelper pinned to 33.1.0 because 34.x is a breaking rewrite)
- RabbitMQ topology is now declared via the RabbitMQ operator; the app only passive-declares the exchange and queue
- Remove unused NetTopologySuite.Core dependency

## [2.6.3] - 02-08-2026
### Changed
- Properly use the sanatized data origin name to delete paritions

## [2.6.2] - 25-07-2026
### Added
- Ignore BadData and BlankLines for supplier with different opinions about CSVs

## [2.6.1] - 25-07-2026
### Changed
- Attempt to properly pass import_id to the DB by passing queued_import_id consistently
- Remove legacy clean_old_data call

## [2.6.0] - 19-07-2026
### Added
- Support for harvester_rebuild_gtfs_service_dates to build normalized service dates
### Changed
- Attempt to fix wrong import id being passed to DB causing cleanup to fail
- Attempt to fix an issue around partitions starting with numbers in their names, causing "42601: trailing junk after numeric literal at or near 'bla''"

## [2.5.2] - 25-05-2026
### Changed
- Passive declare queues / exchanges

## [2.5.1] - 22-03-2026
### Fixed
- Filter duplicate stop rows inside each upsert batch by `data_origin + id` before calling `public.upsert_stops`, preventing `uq_stops_data_origin_id` violations caused by duplicate records in a single chunk.

## [2.5.0] - 22-03-2026
### Changed
- Refactored `Komikaan:ContactPoint` configuration access to use a strongly typed `KomikaanSettings` model via `IOptions`.
- Moved Discord webhook client base URL from hardcoded `Program` value to `Komikaan:DiscordWebhookUrl` in `KomikaanSettings` via `IOptions`.
- Removed direct environment-variable reading for contact point in `GenericGTFSSupplier`.
- Added cancellation token propagation from `DetectorContext` through `HarvestingManager` into `GenericGTFSSupplier` and all GTFS upsert operations.
- Refactored GTFS upsert APIs to `IAsyncEnumerable<T>` and updated CSV ingestion to use `GetRecordsAsync` for streaming imports to reduce memory usage.
- Updated `UpsertEntityAsync` async-stream batching to use .NET 10 `IAsyncEnumerable.Chunk` with cancellation-aware iteration.
