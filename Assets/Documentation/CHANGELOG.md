2.0.28-alpha    27.08.2019
- Added a script to Canvas that detects the size of a device at the start and
  changes reference value if necessary.

2.0.27-alpha    27.08.2019
- Bug #157 fixed, where all lamps where added to workspace at start.
- Bug #158 fixed, where coming from video mapping disables selection.

2.0.26-alpha    27.08.2019
- Selecting is disabled through color wheen.

2.0.25-alpha    27.08.2019
- Polling ssid lists from lamps and showing in client mode settings.
- Bugfix, where default name in settings menu didn't work.
- Possible UI fix, where settings menu was empty (only in clound builds).
- Bugfix, where loading a workspace also added one lamp to scene, that caused
  lamp duplication.
- Possible bugfix, where deselected lamps applied itsh from color wheel.
- Added "select all" & "Deselect all" buttons to draw menu.

2.0.24-alpha    27.08.2019
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