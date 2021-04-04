using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Instrument
{
    public double volume;
    public EnvelopeADSR envelope;

    public OscillatorModParams[] oscillators;

    private Oscillator oscillator;
    
    private List<int> noteIDs;
    private List<Note> notes;
    private List<double> notesStartTime;
    private List<double> notesEndTime;

    public void Init(uint seed)
    {
        oscillator = new Oscillator(seed);

        noteIDs = new List<int>();
        notes = new List<Note>();
        notesStartTime = new List<double>();
        notesEndTime = new List<double>();
    }

    public double Play(double time)
    {
        double finalSound = 0;

        int index = 0;
        while (index < notes.Count)
        {
            double sound = PlayNote(index, time);

            if (notesEndTime[index] > 0 && notesEndTime[index] + envelope.releaseTime <= time)
            {
                noteIDs.RemoveAtSwapBack(index);
                notes.RemoveAtSwapBack(index);
                notesStartTime.RemoveAtSwapBack(index);
                notesEndTime.RemoveAtSwapBack(index);
            }
            else
            {
                finalSound += sound;
                index++;
            }
        }
            
        return volume * finalSound;
    }

    public void NoteOn(int id)
    {
        int index = noteIDs.FindIndex(x => x == id);
        if (index == -1)
        {
            noteIDs.Add(id);
            notes.Add(new Note(id));
            notesStartTime.Add(AudioSettings.dspTime);
            notesEndTime.Add(0);
        }
        else
        {
            notesStartTime[index] = AudioSettings.dspTime;
            notesEndTime[index] = 0;
        }
    }

    public void NoteOff(int id)
    {
        int index = noteIDs.FindIndex(x => x == id);
        if (index == -1)
        {
            Debug.LogWarning(string.Format("The note {0} doesn't exist in the list.", id));
        }
        else
        {
            notesEndTime[index] = AudioSettings.dspTime;
        }
    }

    private double PlayNote(int index, double time)
    {
        double sound = 0;
        for (int i = 0; i < oscillators.Length; i++)
        {
            sound += oscillators[i].weigth * oscillator.GetValue(time, notes[index].frequency, oscillators[i].osParams);
        }

        //We divide by the total weight to noramlized the value between -1 and 1
        //For some reason OnAudioFilterRead, I don't hear a difference.
        //So we could remove the totalWeight if we can confirm it isn't needed
        return envelope.GetAmplitude(time, notesStartTime[index], notesEndTime[index]) * sound;
    }
}

[System.Serializable]
public struct OscillatorModParams
{
    public double weigth;
    public OscillatorParams osParams;
}