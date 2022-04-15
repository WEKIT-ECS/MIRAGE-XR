# MIRAGE·XR Changelog

# 1.4 (2021-05-06)

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

# 1.3 (2021-04-15)

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


# 1.2 (2021-03-23)

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


# 1.0 (2020-10-23)

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
