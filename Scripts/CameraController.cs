using Godot;
using System;

public partial class CameraController : Camera2D
{
    [Export] public float CameraSpeed = 5.0f; 
    [Export] public float MinX = 0f;         
    [Export] public float MaxX = 1300f;   

    private Player player1;
    private Player player2;
    public override void _Process(double delta)
    {
        if (player1 == null || player2 == null)
        {
            FindPlayers();
        }
        if (player1 == null || player2 == null) 
            return; 

        float midpointX = (player1.Position.X + player2.Position.X) / 2f;

        midpointX = Mathf.Clamp(midpointX, MinX, MaxX);

        Vector2 targetPosition = new Vector2(midpointX, Position.Y);

        Position = Position.Lerp(targetPosition, (float)delta * CameraSpeed);

    }
    private void FindPlayers()
    {
        foreach (Node node in GetTree().GetNodesInGroup("Players"))
        {
            if (node is Player p)
            {
                if (p.PlayerID == 1) player1 = p;
                else if (p.PlayerID == 2) player2 = p;
            }
        }
    }
}
