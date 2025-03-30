using Godot;
using System;
using System.Collections.Generic;

public partial class MotionInputBuffer : Node
{
    private class InputBufferEntry
    {
        public string Direction;
        public float Time;
        public InputBufferEntry(string direction, float time)
        {
            Direction = direction;
            Time = time;
        }
    }

    private List<InputBufferEntry> inputBuffer = new();
    private float inputBufferDuration = 0.3f;
    public void AddDirection(string direction)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;

        if (direction != "" && (inputBuffer.Count == 0 || inputBuffer[^1].Direction != direction))
        {
            inputBuffer.Add(new InputBufferEntry(direction, currentTime));
        }

        inputBuffer.RemoveAll(entry => currentTime - entry.Time > inputBufferDuration);
    }
    public bool MatchesPattern(string[] pattern)
    {
        int patternIndex = 0;
        foreach (var entry in inputBuffer)
        {
            if (entry.Direction == pattern[patternIndex])
            {
                patternIndex++;
                if (patternIndex >= pattern.Length)
                    return true;
            }
        }
        return false;
    }
    public void Clear()
    {
        inputBuffer.Clear();
    }
}
