using System;
using i5.Toolkit.Core.ServiceCore;
using MirageXR;
using UnityEngine;

public class MaggieManager
{
    public ITextToSpeechService TTSService { get; set; }

    public readonly string[] okMessages = new string[]
    {
        "Ok.",
        "Done.",
        "Yup."
    };

    public readonly string[] errorMessages = new string[]
    {
        "There's some error. Check the log.",
        "I detected an error. Check the log.",
        "Error detected, please check the log.",
    };

    public readonly string[] activityReadyMessages = new string[]
    {
        "Activity loaded, let's roll!",
        "Are you ready for the activity? Cause I am.",
        "Time to boogie. Let's start the activity.",
        "All systems go. Igniting the activity.",
        "Ok, I'm ready for action. How about you?",
        "Let's go.",
        "Everything seems to be in order. Let's do some work.",
        "Yippee. Let's begin."
    };



    public MaggieManager(ITextToSpeechService ttsService = null)
    {
        if (ttsService == null)
        {
            TTSService = new HTKTextToSpeechService();
        }
        else
        {
            TTSService = ttsService;
        }
    }

    public void Speak(string text)
    {
        TTSService.StopSpeaking();

        // Don't talk empty and don't talk over.
        if (!string.IsNullOrEmpty(text) && !TTSService.IsSpeaking())
        {
            TTSService.StartSpeaking(text);
        }
    }

    public void ActivityReady()
    {
        TTSService.StartSpeaking(SelectRandom(activityReadyMessages));
    }

    public void Stop()
    {
        TTSService.StopSpeaking();
    }

    public void Error()
    {
        TTSService.StartSpeaking(SelectRandom(errorMessages));
    }

    public void Ok()
    {
        TTSService.StartSpeaking(SelectRandom(okMessages));
    }

    private string SelectRandom(string[] messages)
    {
        return messages[UnityEngine.Random.Range(0, messages.Length)];
    }
}
