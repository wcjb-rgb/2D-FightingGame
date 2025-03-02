using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public string CharacterName = "Ken";
    [Export] public float Speed = 200f;
    [Export] public float JumpForce = -600f;
    [Export] public float Gravity = 980f;
    [Export] public int PlayerID = 1;

    private AnimationPlayer _anim;
    private Timer _attackCooldown;
    private bool _isFacingRight = true;
    private Player opponent;

    private enum PlayerState { Idle, Moving, Jumping, Attacking }
    private PlayerState _state = PlayerState.Idle;

    private string moveLeft, moveRight, jump, attackLP, attackLK, attackHP, attackHK;

    public override void _Ready()
    {
    _anim = GetNode<AnimationPlayer>("AnimationPlayer");
    _attackCooldown = GetNode<Timer>("AttackCooldown");
    _attackCooldown.Timeout += () =>
    {
        if (_state == PlayerState.Attacking)
            _state = IsOnFloor() ? PlayerState.Idle : PlayerState.Jumping;
    };

    AssignInputs();
    CallDeferred("FindOpponent");
    
    }

    private void AssignInputs()
    {
        if (PlayerID == 1)
        {
            moveLeft = "p1_left"; moveRight = "p1_right"; jump = "p1_up";
            attackLP = "p1_LP"; attackLK = "p1_LK"; attackHP = "p1_HP"; attackHK = "p1_HK";
        }
        else
        {
            moveLeft = "p2_left"; moveRight = "p2_right"; jump = "p2_up";
            attackLP = "p2_LP"; attackLK = "p2_LK"; attackHP = "p2_HP"; attackHK = "p2_HK";
        }
    }

    private void FindOpponent()
    {
        foreach (Node node in GetParent().GetChildren())
        {
            if (node is Player p && p != this)
            {
                opponent = p;
                break;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity();
        
        // Ensure both players always face each other
        UpdateFacingDirection();

        if (_state != PlayerState.Attacking)
        {
            HandleJumping();
            HandleMovement();
            HandleAttack();
        }

        MoveAndSlide();
        CheckLanding();
    }

    private void ApplyGravity()
    {
        if (!IsOnFloor())
            Velocity += new Vector2(0, Gravity * (float)GetProcessDeltaTime());
    }

    private void UpdateFacingDirection()
{
    if (opponent != null)
    {
        bool shouldFaceRight = Position.X < opponent.Position.X;

        if (_isFacingRight != shouldFaceRight)
        {
            _isFacingRight = shouldFaceRight;

            GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = !_isFacingRight;
            
            var hitboxNode = GetNode<Area2D>("Hitbox");
            hitboxNode.Scale = new Vector2(_isFacingRight ? 1 : -1, 1);
        }
    }
}


    private void HandleMovement()
    {
        if (_state == PlayerState.Jumping || _state == PlayerState.Attacking)
            return;

        float direction = Input.GetActionStrength(moveRight) - Input.GetActionStrength(moveLeft);
        Velocity = new Vector2(direction * Speed, Velocity.Y);

        if (direction != 0)
        {
            _state = PlayerState.Moving;
            _anim.Play((_isFacingRight == (direction > 0)) ? "walk_forward" : "walk_backward");
        }
        else
        {
            _state = PlayerState.Idle;
            _anim.Play("idle");
        }
    }

    private void HandleJumping()
    {
        if (_state == PlayerState.Attacking)
            return;

        if (IsOnFloor() && Input.IsActionJustPressed(jump))
        {
            Velocity = new Vector2(Velocity.X, JumpForce);
            _state = PlayerState.Jumping;

            float direction = Input.GetActionStrength(moveRight) - Input.GetActionStrength(moveLeft);
            bool movingForward = (_isFacingRight && direction > 0) || (!_isFacingRight && direction < 0);
            bool movingBackward = (_isFacingRight && direction < 0) || (!_isFacingRight && direction > 0);

            if (movingForward) _anim.Play("forward_jump");
            else if (movingBackward) _anim.Play("backward_jump");
            else _anim.Play("jump");
        }
    }

    private void CheckLanding()
    {
        if (IsOnFloor() && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
            _anim.Play("idle");
        }
    }

    private void HandleAttack()
    {
        if (_state == PlayerState.Attacking || !IsOnFloor())
            return;

        if (Input.IsActionJustPressed(attackLP)) PerformAttack("LP");
        if (Input.IsActionJustPressed(attackLK)) PerformAttack("LK");
        if (Input.IsActionJustPressed(attackHP)) PerformAttack("HP");
        if (Input.IsActionJustPressed(attackHK)) PerformAttack("HK");
    }

    private void PerformAttack(string attackType)
    {
        _state = PlayerState.Attacking;
        Velocity = Vector2.Zero;
        _anim.Play(attackType);
        _attackCooldown.Start(0.4f);
    }
}
