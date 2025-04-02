using Godot;
using System;

public partial class AIBehaviour : CharacterBody2D
{
	private Player player;
	private Player AIplayer;
	private float AISpeed = 380f;
	private float range = 50f;
	private bool attacking = false;

	private AnimationPlayer _anim;

	public override void _Ready()
	{
		AIplayer = (Player)GetParent();
		player = GetNode<Player>("Player");

		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement(delta);
		HandleAttack();
	}

	private void HandleMovement(double delta)
	{
		float direction = AIplayer.Position.X - player.Position.X;
		if (direction > 0)
		{
			Velocity = new Vector2(AISpeed,Velocity.Y);
			_anim.Play("walk_forward");
		}
		else if (direction < 0)
		{
			Velocity = new Vector2(-AISpeed, Velocity.Y);
			_anim.Play("walk_backward");
		}
		else
		{
			Velocity = Vector2.Zero;
			_anim.Play("idle");        }
	}

	private void HandleAttack()
	{
		if (AIplayer.Position.DistanceTo(player.Position) <range && !attacking)
		{
			attacking = true;
			AIplayer.PerformAttack("LP");
		} 
	}
}
