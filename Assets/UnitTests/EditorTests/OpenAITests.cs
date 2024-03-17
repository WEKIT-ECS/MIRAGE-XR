using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using MirageXR;

public class OpenAITests
{
    // A Test behaves as an ordinary method
    [Test]
    public void OpenAI_Test()
    {
        //something
    }

    [UnityTest]
    public IEnumerator OpenAI_InitialisationTest()
    {
        DialogueService ds;
        try
        {
            var go = new GameObject();
            ds = go.AddComponent<DialogueService>();
            ds.AI = AIservice.OpenAI;
            ds.AIprompt = "You are shakespeare";
            ds.CreateOpenAIServiceAsync().AsAsyncVoid();

            //ds.SendMessageToAssistantAsync("TEST");

            //var _openAIinterface = new OpenAI_API.OpenAIAPI();
            //var auth = new OpenAI_API.APIAuthentication(_openAIinterface.Auth.ApiKey);
            //var worked = await auth.ValidateAPIKey();
            //createSessionTested = _openAIinterface.Auth.OpenAIOrganization != null;
        }
        catch (Exception ex)
        {
            Assert.Fail($"OpenAI: AI provider initialisation test failed: {ex.Message}, trace: {ex.StackTrace}");
        }

        Assert.Pass($"OpenAI: AI provider initialisation test passed.");

        yield return null;
    }
}
