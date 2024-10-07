using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LearningExperienceEngine.DataModel;
using UnityEngine;
using UnityEngine.Events;

namespace MirageXR.NewDataModel
{
    public class StepManager : IStepManager
    {
        private struct StepQueueItem
        {
            public Guid StepId;
            public Guid HierarchyItemId;
        }

        private const string DefaultStepName = "Step {0}";

        public event UnityAction<ActivityStep> OnStepChanged
        {
            add
            {
                _onStepChanged.AddListener(value);
                if (_currentStep != null)
                {
                    value(_currentStep);
                }
            }
            remove => _onStepChanged.RemoveListener(value);
        }

        private readonly UnityEventActivityStep _onStepChanged = new();

        private List<ActivityStep> _steps = new();
        private List<HierarchyItem> _hierarchy = new();
        private readonly List<StepQueueItem> _stepQueue = new();

        private IContentManager _contentManager;
        private IActivityManager _activityManager;

        private ActivityStep _currentStep;
        private HierarchyItem _currentHierarchyItem;

        public ActivityStep CurrentStep => _currentStep;

        public UniTask InitializeAsync(IContentManager contentManager, IActivityManager activityManager)
        {
            _contentManager = contentManager;
            _activityManager = activityManager;
            return UniTask.CompletedTask;
        }

        public void LoadSteps(Activity activity, Guid parentId = default)
        {
            _steps = activity.Steps;
            _hierarchy = activity.Hierarchy;
            UpdateStepQueue();
            GoToStep(_stepQueue.First().StepId);
        }

        public void AddHierarchyItem(HierarchyItem hierarchyItem, Guid parentId = default)
        {
            if (parentId != default)
            {
                var item = _hierarchy.First(t => t.Id == parentId);
                item.Hierarchy ??= new List<HierarchyItem>();
                item.Hierarchy.Add(hierarchyItem);
            }
            else
            {
                _hierarchy.Add(hierarchyItem);
            }

            UpdateStepQueue();
        }

        public ActivityStep AddStep(Location location, string name = IStepManager.EmptyString, string description = IStepManager.EmptyString, Guid hierarchyItemId = default)
        {
            var step = new ActivityStep
            {
                Name = string.IsNullOrEmpty(name) ? GetDefaultName() : name,
                Description = description,
                Location = location,
                Id = Guid.NewGuid(),
                CreationDate = DateTime.UtcNow,
                Version = Application.version,
                Attachment = null,
                Comments = null,
                Triggers = null,
                PrivateNotes = null,
                RequiredToolsPartsMaterials = null
            };

            if (_currentHierarchyItem == null)
            {
                _currentHierarchyItem = new HierarchyItem
                {
                    Id = Guid.NewGuid(),
                    CreationDate = DateTime.UtcNow,
                    Version = Application.version,
                    Title = GetDefaultHierarchyName(),
                    StepIds = new List<Guid>(),
                };
                
                _hierarchy.Add(_currentHierarchyItem);
            }

            _currentHierarchyItem.StepIds ??= new List<Guid>();
            _currentHierarchyItem.StepIds.Add(step.Id);
            _steps.Add(step);

            _activityManager.UpdateActivity();

            UpdateStepQueue();
            if (_currentStep == null)
            {
                GoToStep(step.Id);
            }

            return step;
        }

        public void GoToNextStep()
        {
            var guid = Guid.Empty;
            for (var i = 0; i < _stepQueue.Count; i++)
            {
                if (_stepQueue[i].StepId == _currentStep.Id && i < _stepQueue.Count - 1)
                {
                    guid = _stepQueue[i + 1].StepId;
                    break;
                }
            }

            if (guid != Guid.Empty)
            {
                GoToStep(guid);
            }
        }

        public void GoToPreviousStep()
        {
            var guid = Guid.Empty;
            for (var i = 0; i < _stepQueue.Count; i++)
            {
                if (_stepQueue[i].StepId == _currentStep.Id && i - 1 >= 0)
                {
                    guid = _stepQueue[i - 1].StepId;
                    break;
                }
            }

            if (guid != Guid.Empty)
            {
                GoToStep(guid);
            }
        }

        public bool TryGoToStep(Guid stepId)
        {
            try
            {
                GoToStep(stepId);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        public void GoToStep(Guid stepId)
        {
            _currentStep = _steps.First(t => t.Id == stepId);
            _onStepChanged.Invoke(_currentStep);
            _contentManager.ShowContent(_currentStep.Id);
        }

        public List<ActivityStep> GetSteps()
        {
            return _steps;
        }

        public List<HierarchyItem> GetHierarchy()
        {
            return _hierarchy;
        }

        public void Reset()
        {
            _steps.Clear();
            _hierarchy.Clear();
            _stepQueue.Clear();
            _currentStep = null;
            _currentHierarchyItem = null;
        }

        private string GetDefaultName()
        {
            return string.Format(DefaultStepName, _steps.Count + 1);
        }

        private string GetDefaultHierarchyName()
        {
            return string.Empty; //string.Format(DefaultStepName, _steps.Count + 1);
        }

        private void UpdateStepQueue()
        {
            _stepQueue.Clear();
            UpdateStepQueueRecursively(_stepQueue, _hierarchy);
        }

        private void UpdateStepQueueRecursively(List<StepQueueItem> stepQueue, List<HierarchyItem> hierarchy)
        {
            _stepQueue.Clear();
            foreach (var item in hierarchy)
            {
                if (item.StepIds is { Count: > 0 })
                {
                    stepQueue.AddRange(item.StepIds.Select(t => new StepQueueItem { StepId = t, HierarchyItemId = item.Id }));
                }

                if (item.Hierarchy is { Count: > 0 })
                {
                    UpdateStepQueueRecursively(stepQueue, item.Hierarchy);
                }
            }
        }
    }
}