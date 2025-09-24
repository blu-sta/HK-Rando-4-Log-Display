# HK Rando 4 Log Display v2.1.0.4

## Requirements

This application is standalone, and does not need to be in any Hollow Knight folder \
Please note, it does need the Reference folder (included in the zip file) to identify items and locations correctly. \
I leave all the unzipped files in a folder in Downloads, and then have a shortcut on the Desktop \
NOTE: This was written for Windows and may require .NET 8 to be installed separately in order to work. \
If this stopped working for you since v2.1.0.0, you may still need a .NET 5 version. \
If this is the case, please reach out to let me know, and I'll restore support for that version.

I have also been working on something that I hope will allow for wider use of the Log Display on more operating systems.

## What's New?
- This section, to summarise the changes for those who don't want to read the change logs
- Recently added rando support:
    - CombatRando (Notch fragments)
    - GrassRandoV2 (New grass location names)
    - LoreRando (Shrine of Believers)
    - Scattered and Lost (Bretta Hearts)
    - VendorRando 
    - YetAnotherRandoConnection
- New buttons
    - The Helper item tab can now separate and hide Out of Logic checks
    - The Helper transition tab can now separate and hide Out of Logic transitions
    - The Spoiler item tab can now hide obtained items, to make it easier to find where Right_Mothwing_Cloak was located
    - The Spoiler transition tab can now hide traversed transitions, to make it easier to find where Room_shop[left1] was located
- Obtained item counters on Spoiler item tab
- Traversed transition counters on Spoiler transition tab
- Displays `*` for items obtained and transitions traversed while out of logic on Tracker and Spoiler tabs

## Compatibility

In addition to the default Randomiser, the following mods are also supported:
- [AccessRando](https://github.com/nerthul11/AccessRandomizer)
- [BenchRando](https://github.com/homothetyhk/BenchRando)
- [BreakableWallRandomizer](https://github.com/Bentechy66/HollowKnight.BreakableWallRandomizer)
  - [WFCP Fork](https://github.com/nerthul11/BreakableWallRandomizer)
- [CombatRando](https://github.com/nerthul11/CombatRandomizer)
- [DarknessRandomizer](https://github.com/dplochcoder/HollowKnight.DarknessRandomizer)
- [ExtraRando](https://github.com/Korzer420/ExtraRando)
- [Fishing](https://github.com/dpinela/Fishing)
- [FlowerQuest](https://github.com/nerthul11/FlowerRandomizer)
- [GodhomeRando](https://github.com/nerthul11/GodhomeRandomizer)
- [GrassRando](https://github.com/StormZillaa/HollowKnightGrassRando)
- [GrassRandoV2](https://github.com/ManicJamie/HollowKnight.GrassRando)
- [LoreMaster](https://github.com/Korzer420/LoreMaster)
- [LostArtifacts](https://github.com/Hoo-Knows/HollowKnight.LostArtifacts)
- [MilliGolf](https://github.com/TheMathGeek314/MilliGolf)
- [MoreDoors](https://github.com/dplochcoder/HollowKnight.MoreDoors)
- [MoreLocations](https://github.com/BadMagic100/MoreLocations)
- [MultiWorld](https://github.com/Shadudev/HollowKnight.MultiWorld)
- [RainbowEggs](https://github.com/dpinela/RainbowEggs)
- [RandomizableLevers](https://github.com/flibber-hk/HollowKnight.RandomizableLevers)
- [RandoPlus](https://github.com/flibber-hk/HollowKnight.RandoPlus)
- [ReopenCityDoor](https://github.com/flibber-hk/HollowKnight.ReopenCityDoor)
- [ScatteredAndLost](https://github.com/dplochcoder/HollowKnight.ScatteredAndLost)
- [SkillUpgrades](https://github.com/flibber-hk/HollowKnight.SkillUpgrades)
- [TheGloryOfBeingAFoolRandomizer](https://github.com/dpinela/TheGloryOfBeingAFoolRandomizer)
- [TheRealJournalRando](https://github.com/BadMagic100/TheRealJournalRando)
- [Transcendence](https://github.com/dpinela/Transcendence)
- [VendorRando](https://github.com/TheMathGeek314/VendorRando)
- [YetAnotherRandoConnection](https://github.com/TheMathGeek314/YetAnotherRandoConnection)

Other mods may work without requiring an update, although might have limited functionality or missing information

### MultiWorld
[MultiWorld](https://github.com/Shadudev/HollowKnight.MultiWorld/) support has been vastly improved as of v2.0.5 \
Please use the `MultiWorld` button to enter player names \
The `Predict` button in the MultiWorld window will attempt to identify players from the Item Spoiler Log

## How to use this app

### Top bar
Shows the mode and seed \
Also displays randomiser files that cannot be found or fail to load \
Also displays counts for the following:
- Items found and previewed
- Locations found and previewed
- Locations available in (and out) of logic

### Helper Tab - Locations

#### Buttons
`Group` toggles how locations are grouped together \
Options include Map Areas, Titled Map Areas, Rooms in Map/Titled Areas, Rooms or None \
`Sort` toggles whether locations are ordered alphabetically or order of appearance in the logs  \
`Out of Logic` cycles between showing, splitting (separating) and hiding Out of Logic checks \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the location appeared in the log \
`Room` will toggle between showing the in-game room code and an alternate description of the room \
**Note**: Alternate descriptions can be personalised by editing the `_sceneDescriptions.json` file

#### Sections
`Countables`: Tracks count of items that could be required for other checks or True Ending \
e.g. Grubs, Charms, Essence, etc. \
`Previewed Locations`: All previewed locations grouped by location pool, showing the item at the location and any costs associated \
`Previewed Items`: All previewed items grouped by item pool, showing the location and any costs associated \
This is effectively the same as `Previewed Locations`, but is an alternative way to check for where to find desired items like charms or spells, as opposed to what items are available at a location such as a shop or whispering root. \
`Location Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of locations in the group \
Locations with `*` prefix are considered out of logic

### Helper Tab - Transitions

#### Buttons

Refer to **Helper Tab - Locations**, as all buttons here have the same functionality

#### Sections

`Transition Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of transitions in the group \
Transitions with \* prefix are considered out of logic.

### Tracker Tab - Items

#### Buttons
`Group` toggles how items are grouped together \
Options include Curated Item Pools, All Item Pools, All Location Pools or None \
Curated Item Pools is loosely defined as collections of specific items that were often considered useful to recall before the days of ItemSync \
`Sort` toggles whether items are ordered alphabetically or order of pick up; disabled when `Group` is curated  \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the item was obtained

#### Sections

`Item Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of items obtained in the pool \
Does not record if items were obtained out of logic \
All Location Pools uses the location pool instead of the item pool \
e.g. a White_Fragment bought at Sly would show under Charms for All Item Pools, Shop for All Location Pools, and True Ending Items for Curated Item Pools

### Tracker Tab - Transitions

#### Buttons
`Group` toggles how transitions are grouped together \
Options are the same as in **Helper Tab - Locations** \
`Sort` toggles whether transitions are ordered alphabetically or order of traversal \
`Focus` toggles whether to use the Source or Destination for grouping \
(This is only really useful when considering Uncoupled transitions) \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the transition was first traversed \
`Room` will toggle between showing the in-game room code and an alternate description of the room

#### Sections

`Transition Groups`: Same as `Transition Groups` in **Helper Tab - Transitions**. \
Does not record if transitions were traversed outside of logic.

### Spoiler Tab - Items

#### Buttons

`Group`, `Sort`, `Expand All` and `Collapse All` function the same as in **Tracker Tab - Items** \
`Obtained` can mark (with ~~strikethrough~~) obtained items, hide obtained items, or ignore the obtained state

#### Sections

`Item Groups`: Same as `Item Groups` in **Tracker Tab - Items**

### Spoiler Tab - Transitions

#### Buttons

`Group`, `Sort`, `Focus`, `Expand All`, `Collapse All` and `Room` function the same as in **Tracker Tab - Transitions** \
`Traversed` can mark (with ~~strikethrough~~) traversed transitions, hide traversed transitions, or ignore the traversed state

#### Sections

`Transition Groups`: Same as `Transition Groups` in **Tracker Tab - Transitions**

### Settings Tab - Common Button bar

Found at the bottom of the settings tabs \
These buttons allow for easily managing information related to the current randomiser playthrough \
`Copy Seed`: Copies current seed to clipboard \
`Copy Shareable Settings`: Copies settings used to set menu selections \
`Zip log files`: Creates and opens a timestamped zip file containing the log files for the current seed, including current state of the application and recent error logs, to provide for troubleshooting and bug reports

### Settings Tab - Seed Settings

Expanders for showing the current settings of the seed

### Settings Tab - App Settings

Buttons to compare and set the state of the Helper, Tracker and Spoiler tabs
Current states are highlighted

### Bottom Bar

`Open Log File` will open the log file for the currently selected tab \
`MultiWorld` will open a window to enter player names for MultiWorld rando \
`Update Available` will appear if a newer version is available from Github \
Also shows the current version number in the right corner

## Future Plans

- Configuration of Curated list (pool selection, charm selection)
- Group by "Location Pools" on Helper Item Tab (i.e. Rando Vanilla tracking support)
- Group by "All Location Map Areas" option for Tracker & Spoiler Item tabs
- Notify user when expected reference data is missing

## Reporting Issues and Feature Requests

If something goes wrong while using this application, \
or you would like something added or improved, \
please contact `@blu.sta` on Discord \
It is a lot easier to add support for new Randos and resolve problems \
if a zip of the log files is provided with your support request \
See also: `Settings tab` > `Zip log files` above

## Appreciation

A big thank you to those who continue to report problems and make suggestions to help improve this application \
And a special thanks to **ColetteMSLP** and **wataniyob**, both of whom constantly test this against the latest rando mods and helps with improving the compatibility

## Reference material

Files found in the **Reference** folder can be updated manually if you do not wish to wait for a patch update \
Please note that if you are partway through a rando run, the items already logged will not update immediately \
and if the schema changes the import may break functionality \
Additionally, `_sceneDescriptions.json` is a file with a list of room codes with descriptions that can be updated as you see fit
