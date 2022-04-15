# Character Augmentation

Character augmentation allows the user to create an intelligent avatar 
which can do different behaviors including following a path or the player, 
talk and play different types of animations. All behaviors are configurable
in all action steps. 

![Annotation Overview](../resources/Recorder/character_settings.jpg)


## Save and load 
All setting for all steps will be saved in a JSON file in a "characterinfo" 
folder in activity folder. Each character will create a JSON file in this 
folder with the name of the character poi.id. At the start of each step 
a method "ParseCharacters" will be called and load the character and the settings 
for that step.


## Edit/Play mode

The setting panel is only available in edit mode. In addition some of the behaviors 
are appears only in play mode. For example looping for the path following and trigger
for moving the steps.

## Classes

	1. CharacterAugmentation: Create the character and the first node. Common methods with other augmentations are implemented here.
	2. CharacterController: Responsible for all behaviors and also loading and saving the data. This is the main class of the characters.
	3. Destination: Contoll the path nodes. Each node has a destination component attached.
	4. CharacterSettings: Holds the character settings
