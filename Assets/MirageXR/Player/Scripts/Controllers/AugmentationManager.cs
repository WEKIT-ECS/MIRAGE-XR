using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MirageXR
{
	public class AugmentationManager
	{
		private const string AUGMENTATION_FORMAT = "AN-{0}";
		private static ActivityManager activityManager => RootObject.Instance.activityManager;

		public ToggleObject AddAugmentation(Action action, Vector3 position)
		{
			var annotation = new ToggleObject
			{
				id = action.id,
				poi = string.Format(AUGMENTATION_FORMAT, Guid.NewGuid()),
				type = ActionType.Tangible,
				scale = 1
			};

			int currentActionIndex = 0;
			for (var i = 0; i < activityManager.ActionsOfTypeAction.Count; i++)
			{
				if (activityManager.ActionsOfTypeAction[i].id == activityManager.ActiveActionId)
				{
					currentActionIndex = i;
					break;
				}
			}

			AddAllAugmentationsBetweenSteps(currentActionIndex, currentActionIndex, annotation, position);

			return annotation;
		}
		
		public void AddAllAugmentationsBetweenSteps(int startIndex, int endIndex, ToggleObject annotation, Vector3 position)
		{
			var actionList = activityManager.ActionsOfTypeAction;
			//remove the annotation from out of the selected steps range

			for (int i = 0; i < actionList.Count; i++)
			{
				if (i >= startIndex && i <= endIndex)
				{
					continue;
				}

				var action = actionList[i];
				foreach (var anno in action.enter.activates.FindAll(p => p.poi == annotation.poi))
				{
					DeleteAugmentationFromStep(actionList[i], anno);
				}

				if (action == activityManager.ActiveAction)
				{
					EventManager.DeactivateObject(annotation);
				}
			}

			//add the annotation to the selected steps range if already not exist
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (actionList[i].enter.activates.All(p => p.poi != annotation.poi))
				{
					actionList[i].enter.activates.Add(annotation);
				}

				if (actionList[i].exit.deactivates.All(p => p.poi != annotation.poi))// also create an exit object (w/out activate options)
				{
					actionList[i].exit.deactivates.Add(annotation);
				}
			}

			//the objects of the annotation will be created only for the original step not all steps.
			RootObject.Instance.workplaceManager.AddAnnotation(activityManager.ActiveAction, annotation, position);
		}
		
		public void DeleteAugmentation(ToggleObject annotation, Action step = null)
		{
			var poi = annotation.poi;

			//close all editor before deleting an annotation, some pois like audio throw exception if you delete the file but the editor is open
			Object.FindObjectOfType<ActionEditor>().DisableAllPoiEditors();

			//delete the annotation form all steps which include the annotation
			//annotation does not exist in other step, then delete it and it's files from anywhere
			if (step == null)
			{
				foreach (var actionObj in activityManager.ActionsOfTypeAction)
				{
					//remove this from triggers
					var trigger = actionObj.triggers.Find(t => t.id == poi);
					actionObj.triggers.Remove(trigger);

					//remove this from activates
					foreach (var anno in actionObj.enter.activates.FindAll(p => p.poi == poi))
					{
						DeleteAugmentationFromStep(actionObj, anno);
						DeleteAugmentationFile(anno);
						RootObject.Instance.workplaceManager.DeleteAugmentation(actionObj, anno);
					}
	
					EventManager.NotifyActionModified(actionObj);
					activityManager.SaveData();//save data(annotations) after deleting additional files like character or models data
					EventManager.NotifyAugmentationDeleted(annotation);//All augmentations classes should do whatever should be done on augmentation deletion
				}
			}
			else
			{
				foreach (var anno in step.enter.activates.FindAll(p => p.poi == poi))
				{
					DeleteAugmentationFromStep(step, anno);
				}
				EventManager.NotifyActionModified(step);

				//save data(annotations) after deleting additional files like character or models data
				activityManager.SaveData();
			}
		}

		public void DeleteAugmentationFromStep(Action action, ToggleObject annotation)
		{
			action.enter.activates.Remove(annotation);
			action.exit.deactivates.Remove(annotation);
		}

		private static void DeleteAugmentationFile(ToggleObject annotation)
		{
			const string httpPrefix = "http://";
			
			var filePath = annotation.url;
			if (filePath.StartsWith(httpPrefix))
			{
				filePath = filePath.Remove(0, httpPrefix.Length);
			}
			var localFilePath = Path.Combine(activityManager.ActivityPath, Path.GetFileName(filePath));
			if (File.Exists(localFilePath))
			{
				File.Delete(localFilePath);
			}
		}
	}
}
