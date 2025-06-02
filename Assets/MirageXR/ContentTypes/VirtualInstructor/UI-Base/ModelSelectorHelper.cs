using System.Collections.Generic;
using System.Linq;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
    /// <summary>
    /// Utility class for selecting an AI model from a list based on a configuration name.
    /// If a matching model by name is found, it is returned; otherwise, the first available model is selected.
    /// Returns null if the provided model list is empty or null.
    /// </summary>
    public static class ModelSelectorHelper
    {
        public static AIModel SelectModel(List<AIModel> models, string configName)
        {
            if (models == null || models.Count == 0)
                return null;

            if (!string.IsNullOrEmpty(configName))
            {
                var match = models.FirstOrDefault(m => m.Name.Equals(configName));
                if (match != null)
                    return match;
            }

            return models[0];
        }
    }
}