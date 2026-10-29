using System;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    Jump,
    Swim,
    Coin,
    Stomp,
    PowerUp,
    Damage,
    Death,
    Bump,
    OneUp,
    Flag,
    Axe,
    Fireball,
    BossHit,
    MenuMove,
    Select,
    LevelClear,
}

/* Sintetizador 8 bits: genera toda la musica y los efectos en tiempo de ejecucion,
   sin archivos de audio externos. */
public static class SoundSynth
{
    public const int SampleRate = 44100;

    private enum Wave
    {
        Square,
        Triangle,
        Saw,
        Noise,
    }

    private static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    private static int MidiToFreq(int note)
    {
        return Mathf.RoundToInt(440f * Mathf.Pow(2f, (note - 69) / 12f));
    }

    private static float WaveValue(Wave wave, float phase, float duty, System.Random noise)
    {
        float p = phase - Mathf.Floor(phase);

        switch (wave)
        {
            case Wave.Square:
                return p < duty ? 1f : -1f;

            case Wave.Triangle:
                return 4f * Mathf.Abs(p - 0.5f) - 1f;

            case Wave.Saw:
                return 2f * p - 1f;

            case Wave.Noise:
                return (float)(noise.NextDouble() * 2.0 - 1.0);
        }

        return 0f;
    }

    private static float Envelope(int sample, int length, float attackSamples, float releaseSamples, float decay)
    {
        float env = 1f;

        if (sample < attackSamples) {
            env = sample / Mathf.Max(1f, attackSamples);
        }

        int releaseStart = length - Mathf.RoundToInt(releaseSamples);

        if (sample > releaseStart) {
            env = Mathf.Min(env, Mathf.Max(0f, (length - sample) / Mathf.Max(1f, releaseSamples)));
        }

        if (decay > 0f) {
            env *= Mathf.Lerp(1f, 0.35f, (float)sample / Mathf.Max(1, length) * decay);
        }

        return env;
    }

    private static void AddTone(float[] buffer, float startSeconds, float duration, float startFreq, float endFreq,
        Wave wave, float amplitude, float duty = 0.5f, float attack = 0.004f, float release = 0.02f, float decay = 0.6f)
    {
        int start = Mathf.RoundToInt(startSeconds * SampleRate);
        int length = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        double phase = 0.0;
        System.Random noise = new System.Random(1234);

        for (int i = 0; i < length; i++)
        {
            int index = start + i;

            if (index < 0 || index >= buffer.Length) {
                continue;
            }

            float t = (float)i / length;
            float freq = Mathf.Lerp(startFreq, endFreq, t);

            phase += freq / SampleRate;

            float env = Envelope(i, length, attack * SampleRate, release * SampleRate, decay);
            buffer[index] += WaveValue(wave, (float)phase, duty, noise) * amplitude * env;
        }
    }

    private static void AddNoise(float[] buffer, float startSeconds, float duration, float amplitude, float smooth = 0.6f)
    {
        int start = Mathf.RoundToInt(startSeconds * SampleRate);
        int length = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        System.Random noise = new System.Random(4321);
        float value = 0f;

        for (int i = 0; i < length; i++)
        {
            int index = start + i;

            if (index < 0 || index >= buffer.Length) {
                continue;
            }

            float target = (float)(noise.NextDouble() * 2.0 - 1.0);
            value = Mathf.Lerp(value, target, 1f - smooth);

            float env = 1f - (float)i / length;
            buffer[index] += value * amplitude * env * env;
        }
    }

    private static AudioClip Build(string name, float[] samples, bool loop)
    {
        if (cache.TryGetValue(name, out AudioClip cached) && cached != null) {
            return cached;
        }

        // normaliza para evitar saturacion
        float peak = 0.0001f;

        for (int i = 0; i < samples.Length; i++) {
            peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
        }

        float scale = peak > 0.98f ? 0.98f / peak : 1f;

        for (int i = 0; i < samples.Length; i++) {
            samples[i] = Mathf.Clamp(samples[i] * scale, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        cache[name] = clip;
        return clip;
    }

    /* ------------------------------- efectos -------------------------------- */

    public static AudioClip SfxClip(Sfx sfx)
    {
        float[] buffer;

        switch (sfx)
        {
            case Sfx.Jump:
                buffer = New(0.22f);
                AddTone(buffer, 0f, 0.20f, 330f, 880f, Wave.Square, 0.32f, 0.35f, 0.004f, 0.05f, 0.8f);
                AddTone(buffer, 0f, 0.12f, 660f, 1200f, Wave.Triangle, 0.10f, 0.5f, 0.002f, 0.05f, 0.9f);
                break;

            case Sfx.Swim:
                buffer = New(0.26f);
                AddTone(buffer, 0f, 0.24f, 260f, 620f, Wave.Triangle, 0.30f, 0.5f, 0.02f, 0.08f, 0.4f);
                AddNoise(buffer, 0f, 0.20f, 0.05f, 0.9f);
                break;

            case Sfx.Coin:
                buffer = New(0.34f);
                AddTone(buffer, 0f, 0.07f, 988f, 988f, Wave.Square, 0.28f, 0.5f, 0.002f, 0.01f, 0.2f);
                AddTone(buffer, 0.07f, 0.24f, 1319f, 1319f, Wave.Square, 0.26f, 0.5f, 0.002f, 0.12f, 0.4f);
                break;

            case Sfx.Stomp:
                buffer = New(0.22f);
                AddNoise(buffer, 0f, 0.16f, 0.25f, 0.35f);
                AddTone(buffer, 0f, 0.18f, 320f, 90f, Wave.Square, 0.30f, 0.45f, 0.002f, 0.05f, 0.9f);
                break;

            case Sfx.PowerUp:
                buffer = New(0.72f);
                int[] upNotes = { 72, 76, 79, 84, 88 };
                for (int i = 0; i < upNotes.Length; i++) {
                    AddTone(buffer, i * 0.075f, 0.09f, MidiToFreq(upNotes[i]), MidiToFreq(upNotes[i]), Wave.Square, 0.24f, 0.4f, 0.003f, 0.02f, 0.3f);
                }
                AddTone(buffer, 0.375f, 0.30f, MidiToFreq(88), MidiToFreq(92), Wave.Square, 0.26f, 0.45f, 0.01f, 0.15f, 0.6f);
                break;

            case Sfx.Damage:
                buffer = New(0.55f);
                int[] downNotes = { 76, 72, 69, 65, 62 };
                for (int i = 0; i < downNotes.Length; i++) {
                    AddTone(buffer, i * 0.075f, 0.10f, MidiToFreq(downNotes[i]), MidiToFreq(downNotes[i] - 1), Wave.Saw, 0.22f, 0.5f, 0.002f, 0.02f, 0.5f);
                }
                AddTone(buffer, 0.35f, 0.18f, 300f, 120f, Wave.Square, 0.20f, 0.5f, 0.004f, 0.08f, 0.9f);
                break;

            case Sfx.Death:
                buffer = New(1.35f);
                AddTone(buffer, 0f, 0.80f, 660f, 110f, Wave.Square, 0.30f, 0.5f, 0.004f, 0.10f, 0.2f);
                AddTone(buffer, 0f, 0.80f, 330f, 60f, Wave.Triangle, 0.14f, 0.5f, 0.004f, 0.15f, 0.1f);
                AddNoise(buffer, 0.75f, 0.35f, 0.12f, 0.55f);
                AddTone(buffer, 0.95f, 0.30f, 110f, 55f, Wave.Square, 0.20f, 0.5f, 0.004f, 0.20f, 0.8f);
                break;

            case Sfx.Bump:
                buffer = New(0.14f);
                AddTone(buffer, 0f, 0.12f, 200f, 120f, Wave.Square, 0.28f, 0.5f, 0.002f, 0.03f, 0.9f);
                AddNoise(buffer, 0f, 0.06f, 0.10f, 0.5f);
                break;

            case Sfx.OneUp:
                buffer = New(0.62f);
                int[] lifeNotes = { 76, 81, 84, 88, 91, 96 };
                for (int i = 0; i < lifeNotes.Length; i++) {
                    AddTone(buffer, i * 0.085f, 0.10f, MidiToFreq(lifeNotes[i]), MidiToFreq(lifeNotes[i]), Wave.Triangle, 0.28f, 0.5f, 0.003f, 0.02f, 0.4f);
                }
                break;

            case Sfx.Flag:
                buffer = New(0.9f);
                int[] flagNotes = { 72, 76, 79, 84, 79, 84, 88 };
                for (int i = 0; i < flagNotes.Length; i++) {
                    AddTone(buffer, i * 0.10f, 0.14f, MidiToFreq(flagNotes[i]), MidiToFreq(flagNotes[i]), Wave.Square, 0.22f, 0.35f, 0.003f, 0.03f, 0.4f);
                }
                break;

            case Sfx.Axe:
                buffer = New(0.7f);
                AddNoise(buffer, 0f, 0.18f, 0.35f, 0.15f);
                AddTone(buffer, 0f, 0.30f, 1400f, 700f, Wave.Square, 0.22f, 0.5f, 0.002f, 0.20f, 0.9f);
                AddTone(buffer, 0.15f, 0.45f, 220f, 70f, Wave.Square, 0.24f, 0.5f, 0.004f, 0.25f, 0.5f);
                break;

            case Sfx.Fireball:
                buffer = New(0.28f);
                AddNoise(buffer, 0f, 0.26f, 0.22f, 0.25f);
                AddTone(buffer, 0f, 0.24f, 700f, 180f, Wave.Triangle, 0.18f, 0.5f, 0.004f, 0.10f, 0.6f);
                break;

            case Sfx.BossHit:
                buffer = New(0.45f);
                AddNoise(buffer, 0f, 0.30f, 0.30f, 0.4f);
                AddTone(buffer, 0f, 0.40f, 260f, 70f, Wave.Saw, 0.28f, 0.45f, 0.002f, 0.20f, 0.7f);
                break;

            case Sfx.MenuMove:
                buffer = New(0.09f);
                AddTone(buffer, 0f, 0.07f, 880f, 880f, Wave.Square, 0.20f, 0.5f, 0.002f, 0.02f, 0.6f);
                break;

            case Sfx.Select:
                buffer = New(0.5f);
                AddTone(buffer, 0f, 0.10f, 660f, 660f, Wave.Square, 0.24f, 0.5f, 0.002f, 0.02f, 0.3f);
                AddTone(buffer, 0.08f, 0.10f, 880f, 880f, Wave.Square, 0.24f, 0.5f, 0.002f, 0.02f, 0.3f);
                AddTone(buffer, 0.16f, 0.28f, 1320f, 1320f, Wave.Square, 0.24f, 0.5f, 0.002f, 0.15f, 0.5f);
                break;

            case Sfx.LevelClear:
                buffer = New(1.6f);
                int[] clearNotes = { 72, 76, 79, 84, 88, 88, 88 };
                for (int i = 0; i < clearNotes.Length; i++) {
                    AddTone(buffer, i * 0.11f, i == clearNotes.Length - 1 ? 0.65f : 0.13f,
                        MidiToFreq(clearNotes[i]), MidiToFreq(clearNotes[i]), Wave.Square, 0.24f, 0.4f, 0.003f, 0.04f, 0.3f);
                    AddTone(buffer, i * 0.11f, i == clearNotes.Length - 1 ? 0.65f : 0.13f,
                        MidiToFreq(clearNotes[i] - 12), MidiToFreq(clearNotes[i] - 12), Wave.Triangle, 0.12f, 0.5f, 0.003f, 0.04f, 0.3f);
                }
                break;

            default:
                buffer = New(0.1f);
                break;
        }

        return Build($"sfx-{sfx}", buffer, false);
    }

    private static float[] New(float seconds)
    {
        return new float[Mathf.CeilToInt(seconds * SampleRate)];
    }

    /* -------------------------------- musica -------------------------------- */

    private static readonly (int note, float beats)[] MenuMelody =
    {
        (72, 1f), (76, 1f), (79, 1f), (76, 1f),
        (74, 1f), (77, 1f), (81, 1f), (77, 1f),
        (72, 1f), (76, 1f), (79, 1f), (84, 1.5f),
        (83, 0.5f), (79, 1f), (76, 1f), (72, 1f),
    };

    private static readonly (int note, float beats)[] OverworldMelody =
    {
        (76, 0.5f), (76, 0.5f), (0, 0.5f), (76, 0.5f), (0, 0.5f), (72, 0.5f), (76, 0.5f), (0, 0.5f),
        (79, 1f), (0, 1f), (67, 1f), (0, 1f),
        (72, 0.5f), (0, 0.5f), (67, 0.5f), (0, 0.5f), (64, 0.5f), (0, 0.5f), (69, 0.5f), (0, 0.5f),
        (71, 0.5f), (70, 0.5f), (69, 0.5f), (0, 0.5f), (67, 0.5f), (76, 0.5f), (79, 0.5f), (81, 0.5f),
        (77, 0.5f), (79, 0.5f), (0, 0.5f), (76, 0.5f), (72, 0.5f), (74, 0.5f), (71, 0.5f), (0, 1f),
    };

    private static readonly (int note, float beats)[] UndergroundMelody =
    {
        (69, 0.5f), (69, 0.5f), (0, 0.5f), (69, 0.5f), (0, 0.5f), (67, 0.5f), (69, 0.5f), (0, 0.5f),
        (72, 0.5f), (0, 0.5f), (64, 0.5f), (0, 0.5f), (60, 1f), (0, 1.5f),
        (65, 0.5f), (0, 0.5f), (67, 0.5f), (0, 0.5f), (69, 1f), (0, 1f),
        (64, 0.5f), (0, 0.5f), (65, 0.5f), (0, 0.5f), (67, 1f), (0, 2f),
    };

    private static readonly (int note, float beats)[] WaterMelody =
    {
        (65, 1f), (69, 1f), (72, 1f), (77, 1.5f), (76, 0.5f), (72, 1f), (69, 1f),
        (67, 1f), (71, 1f), (74, 1f), (79, 1.5f), (77, 0.5f), (74, 1f), (71, 1f),
        (65, 1f), (69, 1f), (72, 1f), (76, 1f), (77, 2f), (72, 1f), (65, 1f),
    };

    private static readonly (int note, float beats)[] CastleMelody =
    {
        (62, 0.5f), (65, 0.5f), (69, 0.5f), (65, 0.5f), (62, 0.5f), (65, 0.5f), (69, 0.5f), (72, 0.5f),
        (74, 1f), (73, 1f), (62, 1f), (0, 1f),
        (60, 0.5f), (63, 0.5f), (67, 0.5f), (63, 0.5f), (60, 0.5f), (63, 0.5f), (67, 0.5f), (70, 0.5f),
        (72, 1f), (71, 1f), (62, 1f), (0, 1f),
    };

    public static AudioClip Music(GameTheme theme)
    {
        switch (theme)
        {
            case GameTheme.Menu:
                return BuildSong("music-menu", 96, MenuMelody, new[] { 48, 45, 41, 43 }, Wave.Triangle, 0.20f, Wave.Triangle, 0.16f, false);

            case GameTheme.Overworld:
                return BuildSong("music-overworld", 132, OverworldMelody, new[] { 48, 53, 55, 48 }, Wave.Square, 0.16f, Wave.Triangle, 0.18f, true);

            case GameTheme.Underground:
                return BuildSong("music-underground", 104, UndergroundMelody, new[] { 45, 41, 43, 40 }, Wave.Square, 0.15f, Wave.Triangle, 0.17f, true);

            case GameTheme.Water:
                return BuildSong("music-water", 92, WaterMelody, new[] { 41, 43, 36, 38 }, Wave.Triangle, 0.19f, Wave.Triangle, 0.15f, true, 3);

            case GameTheme.Castle:
                return BuildSong("music-castle", 144, CastleMelody, new[] { 38, 36, 38, 34 }, Wave.Saw, 0.13f, Wave.Square, 0.18f, true);

            case GameTheme.GameOver:
                return BuildGameOver();

            case GameTheme.Victory:
                return BuildVictory();
        }

        return null;
    }

    private static AudioClip BuildSong(string name, int bpm, (int note, float beats)[] melody, int[] bassRoots,
        Wave melodyWave, float melodyAmp, Wave bassWave, float bassAmp, bool drums, int beatsPerBar = 4)
    {
        if (cache.TryGetValue(name, out AudioClip cached) && cached != null) {
            return cached;
        }

        float beat = 60f / bpm;
        float totalBeats = 0f;

        foreach ((int _, float beats) in melody) {
            totalBeats += beats;
        }

        float[] buffer = New(totalBeats * beat + 0.25f);

        // melodia
        float time = 0f;

        foreach ((int note, float beats) in melody)
        {
            float duration = beats * beat;

            if (note > 0)
            {
                float frequency = MidiToFreq(note);
                AddTone(buffer, time, duration * 0.92f, frequency, frequency, melodyWave, melodyAmp, 0.4f, 0.004f, 0.04f, 0.25f);
                // segunda voz suave una octava abajo
                AddTone(buffer, time, duration * 0.92f, frequency / 2f, frequency / 2f, Wave.Triangle, melodyAmp * 0.25f, 0.5f, 0.004f, 0.04f, 0.2f);
            }

            time += duration;
        }

        // bajo
        float barLength = beatsPerBar * beat;
        int bars = Mathf.Max(1, Mathf.RoundToInt(totalBeats / beatsPerBar));

        for (int bar = 0; bar < bars; bar++)
        {
            int root = bassRoots[bar % bassRoots.Length];
            float barStart = bar * barLength;
            float pattern = Mathf.Min(barLength, 4 * beat);

            for (float offset = 0f; offset < pattern; offset += beat)
            {
                int note = offset < beat * 0.5f ? root : root + 7;
                float frequency = MidiToFreq(note);
                AddTone(buffer, barStart + offset, beat * 0.8f, frequency, frequency, bassWave, bassAmp, 0.5f, 0.004f, 0.05f, 0.3f);
            }
        }

        // percusion
        if (drums)
        {
            for (int bar = 0; bar < bars; bar++)
            {
                for (int hit = 0; hit < beatsPerBar; hit++)
                {
                    float hitTime = bar * barLength + hit * beat;
                    AddNoise(buffer, hitTime, 0.06f, hit % 2 == 0 ? 0.05f : 0.03f, 0.7f);
                }
            }
        }

        return Build(name, buffer, true);
    }

    private static AudioClip BuildGameOver()
    {
        string name = "music-gameover";

        if (cache.TryGetValue(name, out AudioClip cached) && cached != null) {
            return cached;
        }

        float[] buffer = New(4.2f);
        (int note, float start, float duration)[] phrase =
        {
            (67, 0.0f, 0.30f), (64, 0.35f, 0.30f), (60, 0.70f, 0.45f),
            (65, 1.35f, 0.30f), (62, 1.70f, 0.30f), (58, 2.05f, 0.60f),
            (60, 2.85f, 0.40f), (59, 3.30f, 0.40f), (57, 3.75f, 0.40f),
        };

        foreach ((int note, float start, float duration) in phrase)
        {
            int frequency = MidiToFreq(note);
            AddTone(buffer, start, duration, frequency, frequency, Wave.Square, 0.22f, 0.45f, 0.01f, 0.12f, 0.4f);
            AddTone(buffer, start, duration, frequency / 2f, frequency / 2f, Wave.Triangle, 0.14f, 0.5f, 0.01f, 0.12f, 0.3f);
        }

        return Build(name, buffer, false);
    }

    private static AudioClip BuildVictory()
    {
        string name = "music-victory";

        if (cache.TryGetValue(name, out AudioClip cached) && cached != null) {
            return cached;
        }

        float[] buffer = New(5.5f);
        (int note, float start, float duration)[] fanfare =
        {
            (72, 0.0f, 0.18f), (72, 0.20f, 0.18f), (72, 0.40f, 0.18f), (76, 0.60f, 0.45f),
            (72, 1.15f, 0.18f), (76, 1.35f, 0.18f), (79, 1.55f, 0.60f),
            (77, 2.25f, 0.20f), (79, 2.45f, 0.20f), (81, 2.65f, 0.75f),
            (84, 3.50f, 0.90f), (79, 4.45f, 0.35f), (84, 4.85f, 0.65f),
        };

        foreach ((int note, float start, float duration) in fanfare)
        {
            int frequency = MidiToFreq(note);
            AddTone(buffer, start, duration, frequency, frequency, Wave.Square, 0.20f, 0.4f, 0.005f, 0.08f, 0.35f);
            AddTone(buffer, start, duration, frequency / 2f, frequency / 2f, Wave.Triangle, 0.15f, 0.5f, 0.005f, 0.08f, 0.3f);
        }

        // acorde final
        foreach (int note in new[] { 84, 88, 91 })
        {
            int frequency = MidiToFreq(note);
            AddTone(buffer, 5.0f, 0.45f, frequency, frequency, Wave.Square, 0.12f, 0.4f, 0.01f, 0.30f, 0.2f);
        }

        return Build(name, buffer, false);
    }

    /* ------------------------------- ambiente ------------------------------- */

    public static AudioClip Ambient(GameTheme theme)
    {
        string name = $"ambient-{theme}";

        if (cache.TryGetValue(name, out AudioClip cached) && cached != null) {
            return cached;
        }

        float[] buffer = New(8f);
        System.Random random = new System.Random((int)theme * 977 + 13);

        switch (theme)
        {
            case GameTheme.Water:
                AddNoise(buffer, 0f, 8f, 0.05f, 0.92f);

                for (int i = 0; i < 14; i++)
                {
                    float start = (float)random.NextDouble() * 7.5f;
                    float frequency = 300f + (float)random.NextDouble() * 500f;
                    AddTone(buffer, start, 0.18f, frequency, frequency * 1.8f, Wave.Triangle, 0.05f, 0.5f, 0.01f, 0.10f, 0.2f);
                }
                break;

            case GameTheme.Castle:
                AddNoise(buffer, 0f, 8f, 0.09f, 0.97f);

                for (int i = 0; i < 4; i++) {
                    AddTone(buffer, i * 2f, 1.9f, 44f, 38f, Wave.Triangle, 0.11f, 0.5f, 0.4f, 0.5f, 0.1f);
                }

                for (int i = 0; i < 6; i++) {
                    AddNoise(buffer, (float)random.NextDouble() * 7.5f, 0.25f, 0.05f, 0.6f);
                }
                break;

            case GameTheme.Underground:
                AddNoise(buffer, 0f, 8f, 0.035f, 0.95f);

                for (int i = 0; i < 6; i++)
                {
                    float start = (float)random.NextDouble() * 7.4f;
                    float frequency = 900f + (float)random.NextDouble() * 600f;
                    AddTone(buffer, start, 0.12f, frequency, frequency * 0.6f, Wave.Triangle, 0.06f, 0.5f, 0.002f, 0.10f, 0.2f);
                }
                break;

            default:
                AddNoise(buffer, 0f, 8f, 0.025f, 0.96f);
                break;
        }

        return Build(name, buffer, true);
    }

}
