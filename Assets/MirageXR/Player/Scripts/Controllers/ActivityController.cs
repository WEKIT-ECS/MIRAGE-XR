using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace MirageXR
{
    public class ActivityController : MonoBehaviour
    {
        private static ActivityController instance;
        public static ActivityController Instance => instance;

        private List<Action> ActionsOfTypeAction
        {
            get { return ActivityManager.Instance.ActionsOfTypeAction; }
        }


        private string Path
        {
            get { return ActivityManager.Instance.Path; }
        }

        private void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (this != instance)
            {
                Destroy(gameObject);
            }
        }

        public void DeleteAugmentation(ToggleObject annotation, Action step = null)
        {
            var poi = annotation.poi;

            //close all editor before deleting an annotation, some pois like audio throw exception if you delete the file but the editor is open
            FindObjectOfType<ActionEditor>().DisableAllPoiEditors();

            //delete the annotation form all steps which include the annotation

            //annotation does not exist in other step, then delete it and it's files from anywhere
            if (step == null)
            {
                foreach (var actionObj in ActionsOfTypeAction)
                {

                    //remove this from triggers
                    var trigger = actionObj.triggers.Find(t => t.id == poi);
                    actionObj.triggers.Remove(trigger);

                    //remove this from activates
                    foreach (var anno in actionObj.enter.activates.FindAll(p => p.poi == poi))
                    {
                        DeleteAugmentationFromStep(actionObj, anno);
                        DeleteAugmentationFile(anno);
                        WorkplaceManager.Instance.DeleteAugmentation(actionObj, anno);

                    }

                    EventManager.NotifyActionModified(actionObj);

                    //save data(annotations) after deleting additional files like character or models data
                    ActivityManager.Instance.SaveData();

                    //All augmentations classes should do whatever should be done on augmentation deletion
                    EventManager.NotifyAugmentationDeleted(annotation);
                }
            }
            else
            {
                foreach (var anno in step.enter.activates.FindAll(p => p.poi == poi))
                {
                    DeleteAugmentationFromStep(step, anno);
                    if (step == null)
                    {
                        DeleteAugmentationFile(anno);
                        WorkplaceManager.Instance.DeleteAugmentation(step, anno);
                    }

                }
                EventManager.NotifyActionModified(step);

                //save data(annotations) after deleting additional files like character or models data
                ActivityManager.Instance.SaveData();
            }

        }

        public void DeleteAugmentationFromStep(Action action, ToggleObject annotation)
        {
            action.enter.activates.Remove(annotation);
            action.exit.deactivates.Remove(annotation);
        }


        private void DeleteAugmentationFile(ToggleObject annotation)
        {
            // clean up associated file
            string filePath = annotation.url;
            const string httpPrefix = "http://";
            if (filePath.StartsWith(httpPrefix))
            {
                filePath = filePath.Remove(0, httpPrefix.Length);
            }
            string localFilePath = System.IO.Path.Combine(Path, System.IO.Path.GetFileName(filePath));
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }
    }

}
