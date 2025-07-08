# Ready Player Me Unity SDK Core

[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/readyplayerme/rpm-unity-sdk-core)](https://github.com/readyplayerme/rpm-unity-sdk-core/releases/latest) [![GitHub Discussions](https://img.shields.io/github/discussions/readyplayerme/rpm-unity-sdk-core)](https://github.com/readyplayerme/rpm-unity-sdk-core/discussions) [![Run Integration Tests](https://github.com/readyplayerme/rpm-unity-sdk-core/actions/workflows/integration-test.yml/badge.svg)](https://github.com/readyplayerme/rpm-unity-sdk-core/actions/workflows/integration-test.yml)

This is an open source Unity plugin that contains all the core functionality required for a Ready Player Me avatar integration, and manages all the plugin updates and dependencies including Ready Player Me Avatar Loader and glTFast. 

Please visit the online documentation and join our public `forums` community.

![](https://i.imgur.com/zGamwPM.png) **[Online Documentation]( https://docs.readyplayer.me/ready-player-me/integration-guides/unity )**

![](https://github.com/readyplayerme/rpm-unity-sdk-webview/assets/25016626/130b50db-d6af-4277-9da3-03172bc085eb) **[Forums](https://forum.readyplayer.me/)**

:octocat: **[GitHub Discussions]( https://github.com/readyplayerme/rpm-unity-sdk-core/discussions )**

## Quick Start
The installation steps can be found [here.](Documentation~/QuickStart.md)

## Avatar Creator

### Supported Platforms
- Windows/Mac/Linux Standalone
- Android*
- iOS*

### Sample
Steps for trying out avatar creator sample can be found [here.](Documentation~/AvatarCreatorSample.md).

### Structure
- It provides APIs for creating, customizing and loading the avatar.
- It also contains a sample which demonstrates the usage of the APIs and replicates RPM web avatar creator.
- The documentation of provided sample can be found [here.](Documentation~/SampleStructure.md)

### Customization Guide
A guide for customizing avatar creator can be found [here.](Documentation~/CustomizationGuide.md)

### Note
- [*]Camera support is only provided for Windows and WebGL, using Unity’s webcam native API.
- Unity does not have a native file picker, so we have discontinued support for this feature.
- To add support for file picker (for selfies) you have to implement it yourself
