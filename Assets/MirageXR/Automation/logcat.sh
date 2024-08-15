#!/usr/bin/env bash

/Applications/Unity/Hub/Editor/2022.3.7f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb logcat -s Unity ActivityManager PackageManager dalvikvm DEBUG >> ../../../Temp/quest3.log &
tail -f  ../../../Temp/quest3.log


