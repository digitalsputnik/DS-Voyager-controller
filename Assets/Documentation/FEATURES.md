## In this file is list of features that are finished and ready to test.

**Some things to note:**
* Controls menu container is on the right side.
* Inspector menu container is on the left side.

ADD LAMPS MENU
* List of lamps in network.
* Lamps that are allready in workspace should not be listed.
* Lamps that are not up to date should not be listed.                           **NOT FINISHED**
* Lamps that are not connected and/or have bad connection should not be listed. **NOT FINISHED**
* Add All lamps button should add all lamps to workspace with random 
  vertical position.

ADD PICTURES MENU
* Clicking the button should open up file dialogue window.
* Only .jpeg, .jpg and .png files should be available to select.
* Selected image should appear fitting inside view.                             **NOT FINISHED**

SELECT MODE
* Opens another menu in controls container.
* Enables selection on lamps in workspace.
* When at least one lamp is selected, new buttons should appear, including:
    * Video Mapping - takes to another scene with selected lamps.
* When leaving selection mode menu, selecting on lamps should be disabled.
    
VIDEO MAPPING
* Should contain the laps selected from select mode.
* Should contain controls souch as:
    * Set video button
    * Itsh property - clicking on it opens color wheel
* When going back, the workspace state should be the same, before going to
  video mapping.

SET VIDEO MENU
* Should contain list of videos selected before.
* Every video should have a thumbnail.
* Clicking ADD VIDEO should open up a file dialogue and only .mp4 files
  should be selectable.
* After selecting a new video, it should appear to the list.
* Clicking on garbage bin should remove it from the list.
* Clicking on a video item should open the video in a workspace.
* The opened video should be inside a camera view bounderies.
* Lamps should show mapped pixels, video multiplied with itsh.

LAMPS SETTINGS
* Contains list of different settings, including:
    * NETWORK                                                                   
    * DMX MODE

DMX MODE INSPECTOR
* Selection mode is enabled.
* Only selected lamps have universe (u) and channel (c) shown in workspace.
* Has different settings, including:
    * Start universe
    * Start channel
    * Divison
        * Per lamp
        * Per 16 pixels
        * Per every pixel
    * Protocol
        * Art Net
        * sACN
* On set button clicked, the settings will be sent to selected lamps on
  port 30001. This can be checked with WireShark.
* When leaving DMX mode, selection should be disabled and universes and
  channels should dissapear from workspace.

SAVE
* After filling the filename field and pressing SAVE, the workspace should be
  saved without any errors.
* The save should be listed to LOAD menu.
* If saving and a save with the same name allready exists, dielogue should
  appear with "REPLACE" and "CANCEL" options.                                   **NOT FINISHED**

LOAD
* Contains all the saves made in SAVE menu.
* Clicking on a save, the saved workspace should appear.
* Removing the save should remove it from the list.

ABOUT
* The copyright year should be right.
* Version of the app should be correct.