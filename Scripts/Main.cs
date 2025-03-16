using Godot;

public partial class Main : Control
{
    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("Background").Play("default");
    }
}

