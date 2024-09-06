# Contributing: Agile Workflow in MirageXR

## Set up git to access this GitHub Instance

Create an SSH key according to [GitHub's instructions](https://docs.github.com/en/authentication/connecting-to-github-with-ssh/generating-a-new-ssh-key-and-adding-it-to-the-ssh-agent) and add it to your GitHub account. After that, you can pull from the repository and push changes using Git.

### Git client

You can use any Git client you want. Recommended options are the Git bash and Visual Studio's Team Explorer, however other options are also valid. In the Git bash, the relevant commands are just the standard commands that are most often used: add, commit, push, pull, checkout, merge

## Agile workflow on GitHub

The general workflow is well described in the following book chapter: <https://codereality.net/ar-for-eu-book/chapter/development/projectGuide/agileProjectManagement/>

Here is the summary of the workflow that we are using:

### Issues

Tasks are created as issues in the Gitlab repository. Every intent to change code in the repository (e.g. to add features) starts by filing an issue for it. Do not start implementing something if there is no issue that tracks the task. To make things transparent, it is a good idea to communicate on a topic by commenting on the issue. If you want to develop an issue, you need to assign yourself to the issue so that others see that you currently work on it and to avoid that two people work on the same problem. There are also the two tags "Todo" and "In Progress" which can be added to the issues to indicate their status. In case you you get stuck on a task and do not want to continue on it, un-assign yourself again and leave a note as a comment that describes where to find your implementation, what its current state is and which insights you have gained while working on the issue. This will help other developers to pick up the work where you left it. However, if possible, try to finish tasks before starting new ones.

### Branches

After assigning yourself to the issue to develop the feature, you need to create a branch which is based on the develop branch. How you do this is up to you - you can create it using your Git client but there is also a button on each issue that can be used to generate the branch for you. The important part during the creation of the branch is that it should be based on the **develop** branch and not the master branch. Branches which introduce new features should start with the username of who implements it (e.g., "fwild") to group them. To indicate to which issue a branch belongs, you can start the name of the branch after the prefix with the issue's ID, followed by a short name of the issue. For instance, an issue with the ID 42 which states that a login solution should be implemented can be implemented on a branch "fwild/42-login-solution".

### Submodules

lib-lee runs as an embedded package (using a git submodule). Intialization after fresh cloning is required (`git submodule init; git submodule update`). When committing changes involving lib-lee, make sure you first merge the lib-lee pull request for the changes onto the lib-lee develop branch, switch the branch to develop, and then commit, push, and merge the changes in the main project.

### Commit messages

Provide clear commit messages which state exactly what you changed and why you changed it. This will speed up the review process and help developers understand the history of the project.

### Test code before posting a pull request

Code should be tested by the developer before posting a merge request. The absolut minimum are manual tests in the editor where you use the input simulation of the MRTK to navigate in the application. Preferrably, the feature should also be tested on the target device, too, before creating the merge request. For new features or new code, it would be awesome, if you could already design them with unit tests in mind and create unit tests for your changes. Unit tests speed up the review process. Try to create unit tests during your implementation of a feature since it is difficult to add them to existing code at a later stage.

TIP: Whenever we commit a pull request (PR) onto develop or master, a GitHub action workflow will execute all edit mode and play mode tests automatically, and list the results as part of the actions output (check the windows workflow or the linux workflow in [this list](https://github.com/WEKIT-ECS/MIRAGE-XR/actions/). This can also be called manually, by selecting the desired workflow, and then clicking `run workflow` to run it on the desired branch.

TIP: If the Android build pipeline fails with a signing error (```UnityException: Can not sign the application```), just untick the "custom keystore" checkbox in player publishing settings, and the apk will be signed with an ad hoc development key!

### Create pull requests

Once you completed the implementation of an issue, create a pull request (aka 'merge request') that asks to merge your feature branch into the **develop** branch again. You can also reference the number of the original issue - it will create a link between the issue and the merge request which helps with transparency. When you create the merge request, the CI pipeline will check your work to make sure it compiles. We generally should only accept merge requests where the CI pipeline works.

We are generating the [Changelog.md](https://github.com/WEKIT-ECS/MIRAGE-XR/blob/develop/CHANGELOG.md) for each release from the pull request messages and issues closed over the period, so it helps if you match the style of what we write in the change log in the pull request message!

### Resolve merge conflicts

Another thing that you should take care of when creating a merge request are merge conflicts. If the merge request shows that it cannot be merged because of conflicts, resolve them by merging manually **from the develop branch into your feature branch** using your Git client. To do this, first go to the develop branch and pull all changes, then go back to your feature branch and merge the develop branch into your feature branch. This way, you essentially update your feature branch to the most current state of the project. It is the job of the developer who posts a merge request to make sure that the merge conflicts are resolved. Do not leave the conflict resolution to the reviewer because this means that a person who is not familiar with the code changes has to try and resolve the conflicts.

### Review pull requests

It is advisable to let a team member review the merge request so that they can check and accept it. We use the review mechanism of GitHub for this, providing quick access to changed files and review management. Pull requests remain "In Review" while reviewers are looking at a merge request. If the reviewer is unhappy, they set it to "Changes required" and leave a comment that explains what needs to be changed. It is the job of the developer of that branch to incorporate these changes. After that, the developer resubmits the change request. To speed up the review process, you can also add a comment which describes your changes. If the reviewer is happy with the changes, the reviewer approves the pull request. Subsequently, the developer can merge the code into the target branch. It is also good practise to check the option to delete the feature branch so that we do not accumulate old branches. In the past of this project, the Scrum master has usually reviewed all merge requests.

The review process is quite important and the reviewer should make sure that they actually pay attention to the proposed changes. The job of the reviewer is to try out the implemented feature on their PC. This e.g. helps to find local configurations that only worked on the developer's PC. Moreover, the reviewer should look for bugs bugs and assess the quality of the code.


## Release a new version

The version on the master branch is the live version that is actually delivered to customers. Therefore, the application should be in its best possible state when merging into master. The entire workflow with the feature branches and merge reviews should ensure that no low-quality code or half-finished features end up on the master branch.

To create a new release, create a release branch based on the **develop** branch. This branch freezes the feature set of the application for this release. After testing the overall state of the application on the release branch and further stabilizing the code, it can be merged both into the master branch and the develop branch.

To create a formal release on the master branch, add a Git tag to the latest commit on the master branch and name it "v", followed by a version number. We use [semantic versioning](https://semver.org/), e.g. "v1.2.3", so make sure to look at the principles for incrementing the version numbers before creating a new release.

After a new release is created, if the preview builds show an incorrect version, i.e. they show the previous tag version, try merging the master branch into the develop branch. Do not change the app version in unity player settings from "$appVersion".

Tip: [Here](https://github.com/WEKIT-ECS/MIRAGE-XR/wiki/Making-a-new-Changelog.md) are some instructions on how to generate a tab separated value file (for spreadsheet import) of all closed/merged pull requests and all closed issues. This can make updating of the Changelog.md easier, especially if pull requests summarise the changes in the style we use for the change log.


## Sprints

Sprints are time frames in which a fixed set of tasks are implemented by the development team. At the beginning of a sprint, the scope of tasks are agreed upon in a meeting. Sprints have a fixed start and end date. This also means that the Scrum master moves all unfinished tasks to the next sprint at the end of a sprint and then closes the sprint.

An exception in this repository is the design sprint with the tasks for the designers and the SolAR breakout sprint. I left them as their own milestones and did not move them into sprint 4 since they are not part of the developer's day to day work.

### Add new Issues During a Sprint

In Scrum, the issues for a sprint are determined at the beginning of the sprint, so you should not dynamically add new issues to a sprint. Otherwise, this is a frustrating experience for the developers who cannot work through the tasks of a sprint with the prospect of being 100% done with the sprint's work at some point. It would be like emptying a bowl of water with a spoon while at the other end someone is constantly pouring in more water. Instead, new issues are added to the overall pool of issues (backlog) and can be added to the next sprint once it starts. An exception are really urgent or important tasks which can also be added to the current sprint.

### Prioritise Issues

The priority of issues should be made clear in the sprint kickoff meeting. The Scrum master can also communicate the priority of issues by assigning the existing tags for high, medium or low priority to issues.


## Communication

The main communication on tasks should happen on the corresponding issues by posting comments. The comments there are persistent and are even archived after the issue is closed. So, a developer who joins the project in two years can still search for specific features in the issues list and read up on the conversations and design decisions for the feature. Bear in mind, that by default nobody will receive notifications on an issue unless they actively follow it (which is the case once they have in some way interacted with the issue, e.g. by turning on notifications for it or commenting on it). If you want to notify someone about the issue, you can reference them in the comment of the issue using the @ marker. This should also send them a notification.

For organisational questions, e.g. agreeing on meeting times, etc. we use [Slack](https://mirage-community.slack.com/). You should not discuss design decisions and questions about issues on Slack since you cannot connect the conversation to the issue and messages can vanish after some time.
