using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

public enum WaveType { Sin, Square, Triangle, Saw, Random }

public enum NoteType { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B }

public static class Music
{
    /// <summary>
    /// PI multiply by 2
    /// </summary>
    public const double TAU = 2.0 * math.PI_DBL;

    public const double FRACT_2_PI = 2.0 / math.PI_DBL;

    public const double FRACT_PI_2 = math.PI_DBL / 2.0;

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
}

public struct Oscillator
{
    public Random random;

    public Oscillator(uint seed)
    {
        random = new Random(seed);
    }

    /// <summary>
    /// Create a wave sound from a phase between 0 and 2pi and the wave type
    /// </summary>
    /// <param name="type">The wave type</param>
    /// <param name="phase">The phase</param>
    /// <returns>The wave value</returns>
    public double GetValue(double time, Note note, WaveType type, double LFOAmplitude = 0.0, Note LFONote = default)
    {
        double phase = note.angular * time;
        //Add a low frequency oscillator for frequency modulation if needed
        phase += LFOAmplitude * LFONote.angular * math.sin(LFONote.angular * time);

        return type switch
        {
            WaveType.Sin => SinWave(phase),
            WaveType.Square => SqrWave(phase),
            WaveType.Triangle => TriangleWave(phase),
            WaveType.Saw => SawWave(phase),
            WaveType.Random => RandomWave(ref random),
            _ => throw new System.ArgumentException(string.Format("The WaveType {0} is invalid", type)),
        };
    }

    public double GetValue(double time, Note note, OscillatorParams osParams)
    {
        return GetValue(time, note.Mul(osParams.multiply), osParams.type, osParams.LFOAmplitude, new Note(osParams.LFOFrequency));
    }

    public static double SinWave(double phase)
    {
        return math.sin(phase);
    }

    public static double SqrWave(double phase)
    {
        double sinWave = math.sin(phase);
        return math.sign(sinWave);
    }

    public static double TriangleWave(double phase)
    {
        return math.asin(math.sin(phase) * Music.FRACT_2_PI);
    }

    public static double SawWave(double phase)
    {
        //Only work if the phase is between 0 and 2pi.
        return (phase % Music.TAU) / math.PI_DBL - 1;
    }

    public static double RandomWave(ref Random random)
    {
        return random.NextDouble() * 2 - 1;
    }
}

[System.Serializable]
public struct OscillatorParams
{
    public double multiply;
    public WaveType type;
    public double LFOAmplitude;
    public double LFOFrequency;
}

public struct Note
{
    /// <summary>
    /// Represent the twelth root of two.
    /// Formula pow(2.0, 1.0 / 12.0)
    /// </summary>
    public const double OCT12TH = 1.059463094359;

    /// <summary>
    /// Represent one divide by the twelth root of two.
    /// Formula 1.0 / pow(2.0, 1.0 / 12.0)
    /// </summary>
    public const double OCT12TH_DIV = 1.0 / OCT12TH;

    public double frequency;
    public double angular;

    public Note(double frequency)
    {
        this.frequency = frequency;
        angular = Music.TAU * frequency;
    }

    public Note Mul(double multiply)
    {
        return new Note(frequency * multiply);
    }
}

[System.Serializable]
public struct EnvelopeADSR
{
    public static EnvelopeADSR simple = new EnvelopeADSR(0.01, 0.01, 0.02, 1.0, 0.8);

    public double attackTime;
    public double decayTime;
    public double releaseTime;

    public double startAmplitude;
    public double sustainAmplitude;

    private double startTime;
    private double endTime;

    public EnvelopeADSR(double attackTime, double decayTime, double releaseTime, double startAmplitude, double sustainAmplitude)
    {
        this.attackTime = attackTime;
        this.decayTime = decayTime;
        this.releaseTime = releaseTime;

        this.startAmplitude = startAmplitude;
        this.sustainAmplitude = sustainAmplitude;

        this.startTime = 0.0;
        this.endTime = 0.0;
    }

    public void NoteOn(double time)
    {
        startTime = time;
        endTime = 0;
    }

    public void NoteOff(double time)
    {
        endTime = time;
    }

    public double GetAmplitude(double time)
    {
        if (startTime <= 0)
            return 0;

        double deltaTime = time - startTime;

        double amplitude;
        if (endTime <= 0)
        {
            if (deltaTime <= attackTime)
            {
                amplitude = (deltaTime / attackTime) * startAmplitude;
            }
            else if (deltaTime <= attackTime + decayTime)
            {
                amplitude = startAmplitude + ((deltaTime - attackTime) / decayTime) * (sustainAmplitude - startAmplitude);
            }
            else
            {
                amplitude = sustainAmplitude;
            }
        }
        else if(deltaTime <= endTime + releaseTime)
        {
            amplitude = ((time - endTime) / releaseTime) * -sustainAmplitude + sustainAmplitude;
        }
        else
        {
            amplitude = 0;
        }

        //The amplitude value could be a value really close to 0 but not exactly.
        //In this case, we don't want to get any signal so we put the amplitude at 0
        if (amplitude < 0.0001)
        {
            amplitude = 0;
        }

        return amplitude;
    }
}

public static class Octave0
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 27.5;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave1
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 55.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave2
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 110.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave3
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 220.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave4
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 440.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave5
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 880.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave6
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 1760.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave7
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 3520.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}

public static class Octave8
{
    public const double C = Db * Note.OCT12TH_DIV;
    public const double Db = D * Note.OCT12TH_DIV;
    public const double D = Eb * Note.OCT12TH_DIV;
    public const double Eb = E * Note.OCT12TH_DIV;
    public const double E = F * Note.OCT12TH_DIV;
    public const double F = Gb * Note.OCT12TH_DIV;
    public const double Gb = G * Note.OCT12TH_DIV;
    public const double G = Ab * Note.OCT12TH_DIV;
    public const double Ab = A * Note.OCT12TH_DIV;
    public const double A = 7040.0;
    public const double Bb = A * Note.OCT12TH;
    public const double B = A * Note.OCT12TH;
}
