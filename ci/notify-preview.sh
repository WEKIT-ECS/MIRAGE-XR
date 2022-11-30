#!/usr/bin/env bash

echo "Posting notification"

curl -X POST \
  $SLACK_URL \
  -d "{
	\"channel\": \"#preview-releases\",
	\"username\": \"Release Bot\",
	\"text\": \"A new preview version is available for $TARGET_PLATFORM!\nVersion: *$APP_VERSION*\",
	\"icon_emoji\": \":rocket:\",
	\"attachments\": [
		{
			\"color\": \"#00549F\",
			\"blocks\": [
				{
					\"type\": \"section\",
					\"text\": {
						\"type\": \"mrkdwn\",
						\"text\": \"<https://github.com/WEKIT-ECS/MIRAGE-XR/actions/runs/$GITHUB_RUN_ID| View workflow to download build :package:>\"
					}
				},
				{
					\"type\": \"section\",
					\"text\": {
						\"type\": \"mrkdwn\",
						\"text\": \"Commit Message: $COMMIT_MESSAGE\"
					}
				}
			]
		}
	]
}"
