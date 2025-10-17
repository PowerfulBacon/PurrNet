using PurrNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField, PurrScene] string mainSceneName = "Main";

    void Awake()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
