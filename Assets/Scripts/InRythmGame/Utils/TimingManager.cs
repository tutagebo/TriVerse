using System;
using UnityEngine;

public class TimingManager
{
    public static double BeatToSeconds(double beat, BpmPoint[] bpmPoints)
    {
        const double MIN_BPM = 1e-6;
        const double EPS = 1e-9;
        if (bpmPoints == null || bpmPoints.Length == 0) return 0.0;
        var head = bpmPoints[0];
        double headBpm = Math.Max(head.bpm, MIN_BPM);
        // EPSは誤差を吸収するための微小値らしい
        // これがあることとで0.9999...みたいな値が1.0扱いされる
        if (beat <= head.beat + EPS)    // マイナス拍
        {
            // head.beat を 0秒とみなし、そこから逆換算
            return (beat - head.beat) * 60.0 / headBpm;
        }

        // 通常区間
        double accSec = 0.0;
        for (int i = 0; i < bpmPoints.Length; i++)
        {
            double b0 = bpmPoints[i].beat;
            double bpm = Math.Max(bpmPoints[i].bpm, MIN_BPM);
            double b1 = (i + 1 < bpmPoints.Length) ? bpmPoints[i + 1].beat : double.PositiveInfinity;

            if (beat <= b0 + EPS) break;

            double segEnd = Math.Min(beat, b1);
            double segBeats = Math.Max(0.0, segEnd - b0);
            if (segBeats > 0.0)
                accSec += (segBeats * 60.0) / bpm;

            if (beat <= b1 + EPS) break; // 目的のbeatまで到達
        }
        return accSec;
    }

    public static double SecondsToBeat(double seconds, BpmPoint[] bpmPoints)
    {
        const double MIN_BPM = 1e-6;
        const double EPS = 1e-9;
        if (bpmPoints == null || bpmPoints.Length == 0) return 0.0;

        // 並び順が不明ならソート（コストを嫌うならロード時に一度だけ整列しておく）
        var map = (BpmPoint[])bpmPoints.Clone();
        Array.Sort(map, (a, b) => a.beat.CompareTo(b.beat));

        // 負秒（リードイン等）は先頭BPMで逆換算
        if (seconds <= 0.0 + EPS)
        {
            // 負のときは先頭BPMで計算
            double headBeat = map[0].beat;
            double headBpm = Math.Max(map[0].bpm, MIN_BPM);

            // secondsが-EPS程度でも安定して負拍へ
            return headBeat + (seconds * headBpm) / 60.0;
        }

        double accumulatedSec = 0.0;

        for (int i = 0; i < map.Length; i++)
        {
            double b0 = map[i].beat;
            double bpm = Math.Max(map[i].bpm, MIN_BPM);
            double b1 = (i + 1 < map.Length) ? map[i + 1].beat : double.PositiveInfinity;

            // 区間の長さ拍と秒
            double sectionBeats = b1 - b0;
            double sectionSec = double.IsPositiveInfinity(sectionBeats)
                ? double.PositiveInfinity
                : (sectionBeats * 60.0) / bpm;

            // 同様にEPSで誤差吸収
            if (seconds <= accumulatedSec + sectionSec + EPS)
            {
                double localSec = Math.Max(0.0, seconds - accumulatedSec);
                double localBeats = (localSec * bpm) / 60.0;
                return b0 + localBeats;
            }

            accumulatedSec += sectionSec;
        }

        // 最後の区間を超えたら最後のBPM
        var last = map[map.Length - 1];
        double lastBpm = Math.Max(last.bpm, MIN_BPM);
        double extraBeats = (seconds - accumulatedSec) * lastBpm / 60.0;
        return last.beat + extraBeats;
    }

    /// <summary>
    /// tick(整数) を秒に変換します。内部で beat = tick / resolution に変換してから BeatToSeconds を呼びます。
    /// </summary>
    public static double TickToSeconds(long tick, BpmPoint[] bpmPoints, int resolution)
    {
        if (resolution <= 0) throw new ArgumentException("resolution must be > 0");
        double beat = (double)tick / resolution;
        return BeatToSeconds(beat, bpmPoints);
    }
    static public double GetNowBeat(GameSettings gameSettings)
    {
        double nowElapsedDsp = AudioSettings.dspTime - gameSettings.startMusicDsp;
        return SecondsToBeat(nowElapsedDsp, gameSettings.bpmPoints);
    }
    static public double GetNowDsp(GameSettings gameSettings)
    {
        return AudioSettings.dspTime - gameSettings.startMusicDsp;
    }
}
