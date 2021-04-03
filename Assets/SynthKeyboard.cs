using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource), typeof(PlayerInput))]
public class SynthKeyboard : MonoBehaviour
{
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

        //Debug.LogFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
        //    Octave4.C, Octave4.Db, Octave4.D, Octave4.Eb, Octave4.E, Octave4.F, Octave4.Gb,
        //    Octave4.G, Octave4.Ab, Octave4.A, Octave4.Bb, Octave4.B, Octave5.C);
    }

    private void ReadAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _note = GetNoteByName(context.action.name);

            SetGainIfName(context.action.name, 1f);
        }
        else if (context.canceled)
        {
            SetGainIfName(context.action.name, 0f);
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double phase = _phase;

        int dataLen = data.Length / channels;
        for (int i = 0; i < dataLen; i++)
        {
            double wave = _gain * _settings.amplitude * Oscillator.GetValue(WaveType.Saw, phase);
            for (int j = 0; j < channels; j++)
                data[i + j] = (float)wave;

            //We need to % PI2 to keep the value between 0 and 2pi so it doesn't overflow.
            //Anyway the saw wave need the phase to be between 0 and 2pi to work.
            phase = (phase + Music.FrequencyToAngular(_note.frequency) / _sampleRate) % Music.PI2;
        }

        _phase = phase;
    }

    private Note GetNoteByName(string name) => name switch
    {
        "C" => new Note(Octave4.C),
        "Db" => new Note(Octave4.Db),
        "D" => new Note(Octave4.D),
        "Eb" => new Note(Octave4.Eb),
        "E" => new Note(Octave4.E),
        "F" => new Note(Octave4.F),
        "Gb" => new Note(Octave4.Gb),
        "G" => new Note(Octave4.G),
        "Ab" => new Note(Octave4.Ab),
        "A" => new Note(Octave4.A),
        "Bb" => new Note(Octave4.Bb),
        "B" => new Note(Octave4.B),
        "C1" => new Note(Octave5.C),
        _ => new Note(),
    };

    private void SetGainIfName(string name, float value)
    {
        switch (name)
        {
            case "C":
            case "Db":
            case "D":
            case "Eb":
            case "E":
            case "F":
            case "Gb":
            case "G":
            case "Ab":
            case "A":
            case "Bb":
            case "B":
            case "C1":
                _gain = value;
                break;
        }
    }
}

[System.Serializable]
public struct WaveSettings
{
    [Range(0, 1)]
    public float amplitude;
}