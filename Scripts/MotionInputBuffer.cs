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

    public string GetCurrentDirection(bool isFacingRight, string moveLeft, string moveRight, string down)
    {
        bool left = Input.IsActionPressed(moveLeft);
        bool right = Input.IsActionPressed(moveRight);
        bool downPressed = Input.IsActionPressed(down);

        if (isFacingRight)
        {
            if (downPressed && right) return "down_forward";
            if (downPressed && left) return "down_back";
            if (downPressed) return "down";
            if (right) return "forward";
            if (left) return "back";
        }
        else
        {
            if (downPressed && left) return "down_forward";
            if (downPressed && right) return "down_back";
            if (downPressed) return "down";
            if (left) return "forward";
            if (right) return "back";
        }
        return "";
    }

    public void Clear()
    {
        inputBuffer.Clear();
    }
}
