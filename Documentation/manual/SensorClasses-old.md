# MirageXR: Sensor Classes

This document will give a short overview over the c# classes used for
sensor recording, their functionality and how to work with them.

## DataPlayer.cs

The class that replays recorded data using a PlayerModel. Usually is
used on the player prefab that gets instantiated by a SensorAnnotation
class that wants to replay a recording.

Data is being played in a loop in the Update method after being
activated with the Activate method from an external source (usually the
SensorAnnotation that instantiated the PlayerModel prefab).

Additionally has methods to send a start point, end point and apply
those to the currently active record data to cut it.

If you want to play additionally data you need to add those into the
SaveData class and then add a component to the PlayerPrefab to visualize
that data by manipulating the added component in the Play method where
SaveData is pulled for every frame.

## HandsTrackingManager.cs

Class that tracks the position of the user's hands (if they are in the
FoV). Updates its information to the UIDisplayAPI class so it can be
recorded by the AnnotationEditor.

The TrackingObject is the object that will be used to visualize the
hands in the real-time view; replace the reference if you want to use
a different model.

To replace the hands in the record player you need to add two models to
the PlayerModel prefab and reference those in the DataPlayer script on
the prefab as Hand1 and Hand2.

The code that is responsible for the tracking was not written by used,
it was built after online examples.

## UIDisplayAPI.cs

The class that tracks the Headposition, Gazedirection and Handpositions
and transmits that data to the AnnotationEditor so it can record the
data.

If you want to track and record additional internal HoloLens data it is
recommended to read the data in this Update method and give it to the
AnnotationEditor where you can write it into a SaveData object with the
Record method (need to extend the SaveData class itself, too).

Also the debugging info that is displayed in the top left corner of the
HoloLens screen is created here; most sensor related classes have a
reference to this class so you can just replace the public test1-3
variables with the data you want to display from anywhere. This serves
purely debugging purposes and has no other functionality.

## WEKITCombinedAudioSensorAnnotation.cs

A simple Annotation class that has the methods needed for recording and
replaying and is used to call an AudioAnnotation and a SensorAnnotation
Editor at the same time so an annotation can record audio and sensor
data at the same time

## WEKITSensorAnnotationEditor.cs

The central class of the sensor recording; data is recorded, saved
into SaveData objects and stored here.

Needs a prefab reference of DataPlayer to instantiate it to replay
recorded data. Finds references to the UIDisplayAPI DataManager and
its own player automatically. The private _recordList variable is
where the recorded data for the annotation is stored.

Records data every 0.04 seconds after the StartRecording method is
called; will then call the Record method every 0.04 seconds (25
frames per second) where a SaveData object is created, filled with
this frames sensor information and saved into the _recordList.

If you want to record additional data that is to be stored and
recorded with the sensor data, add the needed variables to the
SaveData class and fill them in the Record method. For that
reason all record data needs to be referenced to this class.