// Player.cs (Refactored and Organized)
using Godot;
using System;

public partial class Player : CharacterBody2D
{
    // === CONFIGURATION ===
    [Export] public float Speed = 400f;
    [Export] public float JumpForce = -1000f;
    [Export] public float Gravity = 1900f;
    [Export] public int PlayerID = 1;
    [Export] public int MaxHealth = 100;
    [Export] public Texture2D PortraitTexture;

    // === STATE ===
    public bool IsAI = false;
    public bool canMove = true;
    private bool isInHitStun = false;
    private int currentHealth;
    public string PlayerName;
    private PlayerState _state = PlayerState.Idle;
    private bool _isFacingRight = true;

    // === INPUTS ===
    private string moveLeft, moveRight, jump, down;
    private string attackLP, attackLK, attackHP, attackHK;

    // === REFERENCES ===
    private Player opponent;
    private AnimationPlayer _anim;
    private Timer _attackCooldown;
    private SoundManager soundManager;
    private MotionInputBuffer motionBuffer;
    private HealthBarManager healthBarManager;
    private PackedScene hadoukenScene = (PackedScene)ResourceLoader.Load("res://Scenes/Hadouken.tscn");

    // === ENUMS ===
    private enum PlayerState { Idle, Moving, Jumping, Attacking, Crouching, KO, Knockdown }

    // === SIGNALS ===
    [Signal] public delegate void VictoryAnimationFinishedEventHandler(Player winner);

    public override void _Ready()
    {
        AddToGroup("Player");

        _anim = GetNode<AnimationPlayer>("AnimationPlayer");
        _anim.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));
        _attackCooldown = GetNode<Timer>("AttackCooldown");

        motionBuffer = new MotionInputBuffer();
        AddChild(motionBuffer);

        soundManager = GetNode<SoundManager>("SoundManager");

        _attackCooldown.Timeout += () =>
        {
            if (_state == PlayerState.Attacking)
                _state = IsOnFloor() ? PlayerState.Idle : PlayerState.Jumping;
        };

        PlayerName = PlayerID == 1 ? "Player1" : "Player2";

        AssignInputs();
        CallDeferred("FindOpponent");
        currentHealth = MaxHealth;

        Control hud = GetTree().Root.FindChild("HUD", true, false) as Control;
        if (hud != null)
            healthBarManager = hud.GetNode<HealthBarManager>("Healthbar");

        if (IsAI)
            AddChild(new AIBrain());
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_state == PlayerState.KO) return;

        ApplyGravity();
        UpdateFacingDirection();

        if (_state == PlayerState.Crouching && !Input.IsActionPressed(down))
        {
            float direction = Input.GetActionStrength(moveRight) - Input.GetActionStrength(moveLeft);
            if (direction != 0)
                PlayMovementAnimation(direction);
            else
                PlayIdle();
        }

        if (CanPerformAction())
        {
            HandleJumping();
            HandleMovement();
            HandleAttack();
        }

        MoveAndSlide();
        CheckLanding();

        motionBuffer.AddDirection(motionBuffer.GetCurrentDirection(_isFacingRight, moveLeft, moveRight, down));
    }

    // === INPUT & MOVEMENT ===
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

    private void ApplyGravity()
    {
        if (!IsOnFloor())
            Velocity += new Vector2(0, Gravity * (float)GetProcessDeltaTime());
    }

    private void HandleMovement()
    {
        if (!canMove || _state == PlayerState.Jumping || _state == PlayerState.Attacking || _state == PlayerState.Knockdown)
            return;

        if (Input.IsActionPressed(down))
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
        PlayMovementAnimation(direction);
    }

    private void HandleJumping()
    {
        if (!canMove || _state == PlayerState.Attacking)
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
        if (_state == PlayerState.Knockdown || _anim.CurrentAnimation == "knockdown") return;
        if (IsOnFloor() && _state == PlayerState.Jumping)
        {
            _state = PlayerState.Idle;
            PlayIdle();
        }
    }

    private void PlayMovementAnimation(float direction)
    {
        if (direction != 0)
        {
            _state = PlayerState.Moving;
            _anim.Play((_isFacingRight == (direction > 0)) ? "walk_forward" : "walk_backward");
        }
        else
        {
            _state = PlayerState.Idle;
            PlayIdle();
        }
    }

    private void PlayIdle() => _anim.Play("idle");
    private bool CanPerformAction() => canMove && _state != PlayerState.Attacking && _state != PlayerState.Knockdown;

    // === FACING ===
    private void UpdateFacingDirection()
    {
        if (opponent == null) return;

        bool shouldFaceRight = Position.X < opponent.Position.X;
        if (_isFacingRight != shouldFaceRight)
        {
            _isFacingRight = shouldFaceRight;
            GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = !_isFacingRight;
            GetNode<Area2D>("Hitbox").Scale = new Vector2(_isFacingRight ? 1 : -1, 1);

            var spawnPoint = GetNode<Node2D>("Spawn");
            spawnPoint.Position = new Vector2(Mathf.Abs(spawnPoint.Position.X) * (_isFacingRight ? 1 : -1), spawnPoint.Position.Y);
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

    // === COMBAT ===
    private void HandleAttack()
    {
        if (!canMove || _state == PlayerState.Attacking || !IsOnFloor()) return;

        if (Input.IsActionJustPressed(attackLP)) PerformAttack("LP");
        if (Input.IsActionJustPressed(attackLK)) PerformAttack("LK");
        if (Input.IsActionJustPressed(attackHP)) PerformAttack("HP");
        if (Input.IsActionJustPressed(attackHK)) PerformAttack("HK");

        string[] hadouken = { "down", "down_forward", "forward" };
        string[] shoryuken = { "forward", "down", "down_forward" };

        if (motionBuffer.MatchesPattern(hadouken) && Input.IsActionJustPressed(attackLP))
        {
            PerformHadouken();
            motionBuffer.Clear();
            return;
        }
        if (motionBuffer.MatchesPattern(shoryuken) && Input.IsActionJustPressed(attackHP))
        {
            PerformShoryuken();
            motionBuffer.Clear();
            return;
        }
    }

    private void PerformAttack(string attackType)
    {
        _state = PlayerState.Attacking;
        Velocity = Vector2.Zero;
        _anim.Play(attackType);
        soundManager?.PlayAttack();
        _attackCooldown.Start(0.4f);
    }

    private void PerformHadouken()
    {
        _state = PlayerState.Attacking;
        Velocity = Vector2.Zero;
        _anim.Play("hadouken");
        soundManager?.PlayHadouken();
        _attackCooldown.Start(0.8f);
    }

    public void SpawnHadouken()
    {
        GD.Print($"{PlayerName} is spawning Hadouken!");
        if (hadoukenScene == null) return;

        Hadouken hadouken = (Hadouken)hadoukenScene.Instantiate();
        hadouken.Position = GetNode<Node2D>("Spawn").GlobalPosition;
        hadouken.SetDirection(_isFacingRight ? Vector2.Right : Vector2.Left);
        hadouken.PlayerID = PlayerID;

        GetTree().Root.AddChild(hadouken);
    }

    private void PerformShoryuken()
    {
        _state = PlayerState.Attacking;
        Velocity = new Vector2(0, JumpForce * 0.4f);
        _anim.Play("Shoryuken");
        soundManager?.PlayShoryu();
        _attackCooldown.Start(0.7f);
    }

    // === COMBAT REACTIONS ===
    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        healthBarManager?.UpdateHealthBar(PlayerName, currentHealth, MaxHealth);

        if (currentHealth <= 0)
            TriggerKO();
    }

    private void TriggerKO()
    {
        canMove = false;
        _state = PlayerState.KO;
        if (_anim.HasAnimation("ko")) _anim.Play("ko");
        if (opponent != null) opponent.canMove = false;
        Engine.TimeScale = 0.5f;
    }

    public void PlayBlockReaction()
    {
        _anim.Play("block");
        soundManager?.PlayBlock();
        DisableMovement(0.2f);
    }

    public void PlayCrouchBlockReaction()
    {
        _anim.Play("crouch_block");
        soundManager?.PlayBlock();
        DisableMovement(0.1f);
    }

    public void PlayHitReaction()
    {
        _anim.Play("hit");
        soundManager?.PlayHit();
        DisableMovement(0.3f);
    }

    public void PlayAirHitReaction(string attackerName)
    {
        if (!canMove) return;

        _anim.Play("knockdown");
        _state = PlayerState.Knockdown;

        Vector2 knockback = new Vector2();
        if (opponent != null && opponent.PlayerName == attackerName)
            knockback.X = (Position.X < opponent.Position.X) ? -300 : 300;

        knockback.Y = -200;
        Velocity = knockback;

        DisableMovement(0.5f);
    }

    private void DisableMovement(float duration)
    {
        canMove = false;
        GetTree().CreateTimer(duration).Connect("timeout", new Callable(this, nameof(EnableMovement)));
    }

    private void EnableMovement() => canMove = true;
    private void DisableMovementAfterKnockback() => DisableMovement(0.3f);
    private void ExitHitStun() => isInHitStun = false;

    // === ANIMATION EVENTS ===
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
            opponent?._anim.Play("Victory");
            soundManager?.PlayVictory();
        }

        if (animName == "Victory")
            EmitSignal(SignalName.VictoryAnimationFinished, this);

        if (animName == "knockdown")
            GetTree().CreateTimer(0.1f).Connect("timeout", new Callable(this, nameof(FinishKnockdown)));
    }

    private void FinishKnockdown()
    {
        _state = IsOnFloor() ? PlayerState.Idle : PlayerState.Jumping;
        _anim.Play(_state == PlayerState.Idle ? "idle" : "jump");
    }

    // === HELPERS ===
    public bool IsBlocking()
    {
        bool holdingBack = (_isFacingRight && Input.IsActionPressed(moveLeft)) || (!_isFacingRight && Input.IsActionPressed(moveRight));
        return holdingBack;
    }

    public bool IsCrouching() => Input.IsActionPressed(down);
    public Texture2D GetPortrait() => PortraitTexture;
}
