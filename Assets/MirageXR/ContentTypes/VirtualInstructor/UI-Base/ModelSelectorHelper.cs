using System.Collections.Generic;
using System.Linq;
using LearningExperienceEngine.DataModel;

namespace MirageXR
{
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