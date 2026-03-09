#!/usr/bin/env sh
set -e

/Applications/Unity/Hub/Editor/6000.0.69f1/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile - \
	-projectPath "$(pwd)" \
	-quit \
        -buildTarget VisionOS \
        -build Builds/visionos27 \
        -customBuildPath Builds/visionos27 \
        -executeMethod BuildCommand.Build	
# -executeMethod LocalBuildPipeline.BuildQuest

echo "done."
