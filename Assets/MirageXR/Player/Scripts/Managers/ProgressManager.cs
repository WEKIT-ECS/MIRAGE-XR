
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class ProgressManager : MonoBehaviour
    {
        [SerializeField] private Button progressLine;
        [SerializeField] private GameObject TaskList;
        [SerializeField] private Text stepStatusLabel;

        private void Update()
        {
            UpdateProgressLine();
        }

        private void UpdateProgressLine()
        {
            var tasks = TaskList.transform.GetComponentsInChildren<TaskStep>();

            if (tasks.Length == 0) 
                return;

            var counter = 0;

            //find out the steps
            foreach (TaskStep task in tasks)
            {
                if (task.Get_actionObject().isCompleted)
                    counter++;
            }

            stepStatusLabel.text = "Step " + counter.ToString()  + " of " + tasks.Length.ToString() + " is done!";

            //adjust the progress line if the task list is not empty
            progressLine.GetComponent<RectTransform>().localScale = new Vector3( (1f / tasks.Length) * counter, 1f , 1f);
            print(counter);
        }
    }

}