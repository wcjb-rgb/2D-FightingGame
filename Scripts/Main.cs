using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        
    }

    private void OnPressed()
    {
        GetTree().ChangeSceneToFile("res://Stage/stage.tscn");
    }
}
