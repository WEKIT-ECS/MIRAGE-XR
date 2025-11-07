# MirageXR

Slipping on a pair of smart glasses can give you superpowers in the workplace, 
superimposing procedural guides and operational data directly where you need it. 
Extended Reality (XR) can teach you in minutes all you need to know for complex,
knowledge-intensive tasks, such as how to service the nose-wheel of an airplane, 
how to operate an ultrasound machine to obtain a healthcare diagnosis, or 
how to check and service a self-driving vehicle.

The MirageXR SOLUTIONS are the flagship products of WEKIT ECS, developed in response 
to real market needs, in consultation with industry and relevant communities, 
and based on state of the art research. MirageXR SOLUTIONS offer an all-in-one 
XR framework that is flexible and customisable for a broad range of industry 
and sector needs. 

The MirageXR COMMUNITY EDITION (CE) is the is the B2B Open Source solution of 
the MirageXR platform. This reference implementation of an XR training system 
for complex work environments is offered ‘as is’ for the XR developer community 
to use, update, and further develop.

The MirageXR SOLUTIONS enable experts and learners to share experience via XR 
and wearables using ghost tracks, in-situ feedback, and anchored instruction. 
They offers six unique value adding features that are hard to emulate:

* In-situ authoring and experience capture - reducing the time and resourcing 
for the production of training content. MirageXR offers a tool for 
creating content directly in XR, cheap and efficiently. It allows both the 
placing of previously imported media content into physical spaces (and into 
step-by-step guides) and the ability to create content while performing 
tasks (e.g., ghost track).

* Experiential approach - MirageXR implements an experiential learning approach. 
Trainees can follow a sequence of performing a task (guided by a holographic 
expert), reviewing their own performance, and performing the same or a similar 
task again.

* Real-time visualisation and feedback - holographic representation of the expert 
reconstructed from the captured data (‘ghost track’) and embedded in the physical 
workplace, including body position, gaze direction, gestures, and voice.

* Open standards and architecture - The MirageXR core is adaptable to different 
platforms, IoT sensors, and wearables to suit the training needs of any 
organisation. It implements the IEEE P1589-2020 standard for augmented reality 
learning experience models, which allows for the automated extraction and 
transformation of content from legacy systems (such as, e.g., an Oasis DITA 
or S1000D-compliant technical documentation system) - compatible with an ecosystem 
of tools, future-proofing your content, to the highest, approved standard.

* Cognitive methods in performance measurement - MirageXR supports established 
Cognitive Science research methodologies such as cognitive task analysis to 
predict or identify errors or suggest improved or corrective practice. After 
a training session, performance analysis can be prepared from the measured data,
stored via the interface with the IEEE Experience API (xAPI).

* Scientifically validated method - the MirageXR Open Source core is based on 
project results of the Horizon 2020 project WEKIT (grant agreement No 687669),
validated in two waves with over 550 participants in pilot trials in medicine,
aviation, and space. The contributing partners from the WEKIT consortium were
Oxford Brookes University, Open University of the Netherlands, and VTT Technical
Research Centre of Finland. 

MirageXR provides an enhanced, location-based experience for learners with the promise 
to get access to expert knowledge faster, better, and more engaging than ever before. 
Better engagement means more effective education and training which translates 
real social and economic benefits to the market.

MirageXR has received funding from the European Union’s Horizon 2020 research 
and innovation programme through the XR4ALL project with the grant agreement 
No 825545, through the OpenReal project funded by The Open University of the UK, 
and through the ARETE project with the grant agreement No 856533.

The Alien character model is licensed under a [Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License](http://creativecommons.org/licenses/by-nc-sa/4.0/). Any reuse of this model must acknowledge the work of the ARETE team at [Consiglio Nazionale delle Ricerche - Istituto per le Tecnologie Didattiche, Palermo Italy](https://www.itd.cnr.it/).

IBM Cloud Unity SDK Core (Assets/IBMWatsonSDK/IBMSdkCore) and IBM Cloud Unity SDK Core (Assets/IBMWatsonSDK/Watson) are licensed under Apache 2.0. The full license text is available in /Assets/IBMWatsonSDK/Watson/LICENSE and /Assets/IBMWatsonSDK/IBMSdkCore/README.md

## Download Releases

Main Build status for HoloLens builds: ![example workflow](https://github.com/WEKIT-ECS/MIRAGE-XR/actions/workflows/windows_workflow.yml/badge.svg?branch=master)

Main Build status for Android builds: ![example workflow](https://github.com/WEKIT-ECS/MIRAGE-XR/actions/workflows/linux_workflow.yml/badge.svg?branch=master)

You can find the releases in the [Releases tab](https://github.com/WEKIT-ECS/MIRAGE-XR/releases).
They contain changelogs and installer files for the HoloLens 1 & 2 devices.

## Download Preview Builds

Preview builds reflect the current state of the development and may be unstable.
Use these installers to test new features before they are available as a regular release.
Preview releases are for testing purposes only and should not be used in production.

Preview Build status for HoloLens builds: ![example workflow](https://github.com/WEKIT-ECS/MIRAGE-XR/actions/workflows/windows_develop_workflow.yml/badge.svg?branch=develop)

Preview Build status for Android builds: ![example workflow](https://github.com/WEKIT-ECS/MIRAGE-XR/actions/workflows/linux_develop_workflow.yml/badge.svg?branch=develop)

![monitored by sentry](https://img.shields.io/badge/monitored%20by-sentry-purple)

## Getting started

1. Install [Unity Hub](https://unity.com/download)
2. This repository contains several large files, so you need to install [git lfs](https://git-lfs.com/), e.g., `brew install git-lfs`
4. Clone the project from here in your favourite way, e.g., `git clone git@github.com:WEKIT-ECS/MIRAGE-XR.git`
5. Place the API keys as described in [compiling MirageXR](https://github.com/WEKIT-ECS/MIRAGE-XR/wiki/Compiling-MirageXR)
6. Add the project to Unity Hub, open it, and install the editor version prompted for.
