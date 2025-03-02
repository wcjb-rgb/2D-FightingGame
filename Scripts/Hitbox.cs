using Godot;
using System;

public partial class Hitbox : Node
{
    private Area2D _leftHand;
    private Area2D _rightHand;
    private Area2D _leftFoot;
    private Area2D _rightFoot;

    public override void _Ready()
    {
        _leftHand = GetNode<Area2D>("../AnimatedSprite2D/LeftHand");
        _rightHand = GetNode<Area2D>("../AnimatedSprite2D/RightHand");
        _leftFoot = GetNode<Area2D>("../AnimatedSprite2D/LeftFoot");
        _rightFoot = GetNode<Area2D>("../AnimatedSprite2D/RightFoot");
        
        _leftHand.BodyEntered += OnHitboxBodyEntered;
        _rightHand.BodyEntered += OnHitboxBodyEntered;
        _leftFoot.BodyEntered += OnHitboxBodyEntered;
        _rightFoot.BodyEntered += OnHitboxBodyEntered;
    }

    private void OnHitboxBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Hurtbox"))
        {
            GD.Print("Hit detected on " + body.Name);
        }
    }
}
