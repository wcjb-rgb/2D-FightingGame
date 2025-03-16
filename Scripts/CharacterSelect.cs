using Godot;
using System.Collections.Generic;

public partial class CharacterSelect : Control
{

    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("Background").Play("default");
    }

}
