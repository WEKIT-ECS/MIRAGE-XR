using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPropManager : MonoBehaviour
{
    [System.Serializable]
    public class PropEvent
    {
        public string name;
        public Vector3 localPosition = Vector3.zero;
        public Vector3 localEulerRotation = Vector3.zero;
        public Vector3 localScale = Vector3.zero;
    }

    [System.Serializable]
    public class PropItem
    {
        public Transform item;
        public PropEvent[] propEvents;
    }

    [SerializeField]
    private PropItem[] propItems;
    private bool eventFound;

    public void SelectProp(string val)
    {
        if ( propItems == null || propItems.Length == 0 ) return;
        
        eventFound = false;

        foreach ( PropItem propItem in propItems)
        {
            if ( propItem.item == null ) continue;

            if ( eventFound ) propItem.item.gameObject.SetActive(false);
            else
            {
                foreach(PropEvent propEvent in propItem.propEvents)
                {
                    if (propEvent.name == val)
                    {
                        propItem.item.localPosition = propEvent.localPosition;
                        propItem.item.localEulerAngles = propEvent.localEulerRotation;
                        propItem.item.localScale = propEvent.localScale;
                        eventFound = true;
                        break;
                    }
                }

                propItem.item.gameObject.SetActive(eventFound);
            }
        }
    }
}
