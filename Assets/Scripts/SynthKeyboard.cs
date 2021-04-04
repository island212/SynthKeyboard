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
        double time = AudioSettings.dspTime;

        int dataLen = data.Length / channels;
        for (int i = 0; i < dataLen; i++)
        {
            double wave = _instrumentSelected.instrument.Play(time, _note);
            for (int j = 0; j < channels; j++)
                data[i + j] = (float)wave;

            time += _timeSteps;
        }
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