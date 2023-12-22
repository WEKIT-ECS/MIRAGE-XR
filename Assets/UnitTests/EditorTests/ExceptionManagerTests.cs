using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MirageXR;
using System;

public class ExceptionManagerTests
{
    private ExceptionManager _em;
    public SentrySdk sentry;

    // Check if dropping an exception throws an error
    [Test]
    public void ExceptionManagerTests_LogCaughtException()
    {
        try
        {
            _em = new GameObject("ExceptionManagerTest").AddComponent<ExceptionManager>();
            _em.LogCaughtException("Test logTest", "stackTrace", LogType.Exception);
        }
        catch (Exception ex)
        {
            Assert.Fail($"ExceptionManager Test: Caught an error when trying to log test statement {ex.Message}.");
        }
    }

    // check if initialisation throws an exception (e.g. because DSN is not working)
    [UnityTest]
    public IEnumerator ExceptionManagerTests_Initialise()
    {
        try
        {
            GameObject go = new GameObject();
            ExceptionManager em = go.AddComponent<ExceptionManager>();
            em.sentry.Dsn = " abc ";
            Assert.Fail("Expected Exception, but did get this far");
        }
        catch (Exception ex)
        {
            Debug.Log("Correctly caught the exception with wrong DSN.");
        }

        try
        {
            GameObject go = new GameObject();
            ExceptionManager em = go.AddComponent<ExceptionManager>();
            //em.sentry.Dsn = "https://b23911205078e7a81bf1489e8aa0fabe@o4506320008118272.ingest.sentry.io/4506320009428992";
        }
        catch (Exception ex)
        {
            Debug.Log("Incorrectly caught an exception during init, something must have gone wrong.");
            Assert.Fail("ExceptionManagerTests: Expected no exception in init, but got: " + ex.Message);
        }

        Debug.Log("Passed.");
        Assert.Pass("Initialisation test passed.");

        yield return null;
    }

}