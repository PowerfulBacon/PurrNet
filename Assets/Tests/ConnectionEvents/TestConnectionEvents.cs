using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine.TestTools;

namespace Purrnet.Tests
{
    public class TestConnectionEvents
    {
        [SetUp]
        public void Setup()
        {
            EditorSceneManager.OpenScene($"Assets/Tests/ConnectionEvents/SimpleNetworkManager.unity");
        }

        [TearDown]
        public void Teardown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        [UnityTest]
        public IEnumerator TestConnectionEventsWithEnumeratorPasses()
        {
            yield return null;
        }
    }
}
