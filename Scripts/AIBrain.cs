using Godot;
using System;

public partial class AIBrain : Node
{
    private Player self;
    private Player opponent;
    private float actionCooldown = 0f;
    private Random rand = new Random();
    public override void _Ready()
    {
        CallDeferred(nameof(FindOpponent));
    }

    private void FindOpponent()
    {
        self = GetParent<Player>();

        foreach (Node node in GetTree().GetNodesInGroup("Player"))
        {
            if (node is Player p && p != self)
            {
                opponent = p;
                break;
            }
        }

        if (opponent == null)
        {
            GD.PrintErr("Could not find opponent!");
        }
    }

    public override void _Process(double delta)
    {
        if (opponent == null || !self.canMove || self.IsOnFloor() == false)
            return;

        actionCooldown -= (float)delta;

        if (actionCooldown <= 0)
        {
            float distance = opponent.Position.X - self.Position.X;
            float absDistance = Mathf.Abs(distance);

            if (absDistance > 120f)
            {
                SimulateInput("forward");
            }
            else
            {
                int choice = rand.Next(0, 3);
                switch (choice)
                {
                    case 0: SimulateInput("LP"); break;
                    case 1: SimulateInput("HP"); break;
                    case 2: SimulateInput("backward"); break;
                }
            }
            actionCooldown = 0.6f + (float)GD.RandRange(0.1f, 0.4f);
        }
    }
    private bool IsFacingRight()
    {
        return self.Position.X < opponent.Position.X;
    }

    private void SimulateInput(string direction)
    {
        string prefix = self.PlayerID == 1 ? "p1_" : "p2_";

        switch (direction)
        {
            case "forward":
                Input.ActionPress(prefix + (IsFacingRight() ? "right" : "left"));
                break;

            case "backward":
                Input.ActionPress(prefix + (IsFacingRight() ? "left" : "right"));
                break;

            case "left":
            case "right":
            case "up":
            case "down":
                Input.ActionPress(prefix + direction);
                break;

            case "LP":
            case "HP":
                Input.ActionPress(prefix + direction);
                Input.ActionRelease(prefix + direction); 
                break;
        }
    }
}
