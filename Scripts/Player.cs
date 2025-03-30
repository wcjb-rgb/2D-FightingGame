using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 380f;
    [Export] public float JumpForce = -1000f;
    [Export] public float Gravity = 1600f;
    [Export] public int PlayerID = 1;
    [Export] public int MaxHealth = 100;
    [Export] public Texture2D PortraitTexture;
    public bool IsAI = false;
    private int currentHealth;
    public bool canMove = true; 
    public string PlayerName;
    private MotionInputBuffer motionBuffer;
    private HealthBarManager healthBarManager;
    private AnimationPlayer _anim;
    private Timer _attackCooldown;
    private AudioStreamPlayer _hitSound;
    private AudioStreamPlayer _blockSound;
    private AudioStreamPlayer _attackSound;
    private bool _isFacingRight = true;
    private Player opponent;
    private enum PlayerState { Idle, Moving, Jumping, Attacking, Crouching, KO }
    private PlayerState _state = PlayerState.Idle;
    private string moveLeft, moveRight, jump, down, attackLP, attackLK, attackHP, attackHK, hadouken;
    private PackedScene hadoukenScene = (PackedScene)ResourceLoader.Load("res://Scenes/Hadouken.tscn");
    [Signal] public delegate void VictoryAnimationFinishedEventHandler(Player winner);
    public override void _Ready()
    {
        AddToGroup("Player");
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _anim.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _attackCooldown = GetNode<Timer>("AttackCooldown");
        _hitSound = GetNode<AudioStreamPlayer>("Hit");
        _blockSound = GetNode<AudioStreamPlayer>("Block");
        _attackSound = GetNode<AudioStreamPlayer>("Attack");
        motionBuffer = new MotionInputBuffer();
        AddChild(motionBuffer);

        _attackCooldown.Timeout += () =>
        {
            if (_state == PlayerState.Attacking)
                _state = IsOnFloor() ? PlayerState.Idle : PlayerState.Jumping;
        };

        PlayerName = (PlayerID == 1) ? "Player1" : "Player2";

        AssignInputs();
        CallDeferred("FindOpponent");
        currentHealth = MaxHealth;

        Control hud = GetTree().Root.FindChild("HUD", true, false) as Control;
        if (hud != null)
        {
            healthBarManager = hud.GetNode<HealthBarManager>("Healthbar");
        }
        
        if (IsAI)
        {
            AIBrain brain = new AIBrain();
            AddChild(brain);
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!canMove || _state == PlayerState.KO) return;
        
        ApplyGravity();
        UpdateFacingDirection();

        if (_state == PlayerState.Crouching && !Input.IsActionPressed(down))
        {
            float direction = Input.GetActionStrength(moveRight) - Input.GetActionStrength(moveLeft);
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

        if (_state != PlayerState.Attacking)
        {
            HandleJumping();
            HandleMovement();
            HandleAttack();
        }

        MoveAndSlide();
        CheckLanding();
        motionBuffer.AddDirection(GetCurrentDirection());
    }
    private void ApplyGravity()
    {
        if (!IsOnFloor())
            Velocity += new Vector2(0, Gravity * (float)GetProcessDeltaTime());
    }  
    private void AssignInputs()
    {
        if (PlayerID == 1)
        {
            moveLeft = "p1_left"; moveRight = "p1_right"; jump = "p1_up"; down = "p1_down";
            attackLP = "p1_LP"; attackLK = "p1_LK"; attackHP = "p1_HP"; attackHK = "p1_HK";
        }
        else
        {
            moveLeft = "p2_left"; moveRight = "p2_right"; jump = "p2_up"; down = "p2_down";
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

                var spawnPoint = GetNode<Node2D>("Spawn");
                spawnPoint.Position = new Vector2(Mathf.Abs(spawnPoint.Position.X) * (_isFacingRight ? 1 : -1), spawnPoint.Position.Y);
            }
        }
    }

    private void HandleMovement()
    {
        if (_state == PlayerState.Jumping || _state == PlayerState.Attacking)
            return;

        bool isCrouching = Input.IsActionPressed(down);

        if (isCrouching)
        {
            if (_state != PlayerState.Crouching) 
            {
                _state = PlayerState.Crouching;
                _anim.Play("crouch");
                Velocity = Vector2.Zero;
            }
            return; 
        }

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

    public bool IsBlocking()
    {
        bool holdingBack = (_isFacingRight && Input.IsActionPressed(moveLeft)) || (!_isFacingRight && Input.IsActionPressed(moveRight));
        return holdingBack; 
    }
    public bool IsCrouching()
    {
        return Input.IsActionPressed(down);
    }
    public void PlayBlockReaction()
    {
        _anim.Play("block");
        _blockSound?.Play();
        DisableMovement(0.2f); 
    }
    public void PlayCrouchBlockReaction()
    {
        _anim.Play("crouch_block");
        _blockSound?.Play();
        DisableMovement(0.1f); 
    }

    public void PlayHitReaction()
    {
        _anim.Play("hit");
        _hitSound.Play();
        DisableMovement(0.3f); 
    }

    private void OnAnimationFinished(string animName)
    {
        if (animName == "crouch_block")
        {
            _state = PlayerState.Crouching;
            if (_anim.HasAnimation("crouch"))
            {
                _anim.Play("crouch");
                _anim.Seek(0.3f, true); 
            }
        }

        if (animName == "ko")
        {
            Engine.TimeScale = 1f;

            opponent._anim.Play("Victory");
        }

        if (animName == "Victory")
        {
            EmitSignal(SignalName.VictoryAnimationFinished, this);
        }
        }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (healthBarManager != null)
        {
            healthBarManager.UpdateHealthBar(PlayerName, currentHealth, MaxHealth);
        }

        if (currentHealth <= 0)
        {
            TriggerKO();
        }
    }
    private void TriggerKO()
    {
        canMove = false;
        _state = PlayerState.KO;

        if (_anim.HasAnimation("ko"))
            _anim.Play("ko");

        if (opponent != null)
            opponent.canMove = false;

        Engine.TimeScale = 0.5f;
    }

    private void DisableMovement(float duration)
    {
        canMove = false;
        GetTree().CreateTimer(duration).Connect("timeout", new Callable(this, nameof(EnableMovement)));
    }

    private void EnableMovement()
    {
        canMove = true;
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

        string[] hadouken = { "down", "down_forward", "forward" };

        if (motionBuffer.MatchesPattern(hadouken) && Input.IsActionJustPressed(attackLP))
        {
            PerformHadouken();
            motionBuffer.Clear();
            return;
        }
    }
    private void PerformAttack(string attackType)
    {
        _state = PlayerState.Attacking;
        Velocity = Vector2.Zero;
        _anim.Play(attackType);
        _attackSound?.Play();
        _attackCooldown.Start(0.4f);
    }

    public Texture2D GetPortrait()
    {
        return PortraitTexture;
    }

    private void PerformHadouken()
{
    _state = PlayerState.Attacking;
    Velocity = Vector2.Zero;
    _anim.Play("hadouken");
    _attackCooldown.Start(0.8f); 

}

public void SpawnHadouken()
{
    GD.Print($"{PlayerName} is spawning Hadouken!");
    if (hadoukenScene == null) return;

    Hadouken hadouken = (Hadouken)hadoukenScene.Instantiate();

    Node2D spawnPoint = GetNode<Node2D>("Spawn");
    hadouken.Position = spawnPoint.GlobalPosition;

    hadouken.SetDirection(_isFacingRight ? Vector2.Right : Vector2.Left);

    // Pass ownership info
    hadouken.PlayerID = PlayerID;

    GetTree().Root.AddChild(hadouken);
}

private string GetCurrentDirection()
{
    bool left = Input.IsActionPressed(moveLeft);
    bool right = Input.IsActionPressed(moveRight);
    bool downPressed = Input.IsActionPressed(down);

    if (_isFacingRight)
    {
        if (downPressed && right) return "down_forward";
        if (downPressed && left) return "down_back";
        if (downPressed) return "down";
        if (right) return "forward";
        if (left) return "back";
    }
    else
    {
        if (downPressed && left) return "down_forward";
        if (downPressed && right) return "down_back";
        if (downPressed) return "down";
        if (left) return "forward";
        if (right) return "back";
    }
    return "";
}
}