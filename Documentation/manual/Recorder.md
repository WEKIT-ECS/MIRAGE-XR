# MirageXR: Recorder: Basic Interaction

Task stations are created using a double tap while the gaze pointer is hitting the spatial map. Once created, air-tapping a task station (TS) will open the menu. Tapping a menu item will create a copy of the object, though distinguishable by a ring surrounding it (menu item = no ring).  Each annotation has it's own menu that will manage the content stored in each. Task stations and annotations also have configuration menus (containing object-level functionality, such as delete and copy), which will have their functionality set in the top level manager objects (in Unity).  

# Code Structure

The new code structure follows a model-view-controller approach([more info](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller)). Following this, we have created the following structure:

Model Element (ARLEM)	| View Component		| Controller				| Functions
----------------------- | --------------------- | ------------------------- | ---------------------------------------------
My Activity				| ActivityModel (None)  | ActivityController.cs		| Create Task Stations, Open Task Station Menus
Action					| Task Station (prefab) | TaskStationController.cs	| (De)activate Task Stations, Create Annotations
EnterExit/ToggleObject	| Annotation (prefab)	| AnnotationController.cs	| (De)activate Annotations, Invoke Functions

## Controllers
These are the code interfaces that handle the movement of data as well as changes to the views.

### ActivityController.cs
This script is attached to the ActivityModel gameobject in the root of the 'Recorder' scene. It requires input to the following public variables:

1. MyWAM - Link to the World Anchor Manager, a component of the 'HoloManager' gameobject (same scene).
2. World Origin Marker Prefab - this is the visualisation of where the person started the app. Should not be null.
3. Task Station Prefab - the visualisation to use for all task stations.
4. Debug Text - this is a text object found in the 'HUD' root gameobject. For development, though can be used to display any information that should be user-locked.

### TaskStationController.cs
This controller is attached to the Task Station prefab. Every task station has its own controller. A number of elements are accessable through the Unity inspector:

+ Gaze focus color and magnification - the change in colour and size when the gaze pointer is hovering over the task station.
+ Menu Animation Settings - this sets set the speed of menu openings and the distance of the menu items (radially) from the centre object.

### AnnotationController.cs
Every created annotation has a controller, which acts as a go-between for the various functions associated with capturing, replaying and storing data.  The process of creating and modifying annotation types is explained [here](ReadMe_AnnotationsForDevelopers.md).

