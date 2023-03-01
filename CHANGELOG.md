# MIRAGE·XR Changelog

# Change Log

# v2.0 (2023-02-08)

## Featured
- New and improved mobile user interface. We completely redesigned the concept of user interaction and dialogues on the mobile platforms. The app now has a significantly improved usability and user experience, and many new features.
- Context help. We introduced a new dynamic context help system, with a comprehensive set of help dialogues for anything we could think of.
- Interactive tutorial. We added a facility for step-by-step interactive tutorials and added a tutorial introducing the user to the editing functionality.

## Added
- Onboarding: We added onboarding swipe-thru slides on first app launch, explaining the key concepts of mirageXR with text and animations.
- Bottom bar: We added a new bottom tab bar with icons for quickly switching between activity stream, profile, search, and the new dynamic context help.
- Quick edit toggle: We introduced a new edit toggle button in the top right corner of the mobile user interface for quickly switching from viewing to editing.
- Collapsable main panel: Users can now minimize the main menu, providing ‘prev’/’next’ quick navigation buttons in the collapsed view for moving forwards and backwards between action steps. Especially on smaller screen phones, this frees valuable screen real estate up for a less cluttered view of the activity.
- Model augmentation: We added boundary box handles for the model augmentation as alternative to the direct manipulation with pinch and rotate.
- Pick & place augmentation: We added trigger functionality to allow jumping to the specified step if the pick object is placed in the correct target location, and we added reset options for placement.
- Pick & place augmentation: We added sound effects for the pick & place augmentation (for correct and incorrect placement).
- Character models: We added trigger functionality to character models, moving on to the next action step, when audio or animation are finished playing (whichever takes longer).
- Character models: The AI mode of character models now supports the use of “%%trigger%%” control commands in the text string of their dialogue responses, triggering to move on to the next action step.
- Audio augmentation: We added a 'jump to' option for audio triggers (not just 'next step').
- Action augmentation: We added gaze trigger functionality for action augmentations.
- Preview: We added a preview button from the publish settings to remind content authors to test their activities before uploading to the cloud.
- Marker augmentation: We improved the marker augmentation to display the target image with a 'find this’ instruction and we worked on the anchor stability of the task station during tracking.
- Locate: We reintroduced the locate functionality to activate a red arrow viewfinder pointing to the augmentation it is activated for.
- Acknowledgements: We added logos of new collaborators to the acknowledgements.

## Changed
- Login at the start: The app starts now with the login, also adding buttons for registering (opens browser for web registration) and anonymous guest login.
- Activity stream: We upgraded the activity list to a swipeable activity stream on mobiles and improved the sorting and search functionality.
- Publish dialogue: We created a new ‘Publish...' dialogue to simplify data handling for content authors, with quick access for saving locally and saving to the cloud in public (or private).
- Image and video augmentations: We added boundary box handles for easier manipulation to the image and video augmentations.
- Step order: We improved the process of adding action steps during editing.
- Best augmentations first: We reordered the list of augmentations in the editor by popularity.
- Content selection: We improved support for content selection, adding a short description and additional context help to the augmentation list on mobiles.
- Calibration dialogue: We created a new calibration guide and dialogue, which now starts automatically when a user opens an activity to view.
- Highlight current step: We are now visually highlighting the current step in the step list also on mobiles.
- Keep alive: We created a new dialogue with from/to dials on mobiles for the ‘keep alive’ functionality of augmentations, simplifying setting from which action step to which action step an augmentation shall remain visible.
- Updated views: We implemented new views for activity settings, steps,  list of step contents, content selection, step settings, and profile.
- Activity: We improved the process of deleting activities.
- Screen layout: We improved the basic screen layout for tablets and large screen phones (e.g. Motorola Edge Pro). We increased icon resolutions.

## Fixed
- We updated the reference resolution for the new mobile user interface, which was causing crashes on some devices.
- We extended the audio trigger functionality to work also with the GhostTrack augmentation.
- Fixed portrait mode for video augmentation player.
- We fixed bugs with the AI mode for character models.
- We fixed bugs causing the image augmentation to crash (iOS), reset (Android), or not display (all).
- We suppressed the calibration video instructions from displaying during the editing tutorial, so that the optional dialogue does not occlude the tutorial.
- We fixed issues with the appearance of character models ('zombie mode') and improved the appearance for some.
- We fixed issues with calibration.
- We fixed a bug causing label augmentations to crash when using the trigger.
- We fixed a bug with pick & place objects forgetting their orientation.

## Enterprise
- We updated the base URI for xAPI statements, retiring ‘wekit-community.org’.
- We removed the Android advertising SDK package from project (it was never used).
- We added a new profile setting for selecting repository servers from a dropdown list of preconfigured endpoints.
- We added new profile settings for selecting the learning record store from a dropdown list, automatically configuring their xAPI endpoint URL, replacing the free text entry.
- We fixed issues with the Sketchfab API direct login and authentication.
- We reinstated the broken deep link launch from QR codes in Moodle (allowing MirageXR to launch from any QR-code enabled mobile camera app).
- We added new app icons.

## Developer
- We migrated to Unity 2021 LTS and updated the CI pipelines to use the corresponding images.
- We updated the ARfoundation versions.
- We created a new UI kit to unify the presentation layer.
- We introduced a new dialogue manager for presenting interactive dialogues.
- We implemented a new drag & drop controller for ordering of UI elements.
- We added CONTRIBUTING.md instructions, replacing the agile development wiki page.
- We updated the CI pipeline badges in the README.md.
- We excluded workspace layout settings from git index and added to gitignore.
- We removed the UserSettings folder from git index and added it to gitignore.
- We removed some ghost meta files that were still tracked by git.
- We updated the cache action on the Android build pipeline.
- We fixed the Android CI pipeline signing error and left a note in the CONTRIBUTING.md about not ticking the developer key option in the build settings.
- We fixed the problem of the Android build pipeline running out of space before concluding the build.
- We added missing standard Windows fonts on the Android build pipeline, which were causing many dialogues to not display type in automated preview builds.
- We added StyleCop support and adjusted the rules to our needs, also reformatting large parts of the code to fix some of the warnings.

### iOS

Version 11.0+ (ARKit required)

### Android

Version 7.0+

### UWP (Windows Holographic)

10\.0.17134.0+

# v1.9 (2022-06-08)

## Featured
- We refactored the core, breaking up ActivityManager and WorkplaceManager into more modular classes, rebuilding a better logic and flow with higher performance. This affects in particular: resuming activities, timing, event handling, bootstrapping, more clean separation of model-view-controller, persistence, addressables, with improved information hiding.
- We added an in-app tutorial for the mobile user interface.
- We migrated the project from GitLab provided by XR4ALL to a new code hosting platform on GitHub, set up new Github runners for Windows and Linux, and refactored our CI pipelines.

## Added
- We added new animations for the character models.
- Pick & place now allows setting whether location resets or is remembered when repeating the activity. We also added contextual help to explain configuration better.
- The model augmentation now has a log-out button, which allows forcing to reauthenticate with Sketchfab.

## Changed
- We fixed a bug in the activity list, which prevented displaying and searching the title.
- We improved the built-in tutorial for the world space smart glasses user interface, changing wording, adding steps, and fixing a bug.
- Several improvements to the character augmentation ensure that: the position marker does not slip so easily underneath the spatial map; handling of triggers; and a bug causing the edit panel to stay visible in the viewer.
- The app version is now passed through via the IEEE P1589-2020 activity model to the repository. 

## Fixed
- We fixed cross-platform issues with the image and video augmentations. 
- The pick & place augmentation on iOS was missing the sphere indicating the target zone.
- In the settings screen on mobile platforms, the labels are now displayed correctly. 
- We fixed an issue causing glyph, character model, and drawing sometimes to forget size or position.
- We fixed an issue with the activity thumbnail not resetting, when creating a new activity. 
- The activity list now correctly updates when returning to it after a new activity was created.
- “https://” is now added automatically to the Moodle URL.
- On the smart glasses UI, the grid augmentation menu now stays open when browsing to an empty action step and back.
- The pointless ‘relocate activity list’ icon is no longer displayed on mobile platforms, only on smart glasses.
- Some QR links were broken due to a bug in the Moodle repository plugin. This is now fixed. 
- We added support for non-Latin characters to TextMeshPro, fixing issues with special characters in passwords and usernames.
- We did some improvement to lighting of holograms.

## Enterprise
- We added a possibility to automatically build multiple apps with separate config files.
- We added an option in the BrandConfiguration class that allows now to specify which augmentation editors shall be made available (with different configurations possible for mobile / smart glasses). 
- It is now possible to configure the xAPI authentication and the Moodle URL via config file.

## Developer
- We migrated the project from GitLab provided by XR4ALL to a new code hosting platform on GitHub.
- We setup new Github runners for Windows and Linux, and refactored our CI pipelines, migrating to GitHub actions, finally also adding a dedicated Android CI build pipeline.
- Add abstraction layer for addressing GameObjects in the scene.
- We introduced formal reviews as part of the changed approval process: every merge request is now reviewed and upon acceptance the original developer has to merge. This improves capacity of sprint masters, shifting workload back to the originating party.
- We improved documentation, including with UML, in particular of the core and the connected augmentation system.
- We implemented automatic syntax checking with StyleCop.
- We cleaned the repository of some 3D models that are no longer used.
- On mobile platforms, we replaced Vuforia with AR Foundation.
- Notifications on slack now include the merge request details.
- Pipeline preview builds now display the correct version number.

### iOS

Version 11.0+ (ARKit required)

### Android

Version 7.0+

### UWP (Windows Holographic)

10\.0.17134.0+


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
