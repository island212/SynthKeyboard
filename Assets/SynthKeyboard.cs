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

    private Note _note;
    private float _gain;

    void Start()
    {
        _sampleRate = AudioSettings.outputSampleRate;

        _random = new System.Random();

        _input = GetComponent<PlayerInput>();
        _input.onActionTriggered += ReadAction;

        GetComponent<AudioSource>().Play();
    }

    private void ReadAction(InputAction.CallbackContext context)
    {
        Debug.Log(context.action.name);

        if (context.started)
        {
            switch (context.action.name)
            {
                case "C":
                case "D":
                case "E":
                case "F":
                case "G":
                case "A":
                case "B":
                    _gain = 1f;
                    break;
            }

            switch (context.action.name)
            {
                case "C": _note = new Note(261.63f); break;
                case "D": _note = new Note(293.66f); break;
                case "E": _note = new Note(329.63f); break;
                case "F": _note = new Note(349.23f); break;
                case "G": _note = new Note(392.00f); break;
                case "A": _note = new Note(440.00f); break;
                case "B": _note = new Note(493.88f); break;
            }
        }
        else if (context.canceled)
        {
            switch (context.action.name)
            {
                case "C":
                case "D":
                case "E":
                case "F":
                case "G":
                case "A":
                case "B":
                    _gain = 0f;
                    break;
            }
        }
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

        int dataLen = data.Length / channels;
        for (int i = 0; i < dataLen; i++)
        {
            phase += PI2 * _note.frequency / _sampleRate;
            double wave = _gain * _settings.amplitude * SqrWave(phase);
            for (int j = 0; j < channels; j++)
                data[i + j] = (float)wave;
        }

        _phase = phase % PI2;
    }
}

public struct Note 
{
    public float frequency;

    public Note(float frequency)
    {
        this.frequency = frequency;
    }
}

[System.Serializable]
public struct WaveSettings
{
    [Range(0, 1)]
    public float amplitude;
}