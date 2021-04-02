using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineTest : MonoBehaviour
{
    [Range(1, 20000)]  //Creates a slider in the inspector
    public float frequency = 100;

    [Range(0, 1f)]
    public float amplitude = 1f;

    public float sampleRate = 44100;

    float phase = 0;

    // ...I cut the rest of the functions because they are mostly unchanged...

    private void Start()
    {
        GetComponent<AudioSource>().Play();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            phase += 2 * Mathf.PI * frequency / sampleRate;

            data[i] = amplitude * Mathf.Sin(phase);

            if (phase >= 2 * Mathf.PI)
            {
                phase -= 2 * Mathf.PI;
            }
        }
    }

}
