using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{

    private int lastFrameIndex;
    private float[] array;

    [SerializeField]
    private TMP_Text fpsText;

    private float highestFps;
    private float lowestfps;
    private float currentfps;

    private void Awake()
    {
        array = new float[100];
        highestFps = 0;
        lowestfps = 1000;
        
    }

    private void Update()
    {
        array[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % array.Length;
        fpsText.text = Mathf.RoundToInt(GetFps()).ToString() + " fps";
    }

    private float GetFps()
    {
        float total = 0f;
        foreach (float time in array)
        {
            total += time;
        }
        return array.Length / total;
    }
}
