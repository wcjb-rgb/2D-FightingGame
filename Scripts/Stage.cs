using Godot;

public partial class Stage : Node2D
{
    public override void _Ready()
    {
        // Retrieve stored characters from Global
        PackedScene p1Scene = GetNode<Global>("/root/Global").Player1Character;
        PackedScene p2Scene = GetNode<Global>("/root/Global").Player2Character;

        if (p1Scene != null && p2Scene != null)
        {
            Player player1 = (Player)p1Scene.Instantiate();
            Player player2 = (Player)p2Scene.Instantiate();

            // Assign Player IDs
            player1.PlayerID = 1;
            player2.PlayerID = 2;

            // Position players at their respective spawn points
            player1.Position = GetNode<Marker2D>("Player1Spawn").Position;
            player2.Position = GetNode<Marker2D>("Player2Spawn").Position;

            AddChild(player1);
            AddChild(player2);
        }
    }
}
