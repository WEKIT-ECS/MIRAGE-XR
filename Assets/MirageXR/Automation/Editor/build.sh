#!/usr/bin/env sh
set -e

/Applications/Unity/Hub/Editor/6000.0.59f2/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile - \
	-projectPath "$(pwd)" \
	-quit \
        -executeMethod BuildCommand.Build	
# -executeMethod LocalBuildPipeline.BuildQuest

echo "done."
