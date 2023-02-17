using UnityEngine;
using Obi;


[RequireComponent(typeof(ObiRope))]
public class HosePump : MonoBehaviour {

    [Header("Bulge controls")]
    public float pumpSpeed = 5;
    public float bulgeFrequency = 3;
    public float baseThickness = 0.04f;
    public float bulgeThickness = 0.06f;
    public Color bulgeColor = Color.cyan;

    [Header("Flow controls")]
    public ParticleSystem waterEmitter;
    public float flowSpeedMin = 0.5f;
    public float flowSpeedMax = 7;
    public float minEmitRate = 100;
    public float maxEmitRate = 1000;

    private ObiRope rope;
    public ObiPathSmoother smoother;
    private float time = 0;

    void OnEnable()
    {
        rope = GetComponent<ObiRope>();
        smoother = GetComponent<ObiPathSmoother>();
        rope.OnBeginStep += Rope_OnBeginStep;
    }
    void OnDisable()
    {
        rope.OnBeginStep -= Rope_OnBeginStep;
    }

    private void Rope_OnBeginStep(ObiActor actor, float stepTime)
    {
        time += stepTime * pumpSpeed;

        float distance = 0;
        float sine = 0;

        // iterate over all particles, changing their radius and color based on a sine wave:
        // (note this would not support resizable / cuttable ropes, to add support for that use rope.elements instead)
        for (int i = 0; i < rope.solverIndices.Length; ++i)
        {
            int solverIndex = rope.solverIndices[i];

            if (i > 0)
            {
                int previousIndex = rope.solverIndices[i - 1];
                distance += Vector3.Distance(rope.solver.positions[solverIndex],rope.solver.positions[previousIndex]);
            }

            sine = Mathf.Max(0, Mathf.Sin(distance * bulgeFrequency - time));

            rope.solver.principalRadii[solverIndex] = Vector3.one * Mathf.Lerp(baseThickness,bulgeThickness, sine);
            rope.solver.colors[solverIndex] = Color.Lerp(Color.white, bulgeColor, sine);
        }

        // change particle emission rate/speed based on sine wave at the last particle:
        if (waterEmitter != null)
        {
            var main = waterEmitter.main;
            main.startSpeed = Mathf.Lerp(flowSpeedMin,flowSpeedMax,sine);

            var emission = waterEmitter.emission;
            emission.rateOverTime = Mathf.Lerp(minEmitRate, maxEmitRate, sine);
        }
    }

    public void LateUpdate() 
    {
        if (smoother != null && waterEmitter != null)
        {
            ObiPathFrame section = smoother.GetSectionAt(1);
            waterEmitter.transform.position = transform.TransformPoint(section.position);
            waterEmitter.transform.rotation = transform.rotation * (Quaternion.LookRotation(section.tangent, section.binormal));
        }
    }
}   

