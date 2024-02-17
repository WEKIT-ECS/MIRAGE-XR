using System.Collections.Generic;

namespace MirageXR
{
    public class ActivityRestorer
    {
        private readonly List<string> _actionsToRestore = new List<string>();
        private readonly List<ToggleObject> _objectsToRestore = new List<ToggleObject>();

        public void RestoreState(Activity activity)
        {
            var reactionsToRestore = new List<string>();
            foreach (var restoreAction in _actionsToRestore)
            {
                foreach (var action in activity.actions)
                {
                    if (action.id.Equals(restoreAction))
                    {
                        foreach (var activate in action.enter.activates)
                        {
                            switch (activate.type)
                            {
                                case ActionType.Action: break;
                                case ActionType.Reaction:
                                    reactionsToRestore.Add(activate.id);
                                    break;
                                default:
                                    _objectsToRestore.Add(activate);
                                    break;
                            }
                        }

                        foreach (var activate in action.exit.activates)
                        {
                            switch (activate.type)
                            {
                                case ActionType.Action: break;
                                case ActionType.Reaction:
                                    reactionsToRestore.Add(activate.id);
                                    break;
                                default:
                                    _objectsToRestore.Add(activate);
                                    break;
                            }
                        }

                        foreach (var deactivate in action.enter.deactivates)
                        {
                            switch (deactivate.type)
                            {
                                case ActionType.Action: break;
                                case ActionType.Reaction:
                                    if (reactionsToRestore.Contains(deactivate.id))
                                    {
                                        reactionsToRestore.Remove(deactivate.id);
                                    }

                                    break;
                                default:
                                    RemoveObjectFromRestoreList(deactivate);
                                    break;
                            }
                        }

                        foreach (var deactivate in action.exit.deactivates)
                        {
                            switch (deactivate.type)
                            {
                                case ActionType.Action: break;
                                case ActionType.Reaction:
                                    if (reactionsToRestore.Contains(deactivate.id))
                                    {
                                        reactionsToRestore.Remove(deactivate.id);
                                    }

                                    break;
                                default:
                                    RemoveObjectFromRestoreList(deactivate);
                                    break;
                            }
                        }
                    }
                }
            }

            foreach (var restoreObject in _objectsToRestore)
            {
                EventManager.ActivateObject(restoreObject);
            }

            foreach (var reaction in reactionsToRestore)
            {
                foreach (var action in activity.actions)
                {
                    if (action.type != ActionType.Reaction)
                        continue;

                    if (action.id.Equals(reaction))
                    {
                        Trigger.SetupTriggers(action);
                    }
                }
            }
        }

        public void RestoreActions(Activity activity, string restoreId)
        {
            if (!string.IsNullOrEmpty(activity.start))
            {
                _actionsToRestore.Add(activity.start);
            }

            foreach (var action in activity.actions)
            {
                // Continue only until restore step is reached.
                if (action.id.Equals(restoreId))
                {
                    break;
                }

                foreach (var activate in action.enter.activates)
                {
                    switch (activate.type)
                    {
                        case ActionType.Action:
                            if (!activate.id.Equals(restoreId))
                            {
                                _actionsToRestore.Add(activate.id);
                            }

                            break;
                        case ActionType.Reaction: break;
                    }
                }

                foreach (var activate in action.exit.activates)
                {
                    switch (activate.type)
                    {
                        case ActionType.Action:
                            if (!activate.id.Equals(restoreId))
                            {
                                _actionsToRestore.Add(activate.id);
                            }

                            break;
                        case ActionType.Reaction: break;
                    }
                }
            }

            foreach (var restoreAction in _actionsToRestore)
            {
                foreach (var action in activity.actions)
                {
                    if (action.id.Equals(restoreAction))
                    {
                        action.isCompleted = true;
                    }
                }
            }
        }

        private void RemoveObjectFromRestoreList(ToggleObject obj)
        {
            for (int i = _objectsToRestore.Count - 1; i >= 0; i--)
            {
                var item = _objectsToRestore[i];
                if (!item.id.Equals(obj.id) ||
                    !item.poi.Equals(obj.poi) ||
                    !item.position.Equals(obj.position) ||
                    !item.rotation.Equals(obj.rotation) ||
                    !item.predicate.Equals(obj.predicate)) continue;

                if (string.IsNullOrEmpty(item.url) || item.url.Equals(obj.url))
                {
                    _objectsToRestore.Remove(item);
                }
            }
        }

        public void Clear()
        {
            _actionsToRestore.Clear();
            _objectsToRestore.Clear();
        }
    }
}