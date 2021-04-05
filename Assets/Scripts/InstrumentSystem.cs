using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public struct PlayInstrumentJob
{
    public Instrument instrument;

    public Oscillator oscillator;

    public List<int> noteIDs;
    public List<Note> notes;
    public List<double> notesStartTime;
    public List<double> notesEndTime;

    public double timeSteps;

    public void ExecuteJob(double time, float[] data, int channels)
    {
        switch (channels)
        {
            case 1:
                UpdateOneChannel(time, data);
                break;
            case 2:
                UpdateTwoChannel(time, data);
                break;
            default:
                UpdateGeneric(time, data, channels);
                break;
        }
    }

    private void UpdateOneChannel(double time, float[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] += (float)Play(time);

            time += timeSteps;
        }
    }

    private void UpdateTwoChannel(double time, float[] data)
    {
        for (int i = 0; i < data.Length; i += 2)
        {
            float wave = (float)Play(time);

            data[i] += wave;
            data[i + 1] += wave;

            time += timeSteps;
        }
    }

    private void UpdateGeneric(double time, float[] data, int channels)
    {
        int dataLen = data.Length / channels;
        for (int i = 0; i < dataLen; i++)
        {
            float wave = (float)Play(time);
            for (int j = 0; j < channels; j++)
                data[i + j] += wave;

            time += timeSteps;
        }
    }

    private double Play(double time)
    {
        double finalSound = 0;

        int index = 0;
        while (index < notes.Count)
        {
            double sound = PlayNote(index, time);

            if (notesEndTime[index] > 0 && notesEndTime[index] + instrument.envelope.releaseTime <= time)
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

        return instrument.volume * finalSound;
    }

    private double PlayNote(int index, double time)
    {
        double sound = 0;
        for (int i = 0; i < instrument.oscillators.Length; i++)
        {
            sound += instrument.oscillators[i].weigth * oscillator.GetValue(time, notes[index].frequency, instrument.oscillators[i].osParams);
        }

        //We divide by the total weight to noramlized the value between -1 and 1
        //For some reason OnAudioFilterRead, I don't hear a difference.
        //So we could remove the totalWeight if we can confirm it isn't needed
        return instrument.envelope.GetAmplitude(time, notesStartTime[index], notesEndTime[index]) * sound;
    }
}

public struct HandleNoteJob
{
    public List<int> noteIDs;
    public List<Note> notes;
    public List<double> notesStartTime;
    public List<double> notesEndTime;

    public List<NoteRequest> requestOn;
    public List<NoteRequest> requestOff;

    public void ExecuteJob()
    {
        HandleRequestOn();
        HandleRequestOff();

        requestOn.Clear();
        requestOff.Clear();
    }

    private void HandleRequestOn()
    {
        foreach (var request in requestOn)
        {
            int index = noteIDs.FindIndex(x => x == request.id);
            if (index == -1)
            {
                noteIDs.Add(request.id);
                notes.Add(new Note(request.id));
                notesStartTime.Add(request.time);
                notesEndTime.Add(0);
            }
            else
            {
                notesStartTime[index] = request.time;
                notesEndTime[index] = 0;
            }
        }
    }

    private void HandleRequestOff()
    {
        foreach (var request in requestOn)
        {
            int index = noteIDs.FindIndex(x => x == request.id);
            if (index == -1)
            {
                Debug.LogWarning(string.Format("The note {0} doesn't exist in the list.", request.id));
            }
            else
            {
                notesEndTime[index] = request.time;
            }
        }
    }
}

public struct InstrumentsData
{
    public List<int> noteIDs;
    public List<Note> notes;
    public List<double> notesStartTime;
    public List<double> notesEndTime;

    public object requestOnMutex;
    public object requestOffMutex;
    public List<NoteRequest> requestOn;
    public List<NoteRequest> requestOff;
}

public class InstrumentSystem
{
    private double timeSteps;

    private List<InstrumentsData> instrumentsDatas;
    private List<HandleNoteJob> handleNoteJobs;
    private List<PlayInstrumentJob> playInstrumentJobs;

    public void Init()
    {
        timeSteps = 1.0 / AudioSettings.outputSampleRate;

        instrumentsDatas = new List<InstrumentsData>();
        handleNoteJobs = new List<HandleNoteJob>();
        playInstrumentJobs = new List<PlayInstrumentJob>();
    }

    public int AddInstrument(Instrument instrument)
    {
        int index = instrumentsDatas.Count;
        instrumentsDatas.Add(new InstrumentsData
        {
            noteIDs = new List<int>(),
            notes = new List<Note>(),
            notesEndTime = new List<double>(),
            notesStartTime = new List<double>(),

            requestOnMutex = new Mutex(),
            requestOffMutex = new Mutex(),
            requestOn = new List<NoteRequest>(),
            requestOff = new List<NoteRequest>()
        });

        handleNoteJobs.Add(new HandleNoteJob 
        {
            noteIDs = instrumentsDatas[index].noteIDs,
            notes = instrumentsDatas[index].notes,
            notesStartTime = instrumentsDatas[index].notesStartTime,
            notesEndTime = instrumentsDatas[index].notesEndTime,

            requestOn = new List<NoteRequest>(),
            requestOff = new List<NoteRequest>()
        });

        playInstrumentJobs.Add(new PlayInstrumentJob 
        {
            timeSteps = timeSteps,

            instrument = instrument,
            oscillator = new Oscillator(1234),

            noteIDs = instrumentsDatas[index].noteIDs,
            notes = instrumentsDatas[index].notes,
            notesStartTime = instrumentsDatas[index].notesStartTime,
            notesEndTime = instrumentsDatas[index].notesEndTime
        });

        return index;
    }

    public void NoteOn(int instrumentID, int noteID)
    {
        lock (instrumentsDatas[instrumentID].requestOnMutex)
        {
            instrumentsDatas[instrumentID].requestOn.Add(new NoteRequest { id = noteID, time = AudioSettings.dspTime });
        }    
    }

    public void NoteOff(int instrumentID, int noteID)
    {
        lock (instrumentsDatas[instrumentID].requestOnMutex)
        {
            instrumentsDatas[instrumentID].requestOff.Add(new NoteRequest { id = noteID, time = AudioSettings.dspTime });
        }
    }

    public void OnAudioFilterRead(float[] data, int channels)
    {
        double time = AudioSettings.dspTime;

        for (int i = 0; i < handleNoteJobs.Count; i++)
        {
            //Swap the requestOn buffer
            lock (instrumentsDatas[i].requestOnMutex)
            {
                var instrumentData = instrumentsDatas[i];
                var handleNoteJob = handleNoteJobs[i];

                var tempSwap = instrumentData.requestOn;
                instrumentData.requestOn = handleNoteJob.requestOff;
                handleNoteJob.requestOn = tempSwap;

                instrumentsDatas[i] = instrumentData;
                handleNoteJobs[i] = handleNoteJob;
            }

            //Swap the requestOff buffer
            lock (instrumentsDatas[i].requestOffMutex)
            {
                var instrumentData = instrumentsDatas[i];
                var handleNoteJob = handleNoteJobs[i];

                var tempSwap = instrumentData.requestOff;
                instrumentData.requestOff = handleNoteJob.requestOff;
                handleNoteJob.requestOff = tempSwap;

                instrumentsDatas[i] = instrumentData;
                handleNoteJobs[i] = handleNoteJob;
            }

            handleNoteJobs[i].ExecuteJob();
        }

        for (int i = 0; i < playInstrumentJobs.Count; i++)
        {
            playInstrumentJobs[i].ExecuteJob(time, data, channels);
        }
    }
}
