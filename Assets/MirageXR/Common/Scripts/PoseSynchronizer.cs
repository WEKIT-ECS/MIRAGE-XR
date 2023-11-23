using MirageXR;
using UnityEngine;

public class PoseSynchronizer : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _localSpace = false;

    private Pose _lastPose;

    public Transform target
    {
        get => _target;
        set => _target = value;
    }

    public bool localSpace
    {
        get => _localSpace;
        set => _localSpace = value;
    }

    private void Update()
    {
        if (!_localSpace)
        {
            CopyPose();
        }
        else
        {
            CopyLocalPose();
        }
    }

    private void CopyPose()
    {
        if (!target)
        {
            return;
        }

        var pose = target.GetPose();
        if (_lastPose != pose)
        {
            transform.SetPose(pose);
        }
    }

    private void CopyLocalPose()
    {
        if (!target)
        {
            return;
        }

        var pose = target.GetLocalPose();
        if (_lastPose != pose)
        {
            transform.SetLocalPose(pose);
        }
    }
}
