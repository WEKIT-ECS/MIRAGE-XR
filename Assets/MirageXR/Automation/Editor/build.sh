#!/usr/bin/env sh
set -e

/Applications/Unity/Hub/Editor/6000.0.59f2/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile - \
	-projectPath "$(pwd)" \
	-quit \
        -buildTarget VisionOS \
        -build Builds/visionos26 \
        -customBuildPath Builds/visionos26 \
        -executeMethod BuildCommand.Build	
# -executeMethod LocalBuildPipeline.BuildQuest

echo "done."
