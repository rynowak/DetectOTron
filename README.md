# SignalR Detect-o-tron

This proof of concept detects the usage of SignalR in the source code of a project and sets of project capability based on usage.

![image](https://i.imgur.com/kLLbKn1.gif)

## FAQ.

**Why?** We want to be able to drive VS features based on usage of technologies in code. We have an immediate need to detect SignalR since it has special publishing requirements.

**Why not put something in the project file?** We want to make this as discoverable and easy for users as possible. Requiring users to go to the docs or use a specific project template is the opposite of what we're trying to achieve.

**Why a project capability?** They are well understood and ubiquitious. But since you asked, there's no reason this *has to* be a project capability. This already has a UI, and it seems like a good place to start.

## What's not great about this...

The detection of SignalR here is a a little simplistic. It doesn't handle the case of multiple startup classes, and assumes you have just one. This is also based on the *old way* of doing SignalR, because I haven't added support for the new style in the analyzer.

The barrier between an analyzer and the project system is pretty rough to work around.

The analyzer doesn't analyze until *do something* to edit the startup class. We'll have to solve this for sure.