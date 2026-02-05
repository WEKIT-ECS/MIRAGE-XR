using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class TabManager : MonoBehaviour
    {
        [SerializeField] private TabPair[] tabs;

        private void Start()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                int index = i;

                tabs[i].tabToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        OpenTab(index);
                    }
                });
            }

            OpenTab(0);
        }

        public void OpenTab(int index)
        {
            index = Mathf.Clamp(index, 0, tabs.Length - 1);
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].contentPage.SetActive(i == index);
            }
        }
    }

    [Serializable]
    public class TabPair
    {
        public string tabName;
        public Toggle tabToggle;
        public GameObject contentPage;
    }
}
