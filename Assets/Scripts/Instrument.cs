using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Instrument
{
    public double volume;
    public EnvelopeADSR envelope;

    public OscillatorModParams[] oscillators;

    private Oscillator oscillator;

    public void Init(uint seed)
    {
        oscillator = new Oscillator(seed);
    }

    public double Play(double time, Note note)
    {
        double sound = 0, totalWeight = 0;
        for (int i = 0; i < oscillators.Length; i++)
        {
            totalWeight += oscillators[i].weigth;
            sound += oscillator.GetValue(time, note, oscillators[i].osParams);
        }

        return envelope.GetAmplitude(time) * (sound / totalWeight);
    }
}

[System.Serializable]
public struct OscillatorModParams
{
    public double weigth;
    public OscillatorParams osParams;
}