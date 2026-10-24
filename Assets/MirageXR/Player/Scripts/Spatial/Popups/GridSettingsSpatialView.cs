using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirageXR
{
    public class GridSettingsSpatialView : PopupBase
    {
        public static GridSettingsSpatialView Instance { get; private set; }
        public static bool IsOpen => Instance != null;

        private static GridManager gridManager => RootObject.Instance.GridManager;

        [Header("Close")]
        [SerializeField] private Button _btnClose;

        [Header("Toggles")]
        [SerializeField] private Toggle _activateGridToggle;
        [SerializeField] private Toggle _snapToGridToggle;
        [SerializeField] private Toggle _showMeshToggle;
        [SerializeField] private Toggle _showOriginalObjectToggle;
        [SerializeField] private Toggle _useObjectCenterToggle;

        [Header("Grid Step (Cell Size)")]
        [SerializeField] private TMP_Text _txtGridStep;
        [SerializeField] private Button _btnGridStepPrev;
        [SerializeField] private Button _btnGridStepNext;

        [Header("Angle Step")]
        [SerializeField] private TMP_Text _txtAngleStep;
        [SerializeField] private Button _btnAngleStepPrev;
        [SerializeField] private Button _btnAngleStepNext;

        [Header("Scale Step")]
        [SerializeField] private TMP_Text _txtScaleStep;
        [SerializeField] private Button _btnScaleStepPrev;
        [SerializeField] private Button _btnScaleStepNext;

        private int _currentGridStepIndex = 0;
        private int _currentAngleStepIndex = 0;
        private int _currentScaleStepIndex = 0;

        protected override bool TryToGetArguments(params object[] args)
        {
            return true;
        }

        public override void Initialization(Action<PopupBase> onClose, params object[] args)
        {
            base.Initialization(onClose, args);

            if (Instance != null && Instance != this)
            {
                Close();
                return;
            }

            Instance = this;

            if (_btnClose != null)
            {
                _btnClose.onClick.AddListener(Close);
            }

            if (gridManager == null)
            {
                Debug.LogWarning("[GridSettingsSpatialView] GridManager instance is null.");
                return;
            }

            // Initialize toggles without notifying
            if (_activateGridToggle != null)
            {
                _activateGridToggle.SetIsOnWithoutNotify(gridManager.gridEnabled);
                _activateGridToggle.onValueChanged.AddListener(OnActivateGridToggleValueChanged);
            }

            if (_snapToGridToggle != null)
            {
                _snapToGridToggle.SetIsOnWithoutNotify(gridManager.snapEnabled);
                _snapToGridToggle.onValueChanged.AddListener(OnSnapToGridToggleValueChanged);
            }

            if (_showMeshToggle != null)
            {
                _showMeshToggle.SetIsOnWithoutNotify(gridManager.showMesh);
                _showMeshToggle.onValueChanged.AddListener(OnShowMeshValueChanged);
            }

            if (_showOriginalObjectToggle != null)
            {
                _showOriginalObjectToggle.SetIsOnWithoutNotify(gridManager.showOriginalObject);
                _showOriginalObjectToggle.onValueChanged.AddListener(OnShowOriginalObjectValueChanged);
            }

            if (_useObjectCenterToggle != null)
            {
                _useObjectCenterToggle.SetIsOnWithoutNotify(gridManager.useObjectCenter);
                _useObjectCenterToggle.onValueChanged.AddListener(OnUseObjectCenterValueChanged);
            }

            // Initialize indices
            _currentGridStepIndex = Mathf.Max(0, gridManager.valuesCellSize.IndexOf(gridManager.cellWidth));
            _currentAngleStepIndex = Mathf.Max(0, gridManager.valuesAngleStep.IndexOf(gridManager.angleStep));
            _currentScaleStepIndex = Mathf.Max(0, gridManager.valuesScaleStep.IndexOf(gridManager.scaleStep));

            // Setup step buttons
            SetupStepControl(_btnGridStepPrev, _btnGridStepNext, _txtGridStep,
                () => ChangeGridStep(-1), () => ChangeGridStep(1), UpdateGridStepText);

            SetupStepControl(_btnAngleStepPrev, _btnAngleStepNext, _txtAngleStep,
                () => ChangeAngleStep(-1), () => ChangeAngleStep(1), UpdateAngleStepText);

            SetupStepControl(_btnScaleStepPrev, _btnScaleStepNext, _txtScaleStep,
                () => ChangeScaleStep(-1), () => ChangeScaleStep(1), UpdateScaleStepText);

            UpdateGridStepText();
            UpdateAngleStepText();
            UpdateScaleStepText();
        }

        private void SetupStepControl(Button prevBtn, Button nextBtn, TMP_Text text,
            UnityEngine.Events.UnityAction onPrev, UnityEngine.Events.UnityAction onNext, Action updateText)
        {
            if (prevBtn != null)
            {
                prevBtn.onClick.AddListener(onPrev);
            }
            if (nextBtn != null)
            {
                nextBtn.onClick.AddListener(onNext);
            }
            updateText?.Invoke();
        }

        private void OnActivateGridToggleValueChanged(bool value)
        {
            if (gridManager == null) return;
            if (value)
            {
                gridManager.EnableGrid();
            }
            else
            {
                gridManager.DisableGrid();
            }
        }

        private void OnSnapToGridToggleValueChanged(bool value)
        {
            if (gridManager == null) return;
            if (value)
            {
                gridManager.EnableSnapToGrid();
            }
            else
            {
                gridManager.DisableSnapToGrid();
            }
        }

        private void OnShowOriginalObjectValueChanged(bool value)
        {
            gridManager?.SetShowOriginalObject(value);
        }

        private void OnShowMeshValueChanged(bool value)
        {
            gridManager?.SetShowMesh(value);
        }

        private void OnUseObjectCenterValueChanged(bool value)
        {
            gridManager?.SetUseObjectCenter(value);
        }

        private void ChangeGridStep(int delta)
        {
            if (gridManager == null || gridManager.valuesCellSize == null || gridManager.valuesCellSize.Count == 0) return;
            _currentGridStepIndex = Mathf.Clamp(_currentGridStepIndex + delta, 0, gridManager.valuesCellSize.Count - 1);
            gridManager.SetCellWidth(gridManager.valuesCellSize[_currentGridStepIndex]);
            UpdateGridStepText();
        }

        private void UpdateGridStepText()
        {
            if (_txtGridStep != null && gridManager != null && _currentGridStepIndex < gridManager.optionsCellSize.Count)
            {
                _txtGridStep.text = gridManager.optionsCellSize[_currentGridStepIndex];
            }
        }

        private void ChangeAngleStep(int delta)
        {
            if (gridManager == null || gridManager.valuesAngleStep == null || gridManager.valuesAngleStep.Count == 0) return;
            _currentAngleStepIndex = Mathf.Clamp(_currentAngleStepIndex + delta, 0, gridManager.valuesAngleStep.Count - 1);
            gridManager.SetAngleStep(gridManager.valuesAngleStep[_currentAngleStepIndex]);
            UpdateAngleStepText();
        }

        private void UpdateAngleStepText()
        {
            if (_txtAngleStep != null && gridManager != null && _currentAngleStepIndex < gridManager.optionsAngleStep.Count)
            {
                _txtAngleStep.text = gridManager.optionsAngleStep[_currentAngleStepIndex];
            }
        }

        private void ChangeScaleStep(int delta)
        {
            if (gridManager == null || gridManager.valuesScaleStep == null || gridManager.valuesScaleStep.Count == 0) return;
            _currentScaleStepIndex = Mathf.Clamp(_currentScaleStepIndex + delta, 0, gridManager.valuesScaleStep.Count - 1);
            gridManager.SetScaleStep(gridManager.valuesScaleStep[_currentScaleStepIndex]);
            UpdateScaleStepText();
        }

        private void UpdateScaleStepText()
        {
            if (_txtScaleStep != null && gridManager != null && _currentScaleStepIndex < gridManager.optionsScaleStep.Count)
            {
                _txtScaleStep.text = gridManager.optionsScaleStep[_currentScaleStepIndex];
            }
        }

        public override void Close()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.Close();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
