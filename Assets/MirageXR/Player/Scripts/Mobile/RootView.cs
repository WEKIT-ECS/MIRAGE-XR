using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class RootView : MonoBehaviour
{
    [SerializeField] private Toggle _toggleHome;
    [SerializeField] private Toggle _toggleView;
    [SerializeField] private Toggle _toggleSteps;
    [SerializeField] private PageView _pageView;
    [SerializeField] private CalibrationGuideView _calibrationGuideViewPrefab;
    
    
    private void Start()
    {
        EventManager.OnWorkplaceParsed += OnWorkplaceParsed;
        _toggleView.interactable = false;
        _toggleSteps.interactable = false;
        _toggleHome.onValueChanged.AddListener(OnStepsClick);
        _toggleView.onValueChanged.AddListener(OnViewClick);
        _toggleSteps.onValueChanged.AddListener(OnHomeClick);
        _pageView.OnPageChanged.AddListener(OnPageChanged);
    }

    private void OnDestroy()
    {
        EventManager.OnWorkplaceParsed -= OnWorkplaceParsed;
    }

    private void OnWorkplaceParsed()
    {
        _toggleView.interactable = true;
        _toggleSteps.interactable = true;
        _toggleView.isOn = true;
        
        if (!DBManager.dontShowCalibrationGuide)
        {
            PopupsViewer.Instance.Show(_calibrationGuideViewPrefab);
        }
    }

    private void OnPageChanged(int index)
    {
        switch (index)
        {
            case 0: 
                _toggleHome.isOn = true;
                break;
            case 1:
                _toggleView.isOn = true;
                break;
            case 2: 
                _toggleSteps.isOn = true;
                break;
        }
    }

    private void OnStepsClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 0;
    }

    private void OnViewClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 1;
    }

    private void OnHomeClick(bool value)
    {
        if (value) _pageView.currentPageIndex = 2;
    }
}
