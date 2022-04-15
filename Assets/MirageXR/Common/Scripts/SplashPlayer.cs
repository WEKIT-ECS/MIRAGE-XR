using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashPlayer : MonoBehaviour
{
    [SerializeField] private GameObject splashCanvas;
    [SerializeField] private float splashSize = 3.0f;
    [SerializeField] private float fadingFactor = 0.2f;
    [SerializeField] private float displayLength = 3f;
    [SerializeField] private Image[] splashImages;


    void Start()
    {
        //hide all splashes at the start
        foreach (var img in splashImages)
            img.color = new Color(1, 1, 1, 0);



        StartCoroutine(PlaySplashes());
    }

    private void Update()
    {
        splashCanvas.transform.position = Camera.main.transform.position + Vector3.forward * splashSize;
        splashCanvas.transform.LookAt(2 * Camera.main.transform.position - splashCanvas.transform.position);
    }

    IEnumerator PlaySplashes()
    {

        //first time wait a bit more because of privacy confirmaion questions
        if (!PlayerPrefs.HasKey("FirstTimeLaunch"))
        {
            yield return new WaitForSeconds(2f);
            PlayerPrefs.SetInt("FirstTimeLaunch", 1);
        }

        foreach (var img in splashImages)
        {
            Color temp = new Color(1, 1, 1, 0);
            while (img.color.a < 1)
            {
                temp.a += fadingFactor * Time.deltaTime;
                img.color = temp;
                yield return null;
            }
            img.color = Color.white;
            yield return new WaitForSeconds(displayLength);
            while (img.color.a > 0)
            {
                temp.a -= fadingFactor * Time.deltaTime;
                img.color = temp;
                yield return null;
            }
            img.color = Color.clear;
        }
        SceneManager.LoadSceneAsync("Start", LoadSceneMode.Single);
    }

         
}
