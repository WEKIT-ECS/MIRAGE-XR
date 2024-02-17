using i5.Toolkit.Core.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MirageXR
{
    public class ListView<T>
    {
        protected Transform listViewTarget;
        protected GameObject itemPrefab;

        protected List<ListViewItem<T>> itemEntries;

        public List<T> Items
        {
            get; set;
        }

        public ListView(Transform listViewTarget, GameObject itemPrefab)
        {
            this.listViewTarget = listViewTarget;
            this.itemPrefab = itemPrefab;
        }

        public virtual void UpdateView()
        {
            if (Items == null)
            {
                return;
            }

            if (itemEntries == null)
            {
                itemEntries = new List<ListViewItem<T>>(Items.Count);
            }

            for (int i = 0; i < Items.Count; i++)
            {
                // if there is already an entry: use it to show the content
                if (i < itemEntries.Count)
                {
                    itemEntries[i].gameObject.SetActive(true);
                    itemEntries[i].ShowData(Items[i], i);
                }
                // if there is not entry: create one
                else
                {
                    GameObject entryInstance = GameObject.Instantiate(itemPrefab, listViewTarget);
                    ListViewItem<T> itemEntry = entryInstance.GetComponent<ListViewItem<T>>();
                    if (itemEntry == null)
                    {
                        i5Debug.LogError("The chosen prefab does not contain a component that inherits from ListViewItem", this);
                        GameObject.Destroy(entryInstance);
                        return;
                    }
                    itemEntries.Add(itemEntry);
                    itemEntry.ShowData(Items[i], i);
                }

            }

            // deactivate all unused item entries
            for (int i = Items.Count; i < itemEntries.Count; i++)
            {
                itemEntries[i].gameObject.SetActive(false);
            }
        }
    }

}
