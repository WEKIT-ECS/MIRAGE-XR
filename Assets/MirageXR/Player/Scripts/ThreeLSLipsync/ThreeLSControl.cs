
/* ThreeLSControl
 * 
 * Author: Gerard Llorach
 * Date: 20/03/2018
 * Version: 0.0
 * 
 * DESCRIPTION:
 * This script connects the ThreeLS class with the scene. ThreeLS needs an 
 * AudioSource and returns weights from 0 and 1 for the blend shapes Kiss,
 * Lips Closed and Mouth Open (in FACS its AU22, AU24 and AU27). This script
 * finds the blend shapes of the SkinnedMeshRenderers that match the input name
 * and assigns the weights from ThreeLS to the SkinnedMeshRenderers.
 * 
 * USE:
 * 0. Watch the video explaining how to use it (found in the description of the asset)
 * 1. Add this script to the virtual character. The objects with the blend shapes
 * to modify should be children of this object.
 * 2. Connect your Audio Source to this script (Audio Input variable)
 * 3. In the editor, write down the name of the blend shapes to modify under
 * each corresponding input string array (Kiss Blend Shape Names, Lips Closed
 * Blend Shape Names, Mouth Open Blend Shape Names). You will find the name of
 * the blend shapes in the Skinned Mesh Renderer component of the head/mouth/teeth
 * of your virtual character.
 * 4. Play the scene. If the Audio Source is playing, you should see the lips
 * of your virtual character moving. You will get error messages if the blend
 * shape names you wrote down are not found in any of the child meshes.
 * 5. Play around with the sliders Threshold, Smoothness and Vocal Track Factor 
 * as well as the factors for each blend shape.
 * Threshold helps when the audio is a bit noise. The smaller the value,
 * less sentitiveness. Smoothness makes the transitions of the lips more smooth.
 * Vocal Track Factor tries to take into account differences between speakers. 
 * Values around 0.7-0.8 have been found to be good for female voices.
 * 
 * OTHER COMMENTS:
 * This lipsync strategy should work also with a microphone, although I haven't
 * seen a straight forward way to connect the microphone to the spectrum analysis.
 * Remains to be done I guess.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeLSControl : MonoBehaviour
{

    // PUBLIC
    // Configurable parameters of ThreeLS
    [Range(0.01f, 2.0f)]
    public float threshold = 0.5f;
    [Range(0.01f, 1.0f)]
    public float smoothness = 0.6f;
    [Range(0.01f, 1.0f)]
    public float vocalTractFactor = 1.0f;

    // Blendshape names
    public string[] kissBlendShapeNames;
    public string[] lipsClosedBlendShapeNames;
    public string[] mouthOpenBlendShapeNames;

    // Blendshape factors
    [Range(0.01f, 3.0f)]
    public float kissFactor = 1.0f;
    [Range(0.01f, 3.0f)]
    public float lipsClosedFactor = 1.0f;
    [Range(0.01f, 3.0f)]
    public float mouthOpenFactor = 1.0f;

    // Default state rotations if not using blend shapes, bones instead
    // public Vector3 neutralRotation;

    // Visualization
    public bool spectrumVisualization = false;

    // Audio files
    private AudioSource audioInput;

    // PRIVATE
    // Meshes with the blend shapes
    //public SkinnedMeshRenderer headMesh;
    private SkinnedMeshRenderer[] kissMeshes;
    private int[] kissIndices;
    private SkinnedMeshRenderer[] lipsClosedMeshes;
    private int[] lipsClosedIndices;
    private SkinnedMeshRenderer[] mouthOpenMeshes;
    private int[] mouthOpenIndices;

    // ThreeLS instance
    private ThreeLS LS;

    // Connection between ThreeLS blend shapes and the character's blend shapes
    private class BSReference
    {
        public List<SkinnedMeshRenderer> meshes;
        public List<int> BSIndices;
        public bool[] BSNameIsFound;
        public string[] BSNames;
        public float influence;

        public BSReference(string[] inBSNames)
        {
            BSNames = inBSNames;
            BSNameIsFound = new bool[inBSNames.Length];
            meshes = new List<SkinnedMeshRenderer>();
            BSIndices = new List<int>();
        }
    }
    private BSReference[] BSToFind;

    // Decay of blend shapes after audio stops
    private bool fadingOut;
    private bool wasPlaying;
    private float timeToFadeOut;


    // Use this for initialization
    void Start()
    {

        StartCoroutine(Init());

    }

    IEnumerator Init()
    {
        while (audioInput == null)
        {
            audioInput = GetComponent<AudioSource>();
            yield return new WaitForSeconds(0.1f);
        }


        // Initialize lipsync
        LS = new ThreeLS(audioInput, threshold, smoothness, vocalTractFactor);

        findBlendShapes();

    }

    AudioSource CharacterAudioSource(Transform obj)
    {
        AudioSource audioSource = null;
        if (transform.parent.GetComponentInChildren<AudioSource>() != null)
            audioSource = transform.parent.GetComponentInChildren<AudioSource>();
        else if (transform.parent.parent.GetComponentInChildren<AudioSource>() != null)
            audioSource = transform.parent.parent.GetComponentInChildren<AudioSource>();
        else if (transform.parent.parent.parent.GetComponentInChildren<AudioSource>() != null)
            audioSource = transform.parent.parent.parent.GetComponentInChildren<AudioSource>();

        return audioSource;
    }


    // Update is called once per frame
    void Update()
    {
        if (LS == null) return;

        // Update lipsync values and apply to mesh
        LS.updateLS();

        // Blend shapes
        updateBlendShapes();

#if (UNITY_EDITOR)
        // Editor Adjustment Visualization
        if (spectrumVisualization)
            adjustmentVisualization();
#endif
        // Update LS variables for modification in the Editor
        LS.threshold = threshold;
        LS.smoothness = smoothness;
        LS.updateFrequencyBins(vocalTractFactor);
        BSToFind[0].influence = kissFactor;
        BSToFind[1].influence = lipsClosedFactor;
        BSToFind[2].influence = mouthOpenFactor;


    }


    // Lipsync values
    private void updateBlendShapes()
    {
        // Blend shapes fade to zero when audio stops
        if (!audioInput.isPlaying && wasPlaying)
        {// Start decay of lipsync values when audio stopped
            wasPlaying = false;
            fadingOut = true;
            timeToFadeOut = Time.time + 1.0f; // One second fading out
        }
        if (!audioInput.isPlaying && fadingOut)
        { // Fade out lip values to zero
            fadeOutLips();
            return;
        }
        else if (!audioInput.isPlaying)
            return;

        // Assign weights (kiss, lipsClosed, mouthOpen)
        for (int i = 0; i < 3; i++)
        {
            BSReference BSRef = BSToFind[i];
            float BSWeight = LS.LSbsw[i] * 100.0f * BSRef.influence;
            for (int m = 0; m < BSRef.meshes.Count; m++)
            {
                BSRef.meshes[m].SetBlendShapeWeight(BSRef.BSIndices[m], BSWeight);
            }
        }

        // Simplified example when blendshape indices are known and correspond directly to Kiss, LipsClosed and MouthOpen
        // Kiss
        //headMesh.SetBlendShapeWeight(1, LS.LSbsw[0] * 100.0f);
        // Lips Closed
        //headMesh.SetBlendShapeWeight(2, LS.LSbsw[1] * 100.0f);
        // MouthOpen
        //headMesh.SetBlendShapeWeight(3, LS.LSbsw[2] * 100.0f);

        wasPlaying = true;
    }






    // Spectrum visualization near the origin
    private void adjustmentVisualization()
    {

        // Visualization - Spectrum (cyan), threshold (white), vocalTractEffect (red), energies (green), blend shape weights (yellow)
        var spectrum = LS.smoothSpectrum;
        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            float vv = threshold + (spectrum[i] + 20) / 140;
            float vv_p = threshold + (spectrum[i - 1] + 20) / 140;

            Debug.DrawLine(new Vector3((i - 1) / 200.0f + 1.0f, vv_p, 0), new Vector3(i / 200.0f + 1.0f, vv, 0), Color.cyan);
        }
        // Draw threshold line
        Debug.DrawLine(new Vector3(1.0f, 0, 0), new Vector3(spectrum.Length / 200.0f + 1.0f, 0, 0), Color.white);
        // Draw FBins
        for (int bindInd = 0; bindInd < LS.freqBins.Length - 1; bindInd++)
        {
            int indxIn = Mathf.RoundToInt(LS.freqBins[bindInd] * (LS.fftSize / 2) / (LS.fs / 2));
            int indxEnd = Mathf.RoundToInt(LS.freqBins[bindInd + 1] * (LS.fftSize / 2) / (LS.fs / 2));
            Debug.DrawLine(new Vector3(indxIn / 200.0f + 1.0f, 0.5f, 0), new Vector3(indxIn / 200.0f + 1.0f, -0.5f, 0), Color.red);
            Debug.DrawLine(new Vector3(indxEnd / 200.0f + 1.0f, 0.5f, 0), new Vector3(indxEnd / 200.0f + 1.0f, -0.5f, 0), Color.red);
        }
        // Energies
        Debug.DrawLine(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, LS.energies[1] + 1.0f, 0.0f), Color.green);
        Debug.DrawLine(new Vector3(1.2f, 1.0f, 0.0f), new Vector3(1.2f, LS.energies[2] + 1.0f, 0.0f), Color.green);
        Debug.DrawLine(new Vector3(1.4f, 1.0f, 0.0f), new Vector3(1.4f, LS.energies[3] + 1.0f, 0.0f), Color.green);
        // Blend shape weights
        Debug.DrawLine(new Vector3(2.0f, 1.0f, 0.0f), new Vector3(2.0f, LS.LSbsw[0] + 1.0f, 0.0f), Color.blue);
        Debug.DrawLine(new Vector3(2.2f, 1.0f, 0.0f), new Vector3(2.2f, LS.LSbsw[1] + 1.0f, 0.0f), Color.blue);
        Debug.DrawLine(new Vector3(2.4f, 1.0f, 0.0f), new Vector3(2.4f, LS.LSbsw[2] + 1.0f, 0.0f), Color.blue);

    }




    // Looks for blend shapes names in the children
    private void findBlendShapes()
    {
        // Find blend shapes in children
        // References to mesh and blend shape index
        BSToFind = new BSReference[3];
        BSToFind[0] = new BSReference(kissBlendShapeNames);
        BSToFind[1] = new BSReference(lipsClosedBlendShapeNames);
        BSToFind[2] = new BSReference(mouthOpenBlendShapeNames);

        // Find children with blend shapes
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.GetComponent<SkinnedMeshRenderer>())
                if (child.GetComponent<SkinnedMeshRenderer>().sharedMesh.blendShapeCount != 0)
                {
                    var mesh = child.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                    var BSMesh = child.GetComponent<SkinnedMeshRenderer>();
                    // Find blend shape correspondance
                    for (var j = 0; j < mesh.blendShapeCount; j++)
                    {
                        // Store references of the mesh and blend shape index
                        // Iterate over kiss, lipsClosed and mouthOpen
                        for (int k = 0; k < BSToFind.Length; k++)
                        {
                            // Iterate over the blend shape names assigned to each lipsync blend shape
                            for (int ss = 0; ss < BSToFind[k].BSNames.Length; ss++)
                            {
                                // If the blend shape name of the mesh corresponds to the one written in the editor, store the mesh, the BS index and that it was found
                                if (string.Equals(BSToFind[k].BSNames[ss], mesh.GetBlendShapeName(j)))
                                {
                                    BSToFind[k].meshes.Add(BSMesh);
                                    BSToFind[k].BSIndices.Add(j);
                                    BSToFind[k].BSNameIsFound[ss] = true;
                                    //Debug.Log("Found! " + mesh.GetBlendShapeName(j) + ", in " + mesh.name + ", mesh count " + BSToFind[k].meshes.Count);
                                }
                            }
                        }
                    }
                    //  Debug.Log("Object name: " + child.name +", Blend shape name: " + mesh.GetBlendShapeName(j) + ", Blend shape index: " + j);
                }
        }

        // (Debug) Display the blendshapes not found
        for (int k = 0; k < BSToFind.Length; k++)
        {
            for (int ss = 0; ss < BSToFind[k].BSNames.Length; ss++)
            {
                if (!BSToFind[k].BSNameIsFound[ss])
                    Debug.LogError("Blend Shape " + BSToFind[k].BSNames[ss] + " not found in children!");
            }
        }
    }

    // Fade outs the blend shapes to zero
    private void fadeOutLips()
    {
        // Linearly decrease the value of the blend shapes
        for (var i = 0; i < 3; i++)
        {
            BSReference BSRef = BSToFind[i];
            for (int m = 0; m < BSRef.meshes.Count; m++)
            {
                var indx = BSRef.BSIndices[m];
                var weight = BSRef.meshes[m].GetBlendShapeWeight(indx) * (timeToFadeOut - Time.time);
                BSRef.meshes[m].SetBlendShapeWeight(indx, weight);
            }
        }
        if (timeToFadeOut < Time.time)
        {
            fadingOut = false;
            // Set blend shapes to zero
            for (var i = 0; i < 3; i++)
            {
                BSReference BSRef = BSToFind[i];
                for (int m = 0; m < BSRef.meshes.Count; m++)
                {
                    var indx = BSRef.BSIndices[m];
                    BSRef.meshes[m].SetBlendShapeWeight(indx, 0);
                }
            }
        }

    }

}
