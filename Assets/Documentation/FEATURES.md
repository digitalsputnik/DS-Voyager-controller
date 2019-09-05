## In this file is list of features that are finished and ready to test.

**Some things to note:**
* Controls menu container is on the right side.
* Inspector menu container is on the left side.
* When opening the app with lamps in the network, the first discovered lamp will be automatically added to workspace.

ADD LAMPS MENU
* List of lamps in network.
* Lamps that are already in workspace should not be listed.
* Lamps that are not up to date should have a marking on them.                  **NOT FINISHED**
* Add All lamps button should add all lamps to workspace with uniform 
  vertical position.

ADD PICTURES MENU
* Clicking the button should open up file dialogue window.
* Only .jpeg, .jpg and .png files should be available to select.
* Selected image should appear fitting inside view.                             **NOT FINISHED**

DRAW MODE
* Opens another menu in controls container.
* Enables selection on lamps in workspace.
* When at least one lamp is selected, new buttons should appear, including:
  * Set Video - lets you add video to selected lamps
  * Video Mapping (this only appears if selected lamps have same video) - takes to video mapping view with all lamps associated with the video that is on selected lamps.
* When leaving selection mode menu, selecting on lamps should be disabled.

ITSH menu - COLORWHEEL
* Changes the color settings of the lamp.
* Hue and saturation can be changed from the wheel or sliders on the side menu.
* Intensity and temperature can be changed from the sliders.
* Effect slider will change the proportion of effect applied with 0% - only color will be applied, 100% only video will be applied.  **NOT FINISHED**
    
VIDEO MAPPING
* Should contain the lamps associated with video.
* Should contain following video controls:
    * FPS - lets you choose the framerate for video.
* When going back, the workspace state should be the same, before going to
  video mapping and the video should be playing on lamps as mapped.

SET VIDEO MENU
* Should contain list of videos selected before.
* Should contain list of preset videos. **NOT FINISHED**
* Every video should have a thumbnail.
* Clicking OPEN NEW should open up a file dialogue and only .mp4 files
  should be selectable.
* After selecting a new video, it should appear to the list.
* Clicking on garbage bin should remove it from the list.
* Clicking on a video item should open the video mapping menu.
* The opened video should be inside a camera view bounderies.
* Lamps should show mapped pixels, video multiplied with ITSH and effect. **NOT FINISHED**

LAMPS SETTINGS
* Contains list of different settings, including:
    * NETWORK - Changes network mode on lamps (Master/Router/Client)                                                                   
    * DMX MODE - Enable and disable DMX mode on lamps
    * FORCE UPDATE - Updates selected lamps

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
* Universe and channels are automatically calculated for each lamp.
* When leaving DMX mode, selection should be disabled and universes and
  channels should dissapear from workspace.
* Lamp should be grayed out when on DMX mode but it should display universe and channel. **NOT FINISHED**'

NETWORK MENU
* CLIENT MODE - Will connect lamp to selected network.
* ROUTER MODE - Creates WiFi access point with its serial and "_M" added as SSID.
* MASTER MODE - Creates WiFi access point with its serial as SSID.

SAVE
* After filling the filename field and pressing SAVE, the workspace should be
  saved, with video mapping - project.
* The save should be listed to LOAD menu.
* If saving and a save with the same name already exists, dialogue should
  appear with "REPLACE" and "CANCEL" options.                                   **NOT FINISHED**

LOAD
* Contains all the saved projects made in SAVE menu.
* Clicking on a save, the saved workspace should appear.
* Removing the saved project should remove it from the list.

NEW PROJECT
* Creates new empty workspace **NOT FINISHED**

ABOUT
* The copyright year should be right.
* Version of the app should be correct.
