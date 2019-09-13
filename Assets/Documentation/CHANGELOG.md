2.0.49-alpha                                                          13.09.2019
- Itsh -> Itshe
- Dialog system
- Loading system
- Sending video buffer to lamps on load
- Dialog box title font size from 46 to 50 and increased the box size, so all
  titles in the app have the same size.

2.0.48-alpha                                                          13.09.2019
- Changed the sliderToggle rect value to 1920 unknown issue.

2.0.47-alpha                                                          12.09.2019
- Dialog box title font from 50 to 46 to display whole message
- Grammar error fix from Explenation to Explanation
- Fixed an issue with buttons where the arrow sometimes was in wrong direction
and did not allow to press hide a menu or crashed the app.

2.0.46-alpha                                                          09.09.2019
- Added op codes into the network.

2.0.45-alpha                                                          09.09.2019
- Effect slider added
- Fixed issue with value downscaling
- Added outline for text 

2.0.44-alpha                                                          06.09.2019
- Colorwheel inspector arrow now expands the sliders.
- Changed the padding between options
- Increased the size of the "SIZING" buttons

2.0.43-alpha                                                          06.09.2019
- Fps should be changed without sending video metadata with new start time.
- Removed debug messages.

2.0.42-alpha                                                          05.09.2019
- Fix #174: Implemented Play pause stop in video mapping.

2.0.41-alpha                                                          05.09.2019
- Temperature slider step value from 1 to 100

2.0.40-alpha                                                          05.09.2019
- Fix #170: Lamp didn't send new metadata if moved in video mapping.
- Fix #171: New lamp added from network in video mapping crashed program.
- Fix #172: Fps field didn't change video fps in video mapping.
- Fix #175: Going to video mapping didn't bring all lamps with the same video.
- Remove:   Color wheele is not part of the canvas prefab anymore.
- Refactor: Tweeked some UI settings.
- Fix:      Loads can be deleted now.

2.0.39-alpha                                                          05.09.2019
- Added sliders to ITSH menu

2.0.38-alpha                                                          02.09.2019
- Added and improved scroll rects.
- Picking video takes to videom mapping right away.
- Fixed mapping error in video mapping.

2.0.37-alpha                                                          02.09.2019
- Removed: All unnessesary UI from video mapping.
- Change: Moved video setting to draw menu.
- Added #169: New project save, load and handling system.
- Added #169: Saving lamps UI coordinates on video.
- Added #169: Videos are now copied to a application directory, where they stay
  until a project save is deleted.

2.0.33-alpha                                                          29.08.2019
- Net video position struct that lamp saves to save positions on video.
- Downgraded Newtonsoft JSON library and added needed functionality manually.

2.0.32-alpha                                                          29.08.2019
- fixed an issue where "hold" would activate when clicking the ITSH values.

2.0.31-alpha                                                          29.08.2019
- Changed slider behaviour. It now takes equal amount of time to get from one
  end of the slider to another for all options when holding down.

2.0.30-alpha                                                          28.08.2019
- Added network packages ready to use: Set Video, Video Request, Video Response,
  Fps Request, Fps Response, Set Fps, Set Frame.
- Updated Newtonsoft Json library.
- Clicking "Add all lamps" splits lamp on the workspace uniformly.
- Fix #163 where cutout area side in workspace view is blue but transparent in
  video mapping.
- Fix #164 where default color wheel temperature value was 5599. Note: Lamps
  that have been connected before, might have temperature 5599 allready saved,
  so the color wheel shows it.

2.0.29-alpha                                                          28.08.2019
- Polling ssid list from lamp works now fine after lamp software was updated.
  Known issue: UI is bad in ssid list dropdown after ssid list is received #160.
- Master mode requires lamps to be selected.
- Router mode requires lamps to be selected.
- Lazy bugfix for #155, where changing fps gave unity player a random frame
  index.

2.0.28-alpha                                                          27.08.2019
- Added a script to Canvas that detects the size of a device at the start and
  changes reference value if necessary.
- Added new update bundle.

2.0.27-alpha                                                          27.08.2019
- Bug #157 fixed, where all lamps where added to workspace at start.
- Bug #158 fixed, where coming from video mapping disables selection.

2.0.26-alpha                                                          27.08.2019
- Selecting is disabled through color wheen.

2.0.25-alpha                                                          27.08.2019
- Polling ssid lists from lamps and showing in client mode settings.
- Bugfix, where default name in settings menu didn't work.
- Possible UI fix, where settings menu was empty (only in clound builds).
- Bugfix, where loading a workspace also added one lamp to scene, that caused
  lamp duplication.
- Possible bugfix, where deselected lamps applied itsh from color wheel.
- Added "select all" & "Deselect all" buttons to draw menu.

2.0.24-alpha                                                          27.08.2019
- Colorwheel disables moving items in workspace completely.
- Bug, where trashcan deletes object at instance is removed.
- Bug, where colorwheel doesn't cancel picked itsh, when not hitting "KEEP"
  button, is now fixed.

2.0.23-alpha
- Reordered UI based on Kaspar's wishes.
- First lamp is always added to workspace.
- Removed screen DPI debug messages.
- PPS bug fixed (Play / Pause / Stop).

2.0.22-alpha
- Fixed an issue where itsh sliders were too fast on some machines
  (added delta-time).

2.0.21-alpha
- First grouping demo in video mapping

2.0.20-alpha
- Timesync is using the same network interface as lamp communication.
- Lamp now saves video buffer and should play it if going back to workspace.
- Sync between video buffer and video player.

2.0.19-alpha
- Fixed an issue where eventPointer location did not update when holding the
  button down when changing ITSH values from sliders.

2.0.18-alpha
- Changed canvas back to bigger.
- Created packet classes for better netwiork communication.
- Selection menu fix.
- Removed "ssids from lamps" option from client network menu.
- Bugfix: Clicking through UI on mobile.
- Using safe area on iOS.

2.0.17-alpha
- Created post build process for iOS.

2.0.16-alpha
- Fixed the cosmetic bug on HUD slider.
- Changed Temperature from 4250 to 5600.

2.0.15-alpha
- Fixed issue with timesync and wifi interface.
- Fixed issue where not approved moving items dispatched events.
- Refactored VideoMapper.cs.
- Created WorkspaceUtils class for lamps easy access.
- Refactured all menus.
- Restructured scripts in assets.

2.0.14-alpha
- Refactored play pause stop (pps for now on).
- Removed metadata sending while play is pressed.
- Added app icon and splash screen.

2.0.12-alpha
- Fixed an issue where rotating and moving lamps was not working as intended
  during gameplay.

2.0.11-alpha
- Fixed issues with camera movement and zooming when using mobile devices.

2.0.10-alpha
- Client / Master / Router mode simple implementation.
- Possible video mapper bug fix, where.

2.0.9-alpha
- Added play / pause / stop buttons. Sending happens on port 30001.

2.0.8-alpha
- Added video_timestamp field to metadata. That is a time, when video started
  in UI in lamp time.

2.0.7-alpha
- Voyager Update Utility is incomplete if compiling for iOS.

2.0.6-alpha
- If lamp is moved on video in video mappingm, metadata will be sent again.
- First lamp update test.

2.0.5-alpha
- Fixed a bug where saving using the automatic savename did not save and caused
  load list to appear empty changed the date format from short to long, as '/'
  symbol causes additional pathfinding and problems.

2.0.4-alpha
- Added time offset with lamps.
- Changed text under SELECT MODE.

2.0.3-alpha
- Added a message button on SELECT MODE to guide the user to select atleast one
  lamp.

2.0.2-alpha
- Removed set button from DMX mode settings, updates are sent every time value
  is changed.
- ITSH json serializetion fix. Added constructor to Itsh class with no
  inputs.
