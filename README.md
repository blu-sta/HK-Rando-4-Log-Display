# HK Rando 4 Log Display
## v1.0.1

### How to use this app

This application is standalone, and does not need to be in any Hollow Knight folder \
I leave all the files in a folder in Downloads, and then have a shortcut on the Desktop

This version is compatible with:
- Randomizer 4
- Randomizable Levers
- Rando Plus (Mr Mushroom)
- Transcendence (Charms)

**Top bar**: Shows the rando mode and seed

**Item Helper Tab**: \
`Group by` button toggles how locations are grouped together \
Options include by `area`, by `room & area`, by `room`, and `no grouping` \
`Order` button toggles the ordering of locations (areas and rooms are always alphabetised) \
Options include `alphabetically`, by `time` \
The time value can be displayed or hidden in the alphabetical view \
`Previewed Locations` appears at the top if there are any previewed locations \
Locations show under pool grouped expanders, alphabetically, with room code, item and cost (if applicable) \
`Previewed Items` also appear at the top if `Previewed Locations` appears \
This groups by the rewards of the previewed locations e.g. if you need to buy grubs, charms, etc \
`Groups` are expandable and will show the number of locations in the group (as well as rooms if the appropriate grouping is selected) \
If a group is cleared while expanded, and logic adds more locations to the group, the group will return in the expanded state \
If a room is expanded, it will be expanded in all group options \
`Time` for locations is saved when application is closed and will be restored next time it is opened \
`Expand All` button expands all existing groups (cleared groups are unaffected) \
`Collapse All` button collapses all existing groups (cleared groups are unaffected)

**Transition Helper Tab**: \
All buttons function the same as those in the **Item Helper Tab**, but do not affect the other tab \
There are no previewed transitions, otherwise simply replace location with transition for the previous explanation

**Item Tracker Tab**: \
`Group by` button toggles how items are grouped together \
Options include by `curated groups`, by `pool`, and `no grouping` \
`Order` button toggles the ordering of items \
Options include `alphabetically`, by `time` \
Unlike the helper tabs, time of collecting the items is not currently recorded or displayed \
Pool order is fixed when curated, and alphabetised when grouped by pool \
`Expand All` button expands all existing pools \
`Collapse All` button collapses all existing pools

**Transition Tracker Tab**: \
`Group by` button toggles how transitions are grouped together, similar to the **Item Helper Tab** and **Transition Helper Tab** \
`Order` button functions the same as the **Item Tracker Tab** \
And similarly, time is not tracked, only order \
Expanding and collapsing groups also works the same as the **Item Helper Tab** and **Transition Helper Tab**

**Item Spoiler Tab**: \
Equivalent to the Item Tracker Tab, but with all the information for the seed

**Transition Spoiler Tab**: \
Equivalent to the Transition Tracker Tab, but with all the information for the seed

**Settings Tab**: \
Displays the settings used to generate the save file \
If `Split Group Settings` are randomised on start, the contents of this section would spoil the groups

If something goes wrong while using this application, feel free to contact me on discord \@blu.sta#9997

### To Do List
- Add "Reverse Transition" option to allow for grouping by destination instead of source
	- Would have been helpful in my last UURRando
- Add alternative view for item spoiler to find what is at a specific location
- Consider tracking reachable items (grubs, essence, etc)
	- TrackerDataDebugHistory.txt file
	- "New reachable vanilla placement: Grub at "
	- Entry when collected??
- Randomised pool tracker to link pools as items are discovered
	- The settings in the TrackerLog spoils the pool contents
	- This would be used so you don't need to remember what you have already discovered to be in a pool
	- i.e. do the remembering for you without spoiling everything

### Reference material
Source for json files [link](https://github.com/homothetyhk/RandomizerMod/tree/master/RandomizerMod/Resources/Data)
