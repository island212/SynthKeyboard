using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

[RequireComponent(typeof(AudioSource), typeof(PlayerInput))]
public class SynthKeyboard : MonoBehaviour
{
    public InstrumentSO _instrumentSelected;

    private double _timeSteps;

    private PlayerInput _input;

    void Start()
    {
        if (_instrumentSelected == null)
        {
            Debug.LogError("No instrument selected.");
        }
        else
        {
            _timeSteps = 1.0 / AudioSettings.outputSampleRate;
            _instrumentSelected.instrument.Init(1234);

            _input = GetComponent<PlayerInput>();
            _input.onActionTriggered += ReadAction;

            GetComponent<AudioSource>().Play();
        }

        //Debug.LogFormat("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}",
        //    Octave4.C, Octave4.Db, Octave4.D, Octave4.Eb, Octave4.E, Octave4.F, Octave4.Gb,
        //    Octave4.G, Octave4.Ab, Octave4.A, Octave4.Bb, Octave4.B, Octave5.C);
    }

    private void ReadAction(InputAction.CallbackContext context)
    {
        if (IsValidInputNote(context.action.name))
        {
            int noteID = GetNoteIDByName(context.action.name);

            if (context.started)
            {
                _instrumentSelected.instrument.NoteOn(noteID);
            }
            else if (context.canceled)
            {
                _instrumentSelected.instrument.NoteOff(noteID);
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        switch (channels)
        {
            case 1:
                UpdateOneChannel(data);
                break;
            case 2:
                UpdateTwoChannel(data);
                break;
            default:
                UpdateGeneric(data, channels);
                break;
        }
    }

    #region Update Channels

    private void UpdateOneChannel(float[] data)
    {
        double time = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (float)_instrumentSelected.instrument.Play(time);

            time += _timeSteps;
        }
    }

    private void UpdateTwoChannel(float[] data)
    {
        double time = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i+=2)
        {
            float wave = (float)_instrumentSelected.instrument.Play(time);

            data[i] = wave;
            data[i + 1] = wave;

            time += _timeSteps;
        }
    }

    private void UpdateGeneric(float[] data, int channels)
    {
        double time = AudioSettings.dspTime;

        int dataLen = data.Length / channels;
        for (int i = 0; i < dataLen; i++)
        {
            float wave = (float)_instrumentSelected.instrument.Play(time);
            for (int j = 0; j < channels; j++)
                data[i + j] = wave;

            time += _timeSteps;
        }
    }

    #endregion

    private int GetNoteIDByName(string name) => name switch
    {
        "C" => OctaveID3.C,
        "Db" => OctaveID3.Db,
        "D" => OctaveID3.D,
        "Eb" => OctaveID3.Eb,
        "E" => OctaveID3.E,
        "F" => OctaveID3.F,
        "Gb" => OctaveID3.Gb,
        "G" => OctaveID3.G,
        "Ab" => OctaveID3.Ab,
        "A" => OctaveID3.A,
        "Bb" => OctaveID3.Bb,
        "B" => OctaveID3.B,
        "C1" => OctaveID4.C,
        _ => -1,
    };

    private bool IsValidInputNote(string name)
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
                return true;
            default:
                return false;
        }
    }
}