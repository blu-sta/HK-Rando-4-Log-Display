# HK Rando 4 Log Display Changelog

## v2.1.0.0
Summary of changes since last release version:
- Added support for more randomisers
- Updated support for existing randomisers
- Additional counters on Helper tab
- Included HK Chains as necessary for True Ending
- Updated display of room descriptions

## v2.0.6.9
- Fixed HK Chains not appearing in TE counters
- Removed starting items from items found counter
- Resolved issue with deployed version of application

## v2.0.6.8
- Fixed items found and previewed count

## v2.0.6.7
- Updated room description displays on all tabs
  - Expanders will now show the room code as well as the description
  - Transistions will now show descriptions with doors
- Separated available locations into in-logic and out-of-logic location counts
- Use scene name as backup description before location fallback value
- Added recent error logs to zip package
- #Debug: Added display for listing scenes without descriptions

## v2.0.6.6
- Added AccessRando
- Added Miner's Key to MoreDoors from ScatteredAndLost
- Added Hollow Knight Chains to TE countable list
- Updated item and location counters:
  - Added items previewed to items found
  - Removed location duplicates (e.g. now only counts shops once)
  - Ignore Start location

## v2.0.6.5
- Fixed issue with helper log that prevented loading without any WFCPs
- Added item and location counters in header

## v2.0.6.4
- Added BreakableWFCP (replaces Breakable Walls), GodhomeRando (Statue Marks only), MilliGolf, Fishing, Flower Quest
- Updated ExtraRando (Hot Springs, Colo Tickets), GrassRando (Grass Shop only)
- Added counters for WFCPs (Myla shop) and Statue Marks (Godhome shop)
- Various QOL fixes

## v2.0.6.3

- Added ExtraRando, GrassRando, MoreLocations and TGOBAFRando
- Updated RandoPlus

## v2.0.6.2

- Fix for Hunter's Notes not showing up under correct preview pool
- Updated Breakable Walls

## v2.0.6.1

- Possible fix for Journal Entries not showing up under correct preview pool

## v2.0.6

- Added mod support: Breakable Walls
- Allowed MW player names to start with numbers

## v2.0.5

- Improved countable items on Item Helper tab
- Improved mod support: MultiWorld, RandoPlus
- Added "Predict" button to MultiWorld window to detect player names

## v2.0.4

- Added button to add MultiWorld player names
- Added message in header when log file is not loaded correctly

## v2.0.3

- Fixed MultiWorld rando crash in Tracker log
- Added MultiWorld disclaimer to [Readme](https://github.com/blu-sta/HK-Rando-4-Log-Display/blob/main/README.md#multiWorld-disclaimer)
- Updated scene descriptions
- Added `_sceneDescriptions.json` file to allow for custom descriptions
- Updated LoreMaster
- Added update version to `Update Available` button

## v2.0.2

- Added check for updates on startup, with button to link to Github if update is available
- Added button for showing Room codes/descriptions for locations on Tracker Item and Spoiler Item tabs

## v2.0.1

- Lumafly_Escape preview "Nothing?" is now recognised
- Charm counter now includes Transcendence charms
- Additional counters added for True Ending items

## v2.0.0

- Complete rework of tab layout and internal logic
- Added new tab dedicated to Application Settings
- Added new and updated existing buttons
- Improved mod support
- Added location pools based on the item(s) found at the location
- Modified base item import file to simplify some pools
- Rewrote readme to reflect new functionality
