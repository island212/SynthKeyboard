using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[System.Serializable]
public struct Instrument
{
    public double volume;
    public EnvelopeADSR envelope;

    public OscillatorModParams[] oscillators;
}

public struct NoteRequest
{
    public int id;
    public double time;
}

[System.Serializable]
public struct OscillatorModParams
{
    public double weigth;
    public OscillatorParams osParams;
}