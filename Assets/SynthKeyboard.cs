using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource), typeof(PlayerInput))]
public class SynthKeyboard : MonoBehaviour
{
    private const double PI2 = 2 * math.PI_DBL;

    public WaveSettings _settings;

    private int _sampleRate;

    private System.Random _random;
    private PlayerInput _input;

    private double _phase;

    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;

        _random = new System.Random();

        _input = GetComponent<PlayerInput>();
        _input.onActionTriggered += ReadAction;
        _input.actions.FindAction("C");

        GetComponent<AudioSource>().Play();
    }

    private void ReadAction(InputAction.CallbackContext context)
    {
        //context
    }

    private double SqrWave(double phase)
    {
        double sinWave = math.sin(phase);
        return math.sign(sinWave);
    }

    private double SinWave(double phase)
    {
        return math.sin(phase);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double phase = _phase;

        WaveSettings settings = _settings;
        double angularFrequency = PI2 * settings.frequency / _sampleRate;

        int dataLen = data.Length / channels;

        for (int i = 0; i < dataLen; i++)
        {
            phase += angularFrequency;
            double wave = settings.amplitude * SqrWave(phase);
            for (int j = 0; j < channels; j++)
                data[i + j] = (float)wave;
        }

        _phase = phase % PI2;
    }
}

[System.Serializable]
public struct WaveSettings
{
    [Range(0, 1)]
    public float amplitude;

    [Range(0, 22000)]
    public int frequency;
}