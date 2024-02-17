using System;
using DG.Tweening;
using i5.Toolkit.Core.VerboseLogging;
using MirageXR;
using UnityEngine;
using UnityEngine.UI;

public class OnboardingTutorialView : PopupBase
{
    private const float FADE_TIME = 0.2f;

    [SerializeField] private PageView _pageView;
    [SerializeField] private CanvasGroup _buttonsCanvasGroup;
    [SerializeField] private Button _btnPrevious;
    [SerializeField] private Button _btnNext;
    [SerializeField] private Button _btnSkip;
    [SerializeField] private Button _btnSkipTutorials;
    [SerializeField] private Button _btnViewingTutorial;
    [SerializeField] private Button _btnEditingTutorial;

    public override void Initialization(Action<PopupBase> onClose, params object[] args)
    {
        base.Initialization(onClose, args);

        _btnPrevious.onClick.AddListener(OnPreviousButtonPressed);
        _btnNext.onClick.AddListener(OnNextButtonPressed);
        _btnSkip.onClick.AddListener(OnSkipButtonPressed);
        _btnSkipTutorials.onClick.AddListener(OnSkipButtonPressed);
        _btnViewingTutorial.onClick.AddListener(OnViewingTutorialButtonPressed);
        _btnEditingTutorial.onClick.AddListener(OnEditingTutorialButtonPressed);
        _pageView.onMoveBegin.AddListener(OnPageMoveBegin);
        _pageView.onMoveEnd.AddListener(OnPageMoveEnd);

        _btnPrevious.gameObject.SetActive(false);
        _btnNext.gameObject.SetActive(true);
        _btnSkip.gameObject.SetActive(true);
        _btnSkipTutorials.gameObject.SetActive(false);
    }

    protected override bool TryToGetArguments(params object[] args)
    {
        return true;
    }

    private void OnPreviousButtonPressed()
    {
        _pageView.MovePrevious();
    }

    private void OnNextButtonPressed()
    {
        _pageView.MoveNext();
    }

    private void OnSkipButtonPressed()
    {
        Close();
    }

    private void OnViewingTutorialButtonPressed()
    {
        Close();
        TutorialManager.Instance.StartNewMobileViewingTutorial();
    }

    private void OnEditingTutorialButtonPressed()
    {
        Close();
        TutorialManager.Instance.StartNewMobileEditingTutorial();
    }

    private async void OnPageMoveBegin(int index)
    {
        await _buttonsCanvasGroup.DOFade(0, FADE_TIME).AsyncWaitForCompletion();

        _btnPrevious.gameObject.SetActive(false);
        _btnNext.gameObject.SetActive(false);
        _btnSkip.gameObject.SetActive(false);
        _btnSkipTutorials.gameObject.SetActive(false);
    }

    private void OnPageMoveEnd(int index)
    {
        switch (index)
        {
            case 0:
                _btnPrevious.gameObject.SetActive(false);
                _btnNext.gameObject.SetActive(true);
                _btnSkip.gameObject.SetActive(true);
                _btnSkipTutorials.gameObject.SetActive(false);
                break;
            case 1:
            case 2:
            case 3:
                _btnPrevious.gameObject.SetActive(true);
                _btnNext.gameObject.SetActive(true);
                _btnSkip.gameObject.SetActive(true);
                _btnSkipTutorials.gameObject.SetActive(false);
                break;
            case 4:
                _btnPrevious.gameObject.SetActive(true);
                _btnNext.gameObject.SetActive(false);
                _btnSkip.gameObject.SetActive(false);
                _btnSkipTutorials.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("Page View: Out of range");
                break;
        }

        _buttonsCanvasGroup.DOFade(1f, FADE_TIME);
    }
}
