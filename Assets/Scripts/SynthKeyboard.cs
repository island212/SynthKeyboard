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

    private Note _note;

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
            if (context.started)
            {
                _note = GetNoteByName(context.action.name);

                _instrumentSelected.instrument.envelope.NoteOn(AudioSettings.dspTime);
            }
            else if (context.canceled)
            {
                _instrumentSelected.instrument.envelope.NoteOff(AudioSettings.dspTime);
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
            data[i] = (float)_instrumentSelected.instrument.Play(time, _note);

            time += _timeSteps;
        }
    }

    private void UpdateTwoChannel(float[] data)
    {
        double time = AudioSettings.dspTime;

        for (int i = 0; i < data.Length; i+=2)
        {
            float wave = (float)_instrumentSelected.instrument.Play(time, _note);

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
            float wave = (float)_instrumentSelected.instrument.Play(time, _note);
            for (int j = 0; j < channels; j++)
                data[i + j] = wave;

            time += _timeSteps;
        }
    }

    #endregion

    private Note GetNoteByName(string name) => name switch
    {
        "C" => new Note(Octave3.C),
        "Db" => new Note(Octave3.Db),
        "D" => new Note(Octave3.D),
        "Eb" => new Note(Octave3.Eb),
        "E" => new Note(Octave3.E),
        "F" => new Note(Octave3.F),
        "Gb" => new Note(Octave3.Gb),
        "G" => new Note(Octave3.G),
        "Ab" => new Note(Octave3.Ab),
        "A" => new Note(Octave3.A),
        "Bb" => new Note(Octave3.Bb),
        "B" => new Note(Octave3.B),
        "C1" => new Note(Octave4.C),
        _ => new Note(),
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