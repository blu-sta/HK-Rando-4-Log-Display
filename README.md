# HK Rando 4 Log Display v2.0.0

## Requirements

This application is standalone, and does not need to be in any Hollow Knight folder \
I leave all the files in a folder in Downloads, and then have a shortcut on the Desktop \
It was written for Windows and may require .NET 5 to be installed in order to work

## Compatibility

In addition to the default Randomiser, the following mods are also supported:
- BenchRando
- DarknessRandomizer
- LostArtifacts
- MoreDoors
- RainbowEggs
- RandomizableLevers
- RandoPlus
- ReopenCityDoor
- SkillUpgrades
- TheRealJournalRando
- Transcendence

Other mods may work without requiring an update, although might have limited functionality or missing information

## How to use this app

### Top bar
Shows the mode and seed \
Also displays randomiser files that cannot be found

### Helper Tab - Locations

###### Buttons
`Group` toggles how locations are grouped together \
Options include Map Areas, Titled Map Areas, Rooms in Map/Titled Areas, Rooms or None \
`Sort` toggles whether locations are ordered alphabetically or order of appearance in the logs  \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the location appeared in the log \
`Open Log` will open the helper log file in notepad

###### Sections
`Countables`: Tracks count of items that could be required for other checks \
e.g. Grubs, Charms, Essence, etc. \
`Previewed Locations`: All previewed locations grouped by location pool, showing the item at the location and any costs associated \
`Previewed Items`: All previewed items grouped by item pool, showing the location and any costs associated \
This is effectively the same as `Previewed Locations`, but is an alternative way to check for where to find desired items like charms or spells, as opposed to what items are available at a location such as a shop or whispering root. \
`Location Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of locations in the group \
Locations with \* prefix are considered out of logic

### Helper Tab - Transitions

###### Buttons

Refer to **Helper Tab - Locations**, as all buttons here have the same functionality

###### Sections

`Transition Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of transitions in the group \
Transitions with \* prefix are considered out of logic.

### Tracker Tab - Items

###### Buttons
`Group` toggles how items are grouped together \
Options include Curated Item Pools, All Item Pools, All Location Pools or None \
Curated Item Pools is loosely defined as collections of specific items that were often considered useful to recall before the days of ItemSync \
`Sort` toggles whether items are ordered alphabetically or order of pick up; disabled when `Group` is curated  \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the item was obtained \
`Open Log` will open the tracker log file in notepad

###### Sections

`Item Groups`: Expandable sections defined by the `Group` setting \
Also displays the number of items obtained in the pool \
Does not record if items were obtained out of logic \
All Location Pools uses the location pool instead of the item pool \
e.g. a White_Fragment bought at Sly would show under Charms for All Item Pools, Shop for All Location Pools, and True Ending Items for Curated Item Pools

### Tracker Tab - Transitions

###### Buttons
`Group` toggles how transitions are grouped together \
Options are the same as in **Helper Tab - Locations** \
`Sort` toggles whether transitions are ordered alphabetically or order of traversal \
`Focus` toggles whether to use the Source or Destination for grouping \
(This is only really useful when considering Uncoupled transitions) \
`Expand All` and `Collapse All` can be used to expand/collapse all groups \
`Time` will show or hide the time the transition was first traversed \
`Open Log` will open the tracker log file in notepad

###### Sections

`Transition Groups`: Same as `Transition Groups` in **Helper Tab - Transitions**. \
Does not record if transitions were traversed outside of logic.

### Spoiler Tab - Items

###### Buttons

`Group`, `Sort`, `Expand All` and `Collapse All` function the same as in **Tracker Tab - Items** \
`Obtained` replaces `Time` to show or hide a strikethrough for each item obtained \
`Open Log` will open the item spoiler log file in notepad

###### Sections

`Item Groups`: Same as `Item Groups` in **Tracker Tab - Items**

### Spoiler Tab - Transitions

###### Buttons

`Group`, `Sort`, `Focus`, `Expand All` and `Collapse All` function the same as in **Tracker Tab - Transitions** \
`Traversed` replaces `Time` to show or hide a strikethrough for each transition traversed \
`Open Log` will open the transition spoiler log file in notepad

###### Sections

`Transition Groups`: Same as `Transition Groups` in **Tracker Tab - Transitions**

### Settings Tab - Common Button bar

Found at the bottom of the settings tabs \
These buttons allow for easily managing information related to the current randomiser playthrough \
`Copy Seed`: Copies current seed to clipboard \
`Copy Shareable Settings`: Copies settings used to set menu selections \
`Zip log files`: Creates and opens a timestamped zip file containing the log files for the current seed, including current state of the application, to provide for troubleshooting and bug reports

### Settings Tab - Seed Settings

Expanders for showing the current settings of the seed

### Settings Tab - App Settings

Buttons to compare and set the state of Grouping and Sorting for each tab
Current state is highlighted

### Bottom Bar

Shows the current version number

### Appreciation

A big thank you to those who continue to report problems and make suggestions to help improve this application \
And a special thanks to **ColetteMSLP**, who constantly tests it against the latest rando mods and helps with improving the compatibility

### Reporting issues

If something goes wrong while using this application, please contact `@blu.sta#9997` on Discord

### Reference material

Files found in the **Reference** folder can be updated manually if you do not wish to wait for a patch update \
But please note that items already logged will not be updated immediately, and if the schema changes the import may break

Base RandomizerMod files: [link](https://github.com/homothetyhk/RandomizerMod/tree/master/RandomizerMod/Resources/Data) \
\(Note: items.json has been modified to adjust item pools to be more convenient\) \
BenchRando files: [link](https://github.com/homothetyhk/BenchRando/tree/master/BenchRando/Resources)\
MoreDoors files: [link](https://github.com/dplochcoder/HollowKnight.MoreDoors/tree/main/MoreDoors/Resources/Data)\
RandomizableLevers files: [link](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/tree/main/RandomizableLevers/Resources)
