using Godot;
using System;

public partial class AIBehaviour : CharacterBody2D
{
	private Player player;
	private Player AIplayer;
	private float AISpeed = 380f;
	private float range = 50f;
	private bool attacking = false;
	private Random _random;

	private AnimationPlayer _anim;
	
	public override void _Ready()
	{
		AIplayer = (Player)GetParent();
		player = GetNode<Player>("Player");

		_anim = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleMovement();
		HandleAttack();
	}

	private void HandleMovement()
	{
		float direction = AIplayer.Position.X - player.Position.X;
		if (direction > 0)
		{
			Velocity = new Vector2(direction * AISpeed,Velocity.Y);
			_anim.Play("walk_forward");
		}
		else if (direction < 0)
		{
			Velocity = new Vector2(direction * AISpeed, Velocity.Y);
			_anim.Play("walk_backward");
		}
		else
		{
			Velocity = Vector2.Zero;
			_anim.Play("idle");        }
	}

	public void HandleAttack()
	{
		string[] choices = {"LP", "LK", "HP", "HK"};
		string attack = choices[GD.RandRange(0, 3)];

		if (AIplayer.Position.DistanceTo(player.Position) <range && !attacking)
		{
			if (GD.RandRange(0,100) < 50)
			{
				attacking = true;
				AIplayer.PerformAttack(attack);
			}
			else if (GD.RandRange(0,100) < 10 && !attacking)
			{
				_anim.Play("forward_jump");
			}
		} 
	} 
}
