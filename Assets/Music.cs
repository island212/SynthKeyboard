using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public enum WaveType { Sin, Square, Triangle, Saw }

public enum NoteType { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }

public static class Music
{
    /// <summary>
    /// PI multiply by 2
    /// </summary>
    public const double PI2 = 2.0 * math.PI_DBL;

    public const double TWO_DIVBY_PI = 2.0 / math.PI_DBL;

    public const double HALF_PI = math.PI_DBL / 2.0;

    public static double GetFrequency(NoteType type, int octave) => type switch
    {
        NoteType.C => Octave0.C * math.pow(2, octave),
        NoteType.Db => Octave0.Db * math.pow(2, octave),
        NoteType.D => Octave0.D * math.pow(2, octave),
        NoteType.Eb => Octave0.Eb * math.pow(2, octave),
        NoteType.E => Octave0.E * math.pow(2, octave),
        NoteType.F => Octave0.F * math.pow(2, octave),
        NoteType.Gb => Octave0.Gb * math.pow(2, octave),
        NoteType.G => Octave0.G * math.pow(2, octave),
        NoteType.Ab => Octave0.Ab * math.pow(2, octave),
        NoteType.A => Octave0.A * math.pow(2, octave),
        NoteType.Bb => Octave0.Bb * math.pow(2, octave),
        NoteType.B => Octave0.B * math.pow(2, octave),
        _ => throw new System.ArgumentException(string.Format("The NoteType {0} is invalid", type))
    };

    /// <summary>
    /// Convert frequency (Hz) to angular velocity
    /// </summary>
    /// <param name="frequency">frequency (Hz)</param>
    /// <returns>angular velocity</returns>
    public static double FrequencyToAngular(double frequency) => PI2 * frequency;
}

public static class Oscillator
{
    public static double GetValue(WaveType type, double phase) => type switch
    {
        WaveType.Sin => SinWave(phase),
        WaveType.Square => SqrWave(phase),
        WaveType.Triangle => TriangleWave(phase),
        WaveType.Saw => SawWave(phase),
        _ => throw new System.ArgumentException(string.Format("The WaveType {0} is invalid", type)),
    };

    public static double SqrWave(double phase)
    {
        double sinWave = math.sin(phase);
        return math.sign(sinWave);
    }

    public static double SinWave(double phase)
    {
        return math.sin(phase);
    }

    public static double TriangleWave(double phase)
    {
        return math.asin(math.sin(phase) * Music.TWO_DIVBY_PI);
    }

    public static double SawWave(double phase)
    {
        //Only work if the phase is between 0 and 2pi.
        return phase / math.PI_DBL - 1;
    }
}

public struct Note
{
    /// <summary>
    /// Represent the twelth root of 2.
    /// Formula pow(2.0, 1.0 / 12.0)
    /// </summary>
    public const double OCT12TH = 1.059463094359;

    /// <summary>
    /// Represent 1 divide by the twelth root of 2.
    /// Formula 1.0 / pow(2.0, 1.0 / 12.0)
    /// </summary>
    public const double OCT12THDIV = 1.0 / OCT12TH;

    public double frequency;

    public Note(double frequency)
    {
        this.frequency = frequency;
    }
}

public static class Octave0
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 27.5;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave1
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 55.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave2
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 110.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave3
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 220.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave4
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 440.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave5
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 880.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave6
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 1760.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave7
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 3520.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave8
{
    public const double C = Db * Note.OCT12THDIV;
    public const double Db = D * Note.OCT12THDIV;
    public const double D = Eb * Note.OCT12THDIV;
    public const double Eb = E * Note.OCT12THDIV;
    public const double E = F * Note.OCT12THDIV;
    public const double F = Gb * Note.OCT12THDIV;
    public const double Gb = G * Note.OCT12THDIV;
    public const double G = Ab * Note.OCT12THDIV;
    public const double Ab = A * Note.OCT12THDIV;
    public const double A = 7040.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}
