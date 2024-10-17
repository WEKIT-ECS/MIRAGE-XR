using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabSwitcher : MonoBehaviour
{
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private List<GameObject> tabs;

    private void Start()
    {
        if (toggles.Count != tabs.Count)
        {
            return;
        }
        
        for (var i = 0; i < toggles.Count; i++)
        {
            int index = i;
            toggles[i].onValueChanged.AddListener(delegate { SwitchTab(index); });
        }
        
        SwitchTab(0); // Start tab
    }

    private void SwitchTab(int tabIndex)
    {
        foreach (var t in tabs)
        {
            t.SetActive(false);
        }
        tabs[tabIndex].SetActive(true);
    }
}
