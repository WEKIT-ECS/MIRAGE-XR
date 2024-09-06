#!/usr/bin/env sh
set -e

/Applications/Unity/Hub/Editor/2022.3.7f1/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-nographics \
	-silent-crashes \
	-logFile - \
	-projectPath "$(pwd)" \
	-quit \
	-executeMethod LocalBuildPipeline.BuildQuest

echo "done."
