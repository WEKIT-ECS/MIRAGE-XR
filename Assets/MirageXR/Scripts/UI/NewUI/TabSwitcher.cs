using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabSwitcher : MonoBehaviour
{
    [SerializeField] private List<Toggle> toggles;
    [SerializeField] private List<GameObject> tabs;

    private int _currentTabIndex = -1;
    private bool _initialized;
    private bool _isSwitching;

    public int CurrentTabIndex => _currentTabIndex;

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
        if (_currentTabIndex == -1)
        {
            SwitchTab(0); // Default to first tab if not already switched
        }
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        if (toggles == null || tabs == null || toggles.Count != tabs.Count)
        {
            return;
        }

        _initialized = true;

        for (var i = 0; i < toggles.Count; i++)
        {
            var index = i;
            if (toggles[i] != null)
            {
                toggles[i].onValueChanged.AddListener((isOn) =>
                {
                    if (isOn && !_isSwitching)
                    {
                        SwitchTab(index);
                    }
                });
            }
        }
    }

    public void SwitchTab(int tabIndex)
    {
        Initialize();

        if (tabIndex < 0 || tabs == null || tabIndex >= tabs.Count)
        {
            return;
        }

        if (_isSwitching)
        {
            return;
        }

        _isSwitching = true;
        _currentTabIndex = tabIndex;

        try
        {
            // Update toggles with visual notification
            if (toggles != null)
            {
                // Turn on target toggle first
                if (tabIndex < toggles.Count && toggles[tabIndex] != null)
                {
                    if (!toggles[tabIndex].isOn)
                    {
                        toggles[tabIndex].isOn = true;
                    }
                    else
                    {
                        toggles[tabIndex].onValueChanged?.Invoke(true);
                    }
                }

                // Turn off other toggles and update their visuals
                for (var i = 0; i < toggles.Count; i++)
                {
                    if (i != tabIndex && toggles[i] != null)
                    {
                        if (toggles[i].isOn)
                        {
                            toggles[i].isOn = false;
                        }
                        else
                        {
                            toggles[i].onValueChanged?.Invoke(false);
                        }
                    }
                }
            }

            // Update panels
            if (tabs != null)
            {
                for (var i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i] != null)
                    {
                        tabs[i].SetActive(i == tabIndex);
                    }
                }
            }
        }
        finally
        {
            _isSwitching = false;
        }
    }
}
