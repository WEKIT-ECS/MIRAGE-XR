#!/usr/bin/env bash

echo "Posting notification"

curl -X POST \
  $SLACK_URL \
  -d "{
	\"channel\": \"#releases\",
	\"username\": \"Release Bot\",
	\"text\": \"A new release is available!\nVersion: *$APP_VERSION*\",
	\"icon_emoji\": \":rocket:\",
	\"attachments\": [
		{
			\"color\": \"#00549F\",
			\"blocks\": [
				{
					\"type\": \"section\",
					\"text\": {
						\"type\": \"mrkdwn\",
						\"text\": \"<https://github.com/WEKIT-ECS/MIRAGE-XR/actions/runs/$GITHUB_RUN_ID| Release>\"
					}
				},
				{
					\"type\": \"section\",
					\"text\": {
						\"type\": \"mrkdwn\",
						\"text\": \"<$DOWNLOAD_URL| Download :package:>\"
					}
				}
			]
		}
	]
}"