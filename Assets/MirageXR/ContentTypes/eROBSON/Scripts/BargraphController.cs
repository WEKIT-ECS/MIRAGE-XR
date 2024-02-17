using UnityEngine;

public class BargraphController : MonoBehaviour
{
    [SerializeField] private Renderer[] lightRenderers;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;


    /// <summary>
    /// turn on the bargraph light depends on the power
    /// </summary>
    /// <param name="status">is connected or not</param>
    /// <param name="power">the power received to this bit</param>
    public void TurnOnLights(bool status, float power)
    {
        // Turn off all lights first
        TurnOffAllLights();

        //bit is disconnected
        if (!status)
        {
            return;
        }

        // Determine how many lights to turn on based on the power level
        var lightsToTurnOn = power switch
        {
            > 0f and <= 0.2f => 1,
            > 0.2f and <= 0.4f => 2,
            > 0.4f and <= 0.6f => 3,
            > 0.6f and <= 0.8f => 4,
            > 0.8f and <= 1.0f => 5,
            _ => 0
        };

        // Turn on the appropriate number of lights
        for (var i = 0; i < lightsToTurnOn; i++)
        {
            lightRenderers[i].material = onMaterial;
        }
    }


    private void Start()
    {
        TurnOffAllLights();
    }



    /// <summary>
    /// Turn off all lights first
    /// </summary>
    private void TurnOffAllLights()
    {
        foreach (var led in lightRenderers)
        {
            led.material = offMaterial;
        }
    }
}