# Changelog

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
