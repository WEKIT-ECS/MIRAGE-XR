using UnityEngine;
using System.Collections;

public class EyeBlink : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRenderer;
    [Range (0,100)]
    [SerializeField] private int DefaultEyeOpenAmount = 10;
    [SerializeField] private int blinkingBlendIndex = 0;
    [SerializeField] private float blinkSpeed = 8f;
    [SerializeField] private int blinkingFreq = 10;
    private float _blinking;

    bool _eyesOpened = true;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }


    private void Start()
    {
        _blinking = DefaultEyeOpenAmount;
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (gameObject)
        {
            if (_blinking <= 100f && !_eyesOpened)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blinkingBlendIndex, _blinking);
                _blinking += blinkSpeed;


                if(_blinking >= 100)
                    _eyesOpened = true;
            }


            if (_eyesOpened == true && _blinking >= DefaultEyeOpenAmount)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(blinkingBlendIndex, _blinking);
                _blinking -= blinkSpeed;

                if (_blinking <= DefaultEyeOpenAmount)
                {
                    _eyesOpened = false;
                    yield return new WaitForSeconds(Random.Range(2f / blinkingFreq , 20 / blinkingFreq));
                }   
            }

            yield return null;
        }
    }
}