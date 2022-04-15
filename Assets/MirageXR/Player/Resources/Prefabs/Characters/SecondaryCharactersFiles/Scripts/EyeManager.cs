using System.Collections;
using UnityEngine;

public class EyeManager : MonoBehaviour
{
    [System.Serializable]
    public enum EyeColor
    {
        MakeUpBlue,
        MakeUpBrown,
        MakeUpGreen,
        PlainBlue,
        PlainBrown,
        PlainGreen,
        ColorBrown
    }

    [SerializeField] private EyeColor eyeColor;
    private float tileSize = 0.125f;
    private float col = 4, row = 0;
    private SkinnedMeshRenderer skin;
    private Material mat;
    private Vector2 offset;

    private void Awake()
    {
        row = (float) eyeColor;
        offset = new Vector2(col * tileSize, row * tileSize);
        skin = GetComponent<SkinnedMeshRenderer>();
        mat = skin.material;
        mat.SetTextureOffset("_MainTex", offset);
    }

    private void Start()
    {
        StartCoroutine(_Blink());
    }

    private IEnumerator _Blink()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 6f));

        offset.x = 1f * tileSize;
        mat.SetTextureOffset("_MainTex", offset);
        yield return new WaitForSeconds(0.01f);

        for (float i = 0; i<5f; i++ )
        {
            offset.x = i * tileSize;
            mat.SetTextureOffset("_MainTex", offset);
            if (i==0) yield return new WaitForSeconds(0.1f);
            else yield return new WaitForSeconds(0.05f);
        }

        offset.x = col * tileSize;
        mat.SetTextureOffset("_MainTex", offset);
        yield return new WaitForEndOfFrame();

        //Debug
        //row = (float) eyeColor;
        //offset.y = row * tileSize;

        StartCoroutine(_Blink());
    }
}
