using System;
using UnityEngine;

namespace MirageXR.Services.AI_service_backend
{
    [Serializable]
    public class AIServiceModel
    {
        public string Name;
        public string[] Models;
    }

    [Serializable]
    public class AIServices
    {
        public AIServiceModel[] Services;
    }
}
