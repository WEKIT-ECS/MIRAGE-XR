using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalKeywords : MonoBehaviour
{

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("AppSelection");
    }
}
