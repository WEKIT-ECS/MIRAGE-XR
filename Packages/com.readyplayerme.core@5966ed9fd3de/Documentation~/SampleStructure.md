# Sample Structure

## Avatar Creator State Machine
The avatar creator is based on a state machine pattern. Its used for switching between different screens(states).<br>
<img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/7507b428-967a-4ead-9a14-ce5219d2d5f2"  width="400" height="350">
1. **State To Skip** - Allows skipping of states.
2. **States** - Specify selection screen or states.
3. **Starting State** - Set the starting state. If user is logged in, starting state defaults to Avatar Selection screen.
4. **Default Body Type** - Set default body type if body type selection state is skipped.
5. **Default Gender** -  Set default body type if gender selection state is skipped.

## Selection Screens(States)
1. **Login With Email Selection** - Login by sending a one time code to user's email address.
2. **Avatar Selection** - Select/edit the previously created avatar from the list of avatars.
3. **Gender Selection** - Select gender of the avatar.
4. **BodyType Selection** - Select body type of the avatar - full-body or half-body.
5. **Default Avatar Selection** - Select the default avatar from the list of avatars.
6. **Selfie Selection** - Landing page for taking a selfie and use it to generate the avatar.
7. **Camera Photo Selection** - Opens webcam and allows to take a photo.
8. **Avatar Creator Selection** - Landing page for avatar customization.

## Header
1. **Back Button** - To go back to the previous screen.
2. **Next Button** - To save the final avatar.

## Loading Manager
Show different type of loading
1. **FullScreen Loading** - Toggle a full screen loading.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/0177a63a-8bee-4890-b605-0834642bd0c9" width="600" height="350">
2. **Popup Loading** - Toggle a popup with or without loading.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/6085868a-ed7a-4e83-a888-cde99edcd7ed" width="600" height="320">


## Account Creation Popup
Popup for when next button is pressed after avatar is finalized in Avatar Creator Selection.
If user is signed in this popup will not be shown.<br>
<img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/07b5b2d5-fd7c-4020-ab6e-448efabbdaba" width="600" height="350">


## Avatar Creator Selection Components

### Panels

1. **AssetTypes Panel** - Contains buttons for different asset-type - face, hairstyle, outfit, glasses, face mask, face wear and head wear.
   On click the buttons will open the respective asset-type panel.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/f9a77994-5069-427f-b2d9-64ae4d6edaf9"  width="100" height="300">

2. **AssetType Panel Prefab** - Contains asset buttons for the selected asset type.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/ae7713b3-2958-4d54-a433-570b5505372c"  width="100" height="300">

3. **FaceType Panel Prefab** - Contains asset type panels for face - face shape, eyes, eyebrows, nose, mouth, beard, etc.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/ef2fe745-02aa-4647-8274-dd84c83b076b" width="100" height="300">

4. **Left AssetType Panel Prefab** - Eye color, Skin color, Hair color, etc.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/56baf347-8053-4ce0-852a-cd2aeaf1d5b6"  width="100" height="250">
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/55a360d2-15e2-43c1-b92a-c8dfb2becf0e"  width="100" height="250">

### Buttons

1. **AssetType Button Prefab** - For selecting an asset type. The thumbnail image are present on path `Samples/Icons/AssetType`.<br>
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/87d1cc45-c5c5-4044-a26f-0b9ee73c0596"  width="400" height="250">

2. **Asset Button Prefab** - For selecting an asset with thumbnail and a selection circle. The thumbnail image is fetched from the server.

3. **Clear asset selection button** - For clearing the selected asset.
   <img src="https://github.com/readyplayerme/rpm-unity-sdk-avatar-creator/assets/1121080/24abe1b2-60f5-482c-9d1a-60ca9c8e622c"  width="50" height="50">

