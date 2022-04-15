using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK;
using IBM.Watson.Assistant.V2;
using IBM.Cloud.SDK.DataTypes;
using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Logging;
using System;
using IBM.Watson.Assistant.V2.Model;
using IBM.Watson.ToneAnalyzer.V3.Model;
public class ToneService : MonoBehaviour
{

    public float emotion_threshold;
    private ToneService Tone;

    public string versionDate = "2019-07-03";
    //public string apiKey = "";
    //public string serviceUrl = "https://gateway-lon.watsonplatform.net/tone-analyzer/api";

    private string _stringToTestTone1 = "START AND TEST - OK!";
    private string _stringToTestTone2 = "SECOND TEST - Failed Test Sucks";
    private bool _analyzeToneTested = false;


    public Text ResultsAnalysis;
    //public MeshRenderer MacKenzieRenderer;  // main digital human 

    // Over the shoulder emotional spheres
    public MeshRenderer sphere_emo_joyRenderer;
    public MeshRenderer sphere_emo_angerRenderer;
    public MeshRenderer sphere_emo_fearRenderer;
    public MeshRenderer sphere_emo_disgustRenderer;
    public MeshRenderer sphere_emo_sadnessRenderer;

    public Material original_material;
    public Material red_material;
    public Material blue_material;
    public Material yellow_material;
    public Material green_material;
    public Material purple_material;
    public Material white_material;

    string _testString = "<speak version=\"1.0\"><express-as type=\"HI\">How are you today?</express-as></speak>";
    // Start is called before the first frame update
    void Start()
    {
        sphere_emo_joyRenderer.material = yellow_material;
        sphere_emo_angerRenderer.material = red_material;
        sphere_emo_fearRenderer.material = purple_material;
        sphere_emo_disgustRenderer.material = green_material;
        sphere_emo_sadnessRenderer.material = blue_material;

        sphere_emo_joyRenderer.transform.localScale = new Vector3(.075F, .075F, .075F);
        sphere_emo_angerRenderer.transform.localScale = new Vector3(.075F, .075F, .075F);
        sphere_emo_fearRenderer.transform.localScale = new Vector3(.075F, .075F, .075F);
        sphere_emo_disgustRenderer.transform.localScale = new Vector3(.075F, .075F, .075F);
        sphere_emo_sadnessRenderer.transform.localScale = new Vector3(.075F, .075F, .075F);

        emotion_threshold = 0.75f; // for loose demo - above 75% seems to work well - may vary by signal

      
    }

    private void OnGetToneAnalyze(ToneService resp, Dictionary<string, object> customData)
    {
        Log.Debug("ExampleToneAnalyzer.OnGetToneAnalyze()", "{0}", customData["json"].ToString());

        ResultsAnalysis.text = (customData["json"].ToString());  // works but long and cannot read

        /// Logging
        //Log.Debug("$$$$$ TONE LOG 0 ANGER", "{0}", resp._.tone_categories[0].tones[0].score); // ANGER resp.document_tone.tone_categories [0].tones [0].score);
        //Log.Debug("$$$$$ TONE LOG 1 DISGUST", "{0}", resp.document_tone.tone_categories[0].tones[1].score); // DISGUST
        //Log.Debug("$$$$$ TONE LOG 2 FEAR", "{0}", resp.document_tone.tone_categories[0].tones[2].score); // FEAR
        //Log.Debug("$$$$$ TONE LOG 3 JOY", "{0}", resp.document_tone.tone_categories[0].tones[3].score); // JOY
        //Log.Debug("$$$$$ TONE LOG 4 SAD", "{0}", resp.document_tone.tone_categories[0].tones[4].score); // SADNESS

        //Log.Debug("$$$$$ TONE ANALYTICAL", "{0}", resp.document_tone.tone_categories[1].tones[0].score); // ANALYTICAL
        //Log.Debug("$$$$$ TONE CONFIDENT", "{0}", resp.document_tone.tone_categories[1].tones[1].score); //  CONFIDENT
        //Log.Debug("$$$$$ TONE TENTATIVE", "{0}", resp.document_tone.tone_categories[1].tones[2].score); //  TENTATIVE


        //// EMOTION
        //if (resp.document_tone.tone_categories[0].tones[0].score > emotion_threshold)
        //{
        //    sphere_emo_angerRenderer.transform.localScale += new Vector3(0.025F, 0.025F, 0.025F);

        //}
        //else if (resp.document_tone.tone_categories[0].tones[1].score > emotion_threshold)
        //{
        //    sphere_emo_disgustRenderer.transform.localScale += new Vector3(0.025F, 0.025F, 0.025F);

        //}
        //else if (resp.document_tone.tone_categories[0].tones[2].score > emotion_threshold)
        //{
        //    sphere_emo_fearRenderer.transform.localScale += new Vector3(0.025F, 0.025F, 0.025F);

        //}
        //else if (resp.document_tone.tone_categories[0].tones[3].score > emotion_threshold)
        //{
        //    sphere_emo_joyRenderer.transform.localScale += new Vector3(0.025F, 0.025F, 0.025F);

        //}
        //else if (resp.document_tone.tone_categories[0].tones[4].score > emotion_threshold)
        //{
        //    sphere_emo_sadnessRenderer.transform.localScale += new Vector3(0.025F, 0.025F, 0.025F);

        //}



        // OTHER TEXT - Formatting for On Screen dump - LATER - pretty this up to use standard DESERIALIZE methods and table
        string RAW = (customData["json"].ToString());  // works but long and cannot read
                                                       //RAW = string.Concat("Tone Response \n", RAW); 
        RAW = Regex.Replace(RAW, "tone_categories", " \\\n");
        RAW = Regex.Replace(RAW, "}", "} \\\n");
        RAW = Regex.Replace(RAW, "tone_id", " ");
        RAW = Regex.Replace(RAW, "tone_name", " ");
        RAW = Regex.Replace(RAW, "score", " ");
        RAW = Regex.Replace(RAW, @"[{\\},:]", "");
        RAW = Regex.Replace(RAW, "\"", "");
        ResultsAnalysis.text = RAW;

        _analyzeToneTested = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
