using Godot;

public partial class Stage : Node2D
{
    public override void _Ready()
    {
         GetNode<AnimatedSprite2D>("CanvasLayer2/Background").Play("default");
        
        PackedScene p1Scene = GetNode<Global>("/root/Global").Player1Character;
        PackedScene p2Scene = GetNode<Global>("/root/Global").Player2Character;

        if (p1Scene != null && p2Scene != null)
        {
            Player player1 = (Player)p1Scene.Instantiate();
            Player player2 = (Player)p2Scene.Instantiate();

            player1.PlayerID = 1;
            player2.PlayerID = 2;

            player1.ZIndex = 2; 
            player2.ZIndex = 2; 

            player1.Position = GetNode<Marker2D>("Player1Spawn").Position;
            player2.Position = GetNode<Marker2D>("Player2Spawn").Position;

            AddChild(player1);
            AddChild(player2);
        }
    }
}
