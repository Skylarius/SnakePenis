using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[System.Serializable]
public struct StatsSample
{
    public int Level;
    public float GameDuration;
    public int JumpsCount;
    public int Length;
    public float RealSpeed;
    public int PortalUsage;

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class StatsData
{
    public List<StatsSample> Samples;

    public StatsData()
    {
        this.Samples = new List<StatsSample>();
    }

    public List<string> StatsToString()
    {
        if (Samples == null || Samples.Count == 0)
        {
            return new List<string>() { "Not enough stats yet" };
        }
        return new List<string>()
        {
            $"Average game time: {Samples.Average(e => e.GameDuration)} s",
            $"Average jumps: {(int)Samples.Average(e => e.JumpsCount)}",
            $"Average length: {Samples.Average(e => e.Length)} cm",
            $"Average speed: {Samples.Average(e => e.RealSpeed)} cm/s",
            $"Average portal usage: {(int)Samples.Average(e => e.PortalUsage)}"
        };
    }
}
