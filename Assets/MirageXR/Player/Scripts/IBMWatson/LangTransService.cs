using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using IBM.Cloud.SDK;
using IBM.Watson.LanguageTranslator.V3;
using IBM.Watson.LanguageTranslator.V3.Model;
using System;

public class LangTransService : MonoBehaviour
{

    public Text ResponseTextField; // inspector slot for drag & drop of the Canvas > Text gameobject

    private LanguageTranslatorService languageTranslatorService;

    public string translationModel = "en-de";
    public string versionDate = "2018-12-19";
    //public string apiKey = "";
    //public string serviceUrl = "https://gateway-lon.watsonplatform.net/language-translator/api";

    public string lastTranslationResult = null;

    // Start is called before the first frame update
    void Start()
    {
        LogSystem.InstallDefaultReactors();
        Runnable.Run(ConnectToTranslationService());
    }

    private IEnumerator ConnectToTranslationService()
    {

        languageTranslatorService = new LanguageTranslatorService(versionDate);
		while (!languageTranslatorService.Authenticator.CanAuthenticate()) yield return null;

        //Translate("Where is the library");
    }

    //  Call this method from ExampleStreaming
    public void Translate(string text)
    {
        //  Array of text to translate
        List<string> translateText = new List<string>();
        translateText.Add(text);

        //  Call to the service
        languageTranslatorService.Translate(OnTranslate, translateText, translationModel);
    }

    //  OnTranslate handler
    private void OnTranslate(DetailedResponse<TranslationResult> response, IBMError error)
    {
        //  Populate text field with TranslationOutput
        ResponseTextField.text = response.Result.Translations[0]._Translation;
        lastTranslationResult = response.Result.Translations[0]._Translation;
    }

}
