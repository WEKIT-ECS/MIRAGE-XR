using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEngine;
using static MirageXR.ActivityRecorderService;

namespace MirageXR
{
    [Serializable]
    public class ActivityRecorderServiceConfiguration
    {
        public TrialPartner CurrentTrialPartner;

        public GameObject worldOriginMarkerPrefab;
        public GameObject taskStationPrefab;
        public string dirBase;
        public Transform RecordingOrigin;
        public Vector3 RecordingEuler = Vector3.zero;
        public bool calibrationMarkerFound = false;
        public bool RecordingUnderWay = false;
    }
}
