ExperienceManager
=================

The ExperienceManager resides under /Assets/MirageXR/Common/ExperienceAPI and consists three parts: the ExperienceService, the ExperienceAPIService, and the RemoteUnityLRS.

The ExperienceAPIService is a wrapper for the TinCan RemoteUnityLRS, encapsulating its complexity to make sending xAPI statements as easy as possible (= one line of code).

It makes dropping a statement to an xAPI learning record store (aka 'end point') as easy as pie:

```
ServiceManager.GetService<ExperienceAPIService>().SendMessage(
	userName, 
	"http://adlnet.gov/expapi/verbs/launched", 
	"MirageXR"
);
```

The ExperienceService binds the ExperienceAPIService to the MirageXR applications, setting callbacks to the EventManager for all key system events (activity loaded, start activity, activate augmentation, start action step, finish action step, activity completed, possibly more in the future).

