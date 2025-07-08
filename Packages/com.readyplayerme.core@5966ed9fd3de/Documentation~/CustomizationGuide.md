# Customization Guide

This guide will help you to customize the Ready Player Me avatar creator to fit your needs. The UI was designed in a way that separates the logic from the UI elements

## Requirements
You are allowed to change the entire UI, the only thing that you are required to have in your custom implementation is the Ready Player Me sign-in button and Ready Player Me account-creation UI. This is a legal requirement from Ready Player Me.

![image](https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/c80e9bfa-a635-4ed4-878a-be9506d7d7c1)

![image](https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/c2d8135b-f8e5-4c9d-9ea6-ca00f4af3514)


## Creating a Custom UI From Scratch

All required APIs are provided in the package. They can be used to create a new custom UI. The package also contains a sample UI that can be used as a reference. The following are the most important APIs that you will required.
- AuthManager - Requests for handling authentication.
- AvatarManager - Requests for creating, updating and deleting avatars.
- PartnerAssetManager - Requests for loading partner assets.

## Customizing the Sample

 The sample comes with a default UI similar to the Web avatar creator. The UI is built using uGUI. All major UI components such as screen, asset buttons, asset type panels are prefabs, and can be edited easily. For detailed description of structure of the sample please see the [Sample Structure.](SampleStructure.md)

### Changing background color
- Select different screens under UI > AvatarCreatorCanvas > Creator > Screens and set color in image component or change the sprite.
- Select camera and change the background color.
- Select header under UI > AvatarCreatorCanvas > Creator and change the color.
- Select LoadingManager and change color for either type of loading screen.

Following demonstrate on how to change the background color of different screens. 

https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/ae412932-1bd9-4d00-b6b5-6525adecf9c7

### Adapting UI according to portrait mode
- Select a panel prefab from the "Prefabs/Panels" folder that you want to modify.
- Adjust the size and position of the panel to suit your desired location.
- To switch from a vertical to a horizontal layout, make the following changes:
  - Locate the Grid Layout Group component and adjust the constraint from "Fixed Column Count" to "Fixed Row Count".
  - Find the Scroll Rect component of the panel and select the "Horizontal" option while deselecting the "Vertical" option.

Following demonstrate on how to change avatar creation selection panels according to portrait mode. This is done in runtime but can be replicated by changing the prefabs as mentioned above.

https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/f706e33d-8fb8-4226-8d3c-3e5b6bb17026




