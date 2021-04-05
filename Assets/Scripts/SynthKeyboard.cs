using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = Unity.Mathematics.Random;

[RequireComponent(typeof(AudioSource), typeof(PlayerInput))]
public class SynthKeyboard : MonoBehaviour
{
    public InstrumentSO instrumentSelected;

    private PlayerInput input;
    private InstrumentSystem instrumentSystem;

    private int instrumentID;

    void Start()
    {
        if (instrumentSelected == null)
        {
            Debug.LogError("No instrument selected.");
        }
        else
        {
            instrumentSystem = new InstrumentSystem();
            instrumentSystem.Init();

            instrumentID = instrumentSystem.AddInstrument(instrumentSelected.instrument);

            input = GetComponent<PlayerInput>();
            input.onActionTriggered += ReadAction;

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
                instrumentSystem.NoteOn(instrumentID, noteID);
            }
            else if (context.canceled)
            {
                instrumentSystem.NoteOff(instrumentID, noteID);
            }
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        instrumentSystem.OnAudioFilterRead(data, channels);
    }

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