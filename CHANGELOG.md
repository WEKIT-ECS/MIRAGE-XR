# MIRAGE·XR Changelog

# v1.8 (2021-11-12)

## Featured 
- Updated Keyword Manager. MirageXR now uses voice prompts "Hey Presto", "Hey Mirage" and "Sim Sala Bim" to start voice interaction. Voice interaction has also been improved by adding visual cues for spoken words.
- IBM Watson Integration. Character augmentations can now be fitted with an IBM Watson assistant, improving user interaction.
- Tiltbrush. MirageXR now includes the popular XR painting tool Tiltbrush! The new augmentation  allows adding 3D-painting elements to the training procedures.

## Added
- Most visual augmentations can now be used as a trigger for the next action step.
- Gaze triggers can now be used to jump to any action step.
- Near-touch interaction added for dialogues on HoloLens2.
- 3D object manipulation is now possible through widgets at the object’s boundaries.
- Added a toggle to the mobile video editor panel that allows a user to input the orientation of a video augmentation.

## Changed
- The MirageXR in-app tutorial has been updated, improving performance and maintainability.
- Special characters like ampersand (&) are now disallowed in activity names, as this could cause problems with Moodle.
- Some unnecessary voice lines were removed from the MirageXR voice assistant Maggie.
- The glyph trigger option was moved to the editor panel.
- On loading or creating a new step, the step will be labeled as the current step (in orange, not gray).
- Disabled shadows on HoloLens and improved their quality on other platforms.
- Icons and splash screens were updated.
- The rotation of the character augmentation was improved on mobile devices.
- The default placement of UI elements on HoloLens was optimized.
- The drawing augmentation button was removed on mobile platforms.
- The augmentation panel on task stations was changed from list to a grid view.

## Fixed
- A bug where audio would not replay on ghost augmentations was fixed.
- The spawn point is now correctly visible on HoloLens 2 for both eyes. The custom shader was replaced with a wireframe sphere.
- Editing ghost augmentations will now also replace old audio with the new one.
- The Home button on the Aura has been moved slightly up to avoid being under the floor line.
- The Pick&Place augmentation no longer forgets the target sphere size.
- Deleting a model used by Pick&Place now works correctly.
- The GhostTrack animation and its sound are now synchronised.
- The Sketchfab thumbnail is now correctly displayed.
- The video editor now opens correctly for editing.
- A visual bug where action steps could not be deleted was fixed.
- An issue with the audio augmentation where the loop and trigger options would interact badly was fixed.
- The rotation and scale of the Image augmentation are no longer resetting on load.
- An issue where the Aura would jump up when sitting was fixed.
- The calibration guide is no longer causing errors when selecting “Don’t show again”.
- Private activities are now displayed correctly on Android.
- The bounding box on the measure tool was disabled, as it was causing errors.

## Developer
- Some old folders and files have been removed or moved.
- Dropdown and guide text of character augmentations is replaced by Unity native UI elements.
## Platform Compatibility

### iOS

Version 11.0+ (ARKit required)

### Android

Version 7.0+

### UWP (Windows Holographic)

10\.0.17134.0+

# v1.7 (2021-10-07)

## Featured:
- Spatial learning analytics. New xAPI vocabulary now logs learner behavior data for all augmentations. Improved xAPI support now adds context and results where needed.
- Stand here action. A new ‘stand here’ glyph has been added.
- Auto progression. Auto progression has been added to audio and video augmentations, complementing the already existing auto progression option for character models.

## Added:
- Auto progression to the next action step has been added as an option to the audio and video augmentations, complementing the already existing auto progression option for character models.
- New ‘stand here’ glyph added
- Improved xAPI support now adds context and results where needed
- New xAPI vocabulary now logs learner behaviour data for all augmentations

## Changed:
- The task station’s context menus are now rotated slightly towards the user, bringing the outer panels closer to the eye (Windows Holographic).
- Hover guide text has been added to the character augmentation menus to explain its components.
- Unavailable character augmentation animations will no longer be shown in the drop down menu.
- Augmentation spawn point is now visualised (known bug: renders only on one eye on Hololens).
- Image marker size can be set allowing for more accurate image tracking.
- Ghost augmentation can be replayed without changing the action step (audio replay option still to follow).
- Empty rows of the action list no longer block interaction with virtual objects behind.
- The audio augmentation editor was simplified.

## Fixed:
- Improved virtual keyboard on windows holographic platforms, including resolving a bug that caused the keyboard to close prematurely
- Pick and place no longer forgets the 3D model when there are multiple action steps 
- Activity thumbnail images can be previewed again
- Augmentation buttons are now correctly deleted for an action step if the corresponding augmentation has been deleted
- Video recorded on a mobile device no longer plays back distorted on Hololens

## Developer:
- Updated Unity version to 2020.3.13f1
- Updated MRTK version to 2.7.2
- Reduced the space requirements for pipeline builds 
- Updated NuGet package

## Platform Compatibility

### iOS

Version 11.0+ (ARKit required)

### Android

Version 7.0+

### UWP (Windows Holographic)

10\.0.17134.0+

# 1.6 (2021-08-12)

## Featured
* New possibility for AR measuring. Measuring functionality is added to the act augmentation 'measure' and measurements are stored as result in the xAPI learning record store.
* Navigation assistance. The navigation arrow points the user to the selected target augmentation, one per step.
* New mobile UI is complete. The new mobile UI is now functionally complete with edit panels for each augmentation and calibration.

## Added
* Female avatar for the ghost track augmentation.
* Voice commands "next" and "back" allow switching between steps (only on Hololens).
* Label trigger functionality now available also on iOS and Android.
* Characters now process 3D models as obstacles and will avoid bumping into them.
* Characters are now resizable.
* 'Loading' interstitial when loading or creating an activity.
* Character models will now be included in the ARLEM zip file.
* Auto-progression for character augmentation: checkbox now allows moving automatically to the next action step once animation or audio finished playing (whichever takes longer).

## Changed
* The activity will be reset when the last step is completed.
* Deadline date format is corrected.
* Orkestra support removed (for now).
* Improved glyphs for 'act' augmentation (scissors, all box glyphs).

## Fixed
* Reduced loading time for new activities.
* Marker augmentation now also works on Android and iOS.
* The Pick and place augmentation now also supports 3D models (alternative to the standard 'arrow' shape).
* Improved character movement on mobile devices.
* Closing the augmentation panel now brings up the list of augmentations again.
* Character position will no longer break when switching between action steps.
* Next task station is again locked in edit mode.
* Floor line from the aura to the activity list is visible again.
* Ghost origin will be loaded correctly.
* Model augmentation (Sketchfab) now also works on mobile devices.
* Mobile UI icons are now no longer hidden by android system icons.
* The image presentation animation no longer appears only with keep alive over several action steps.

## Enterprise
* New 'support' button (in the Moodle login panel) opens a defined support forum in a browser.
* New plugin augmentation allows packaging of 3rd party mini-apps as prefabs in the ARLEM zip file.

## Developer
* Android CI pipeline added.
* Separate TestFlight link for development builds.
* Sketchfab client secret now in untracked config file 'SketchfabClient.json' (excluded from tracking via .gitignore; you need to manually copy the SketchfabClient.json to your project folder).

## Platform Compatibility
### iOS
Version 11.0+ (ARKit required)
### Android
Version 7.0+
### UWP (Windows Holographic)
10.0.17134.0+


# 1.5 (2021-06-09)

## Featured
* New UI for mobile devices. New flat user interface for mobile and tablet devices with iOS and Android.
* Pick and place augmentation. The new Pick and place augmentation which allows the users to flag the real world by an arrow.
* GhostTrack aesthetics. New aesthetics for the ghost model in the GhostTrack.

## Added
* Augmentations can now be kept alive between action steps.
* New character models were added to the character augmentation.

## Changed
* The ghost model is replaced with a better avatar model.
* Calibration now restarts the current activity.
* A warning is given when a user tries to download a model with a large file size.
* Character augmentation settings panel has been refactored.
* Character models now handle occlusion by the spatial map.
* It is possible to add different audio, animation, path and movement to the character in each action step.
* The Image Marker augmentation has been re-enabled on Hololens 1 and Hololens 2 However, it is still disabled on iOS and Android.

## Fixed
* The iOS or android apps no longer show a Vuforia license error when first launched.
* Pick and place now stays in edit/play mode when changing action steps.
* Keyboard predictive text now works correctly for Hololens 1 and Hololens 2.
* The Keyboard no longer dismisses itself on Hololens 1 and Hololens 2.
* A bug causing some sketchfab models to not be loaded has been resolved.
* Task stations are correctly spaced apart when loading an activity without calibrating.
* Task stations no longer rotate when they are moved.
* Fixed Moodle repository upload limit to allow for activity models with more than 64kb.

## Enterprise
* No changes

## Developer
* Vuforia License key has been changed.

## Platform Compatibility

### iOS
Version 11.0+ (ARKit required)


# v1.4 (2021-05-06)

## Featured
* New flat user interface for mobile and tablet devices (iOS and Android).
* Pick and place augmentation which allows the users to flag the real world by an arrow.
* New aesthetics for the ghost model in the GhostTrack

## Added
* New voice commands for moving the action list: “Move action list” and the activity list: “Move activity list” (Only on Hololens)
* Pagination for the model augmentation
* Dialog recorder for character augmentation
* Basic lip-sync for character augmentation
* Authors of activities can delete them from the server directly from the app (only on Hololens)
* Improved update of activities stored in the cloud.
* Activities from course assignments listed first now (with deadlines displayed).
* A new animation “Image Display” added to the character augmentation. The user can assign an image annotation to this animation, and the character will show up the image.
* Gaze trigger for label augmentations.

## Changed
* The ghost model is replaced with a better avatar model.
* Terms of use and privacy policy are updated.

## Fixed
* Sketchfab models do not remain in the ARLEM folder anymore after deleting.
* Only the first video could be displayed in each step even if the users had several video augmentations.
* First/Last step buttons and the next and previous step buttons of the Action list now function correctly.
* Login UI is now placed in the correct place at the start of the app.
* The annotation menu is no longer crashing when all steps are deleted.
* Ghost is no longer losing arms on Android and iOS
* Character augmentation models without a dialog recorder function are no longer causing a crash after loading the activity.

## Enterprise
* Terms of use added
* Improved repository functionality

## Developer
* Vuforia updated to version 9.8.5.
* xAPI updated and tincan removed.
* The empty dialogues in CI pipeline build were fixed.

## Platform Compatibility

### iOS
Version 11.0+ (ARKit required)

### Android
Version 7.0+

### UWP (Windows Holographic)
10.0.17134.0+


# v1.3 (2021-04-15)

## Featured
* The new image marker augmentation allows the user to take a photo of an object or an image target and thus allow to move task stations with the marker around.
* The new character augmentation allows the user to add a character to follow a path, follow the player or play various actions in place.
* The new VFX augmentation allows the user to add 19 different visual effects.

## Added
* New annotation menu which is linked to the task stations
* New Image marker augmentation which allows the user to take a photo of an object or an image target and thus allow to move task stations with the marker around.
* New Character augmentation which allows a character to follow a path, follow the player or play various actions in place.
* New VFX augmentation with 19 different VFX effects.
* Wekit:// protocol is registered. The play button on Moodle plugin will open the activity on MirageXR.
* The users can update their activities or clone a new copy of the activities of the other authors.
* Privacy system is added to the ARLEM files. It is possible to set an ARLEM file as private or public by the user or by the admin of the Moodle site.
* New Audio recorder/player with time slider
* Spatial audio with configurable settings (Radius and loop)
* New calibration marker which will be activated with gazing
* Thumbnail contains holographic contents.
* The authentication can be saved and the user will be logged in automatically at the start.
* The activities which are used in the user courses will be displayed on top of the activity list.

## Changed
* Native keyboards are replaced with the old keyboard.
* The local/downloaded activities can be deleted.

## Fixed
* Label augmentation was not displayed on Hololens.
* Tutorial manager got stuck.
* The number of action steps was not updated.
* Image and video augmentation did not work sometimes on iOS.
* OpenID Connect was fixed for iOS and Android.
* Ghost and its audio did not have the same frame rate and could not be matched.

## Enterprise
No changes

## Developer
* Improvement suggestions by Roslyn analyzer were applied to the project.
* Calibration unit test is added.
* Platformmanager and ARManager will not allow AR foundation to be initiated on Hololens.
* The activity and workplace json files will be stored on the database on Moodle server.
* Orkestra integration

## Platform Compatibility

### iOS
Version 11.0+ (ARKit required)

### Android
Version 7.0+

### UWP (Windows Holographic)
10.0.17134.0+


# v1.2 (2021-03-23)

## Added
- Support for iOS
- Near interaction support for UI
- 3D Model augmentation
- Sketchfab connection for importing models
- Additional action glyphs and a scrollable grid for selecting them
- Activity cover thumbnail
- Walkthrough tutorial introduces to the basic functionality on first start
- Optional thumbnail photo now provides preview of Learning Experiences
- Config panel for Moodle login information
- Display app version number on activity list
- OpenID connect support for Sketchfab
- Realign button near aura to recentre panels if lost / stuck in wall
- Image and video augmentation support added for iOS and Android
- Display file size before cloud download of Learning Experiences

## Changed
- Connects to Moodle server for ARLEM files and content
- UI panels more compact
- New UI for image and video players
- Improved mesh display of spatial map in edit mode on Android / iOS
- Improved activity list paging to improve display performance
- Task card closer to task station
- Video always uses audio
- new icons and splash screen

## Fixed
- Surface magnetism solvers added to keep holograms out of the walls
- Workplace calibration fixed, activities now can again be transferred across devices / places
- Activity List properly refreshes now when new activities are created
- Improved keyboard handling (device specific and improved Hololens keyboards)
- Fixed all offsets of all augmentations 
- Ghost sometimes got stuck when moving to next action
- Sometimes small spheres appeared randomly scattered in the activity
- Upload uses Activity Title
- Glyphs can be rotated
- Loading previously saved ARLEM failed
- In some situations, labels would be empty

## Developer
- Developer workflow documented in Wiki
- Updated to MRTk 2.5
- Intelligent rendering via PlatformManager allows building platform-specific UI
- BrandManager allows customising look and feel via config file
- CI release pipeline and notifications
- CI installer builds for HoloLens 1 and 2
- Automatic build version numbering
- More unit tests added

## Platform Compatibility

### iOS
Version 11.0+ (ARKit required)

### Android
Version 7.0+

### UWP (Windows Holographic)
10.0.17134.0+


# v1.0 (2020-10-23)

## Added
- Support for HoloLens 2
- Support for Android
- Unit tests
- CI pipeline
- Home pathway that leads to the main menu
- Task stations are now visualized as a diamond

## Changed
- Updated to MRTK
- Combined player and recorder into one common editor
- Redesigned user interface
  - Updated aura visuals
  - Updated pathway visuals
  - Redesigned the action list menu
  - Updated text label visuals
  - Streamlined activity download and activity starting with an activity download menu
  - Replaced circular edit menu with own editor menus for each annotation
- Menus do not follow the user's head movement anymore but can be placed in a fixed position in space
- Tweaked scaling of objects
- Updated to new logo

## Fixed
- Annotation positions can now be saved in the correct format independent of the user's culture settings
- Event order of central event manager improved
- Performance improvements & code refactoring
