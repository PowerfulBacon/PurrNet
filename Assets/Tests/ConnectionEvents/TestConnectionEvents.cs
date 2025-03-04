using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TestConnectionEvents
{
    [SetUp]
    public void Setup()
    {
        SceneManager.LoadScene($"SimpleNetworkManager");
    }

    [UnityTest]
    public IEnumerator TestConnectionEventsWithEnumeratorPasses()
    {
        yield return null;
    }
}
