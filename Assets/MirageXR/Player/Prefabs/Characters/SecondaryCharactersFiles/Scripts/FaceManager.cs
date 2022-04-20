using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MirageXR;
public class FaceManager : MonoBehaviour
{
    private const bool debugLid = false;
    private const float blendMult = 10f;
    private const float defaultAntennaSpeed = 0.1f;
    private const int blendNumber = 4;

    public Vector3 eyelidLocalPosition = new Vector3(0,0.1128434f,0.06117219f); //0.001174, 0.000564

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private Material material;
    private int idx, _blendValues;
    private float[] blendVal;
    private float i, speedTime, lowerLidXRest = 30f, upperLidXRest = -60f, blendDiv, antennaSpeed;
    private Vector3 lowRot = Vector3.zero, upRot = Vector3.zero, veloL, veloR, lkDirL, lkDirR;
    //private WaitForEndOfFrame oneFrame = new WaitForEndOfFrame();
    private Transform lowerLid, upperLid, headBone, baseBoneL, baseBoneR, capTrnL, capTrnR, capTrgtL, capTrgtR, leafBoneL, leafBoneR;

    private void Awake()
    {
        headBone = transform.FindDeepChild("Alien").Find("rig:Hips").Find("rig:Spine").Find("rig:Spine1").Find("rig:Spine2").Find("rig:Neck").Find("rig:Head");
        lowerLid = transform.FindDeepChild("AlienLowerEyeLids");
        upperLid = transform.FindDeepChild("AlienUpperEyeLids");
        baseBoneL = headBone.FindDeepChild("rig:AntennaLeft");
        baseBoneR = headBone.FindDeepChild("rig:AntennaRight");

        lowerLid.SetParent(headBone, true);
        lowerLid.localPosition = eyelidLocalPosition;
        upperLid.SetParent(headBone, true);
        upperLid.localPosition = eyelidLocalPosition;
               
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        for (idx = 0; idx < skinnedMeshRenderer.materials.Length; idx++ )
            if ( skinnedMeshRenderer.materials[idx].name.Contains("AlienHead") ) { material = skinnedMeshRenderer.materials[idx]; break; }

        _blendValues = Shader.PropertyToID("_blendValues");
        blendVal = new float[blendNumber];
        blendDiv = 1f / blendMult;

        leafBoneL = baseBoneL.Find("rig:AntennaLeftTop");

        capTrgtL = new GameObject("capTarget_L").transform;
        capTrgtL.SetParent(baseBoneL.parent);
        capTrgtL.position = leafBoneL.position;
        capTrnL = new GameObject("capTransform_L").transform;
        capTrnL.position = leafBoneL.position;

        leafBoneR = baseBoneR.Find("rig:AntennaRightTop");

        capTrgtR = new GameObject("capTarget_R").transform;
        capTrgtR.SetParent(baseBoneR.parent);
        capTrgtR.position = leafBoneR.position;
        capTrnR = new GameObject("capTransform_R").transform;
        capTrnR.position = leafBoneR.position;

        antennaSpeed = defaultAntennaSpeed;
    }

    private void Start()
    {
        if (!debugLid) StartCoroutine(_Blink());
        else
        {
            upRot.x = 0; upperLid.localEulerAngles = upRot;
            lowRot.x = 0; lowerLid.localEulerAngles = lowRot;
        }
    }

    private void Update()
    {
        for ( idx = 0; idx < blendVal.Length; idx++ ) blendVal[idx] = skinnedMeshRenderer.GetBlendShapeWeight(idx) * blendDiv;
        if ( material != null ) material.SetFloatArray(_blendValues, blendVal);
        else Debug.LogError("Alien does't have correct materials");

        capTrnL.position = Vector3.SmoothDamp(capTrnL.position, capTrgtL.position, ref veloL, antennaSpeed);
        capTrnR.position = Vector3.SmoothDamp(capTrnR.position, capTrgtR.position, ref veloR, antennaSpeed);

        lkDirL = capTrnL.position - baseBoneL.position;
        lkDirR = capTrnR.position - baseBoneR.position;

        baseBoneL.rotation  = Quaternion.LookRotation(Vector3.Cross(lkDirL, -headBone.right), lkDirL);
        baseBoneR.rotation  = Quaternion.LookRotation(Vector3.Cross(lkDirR, -headBone.right), lkDirR);
    }

    private IEnumerator _Blink()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 6f));

        upRot.x = -30f;
        upperLid.localEulerAngles = upRot;

        yield return new WaitForEndOfFrame();

        upRot.x = 0;
        upperLid.localEulerAngles = upRot;
        lowRot.x = 0;
        lowerLid.localEulerAngles = lowRot;

        yield return new WaitForSeconds(0.1f);

        lowRot.x = lowerLidXRest;
        lowerLid.localEulerAngles = lowRot;

        yield return new WaitForEndOfFrame();

        speedTime = Random.Range(5f, 6f);

        for (i=0; i<=1f; i+=Time.deltaTime*speedTime )
        {
            upRot.x = Mathf.Lerp(0, upperLidXRest, i);
            upperLid.localEulerAngles = upRot;
            yield return new WaitForEndOfFrame();
        }

        upRot.x = upperLidXRest; upperLid.localEulerAngles = upRot;
        yield return new WaitForEndOfFrame();

        StartCoroutine(_Blink());
    }

    public void SetAntennaSpeed(float val)
    {
        if ( val <= 0 ) antennaSpeed = defaultAntennaSpeed;
        else antennaSpeed = val;
    }
}
