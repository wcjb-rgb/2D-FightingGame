using Godot;

public partial class Hadouken : Area2D, IAttackSource
{
    [Export] public int Damage { get; set; } = 10;
    public int PlayerID;
    public string PlayerName => $"Player{PlayerID}";
    private Vector2 direction = Vector2.Right;
    [Export] public float Speed = 700f;

    public bool CanDealChipDamage => true;


    private bool hasHit = false;
    private AnimationPlayer animPlayer;

    public override void _Ready()
    {
        AssignCollisionLayers();
        Connect("area_entered", new Callable(this, nameof(OnHitDetected)));

        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animPlayer.Play("fly");

    }

    public override void _PhysicsProcess(double delta)
    {
        Position += direction * Speed * (float)delta;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.Normalized();
        Scale = new Vector2(dir.X > 0 ? 1 : -1, 1); // flip sprite if needed
    }

    private void AssignCollisionLayers()
    {
        CollisionLayer = 0;
        CollisionMask = 0;

        SetCollisionLayerValue(6, true);

    // What this Hadouken can collide with:
    SetCollisionMaskValue(6, true); 

        if (PlayerID == 1)
        {
            SetCollisionLayerValue(1, true); // same as P1 hitbox
            SetCollisionMaskValue(4, true);  // targets P2 hurtbox
        }
        else if (PlayerID == 2)
        {
            SetCollisionLayerValue(3, true); // same as P2 hitbox
            SetCollisionMaskValue(2, true);  // targets P1 hurtbox
        }

        SetDeferred("collision_layer", GetCollisionLayer());
        SetDeferred("collision_mask", GetCollisionMask());
    }

    private void OnHitDetected(Area2D area)
{
    if (hasHit) return;

    // Hadouken vs Hadouken (clash)
    if (area is Hadouken other && other.PlayerID != PlayerID)
    {
        GD.Print("🔥 Hadouken clash!");
        Explode();
        other.Explode(); // Make the other hadouken explode too
        return;
    }

    // Hadouken vs hurtbox
    if (area is Hurtbox hb && hb.PlayerName != $"Player{PlayerID}")
    {
        hb.CallDeferred("TakeDamage", Damage, $"Player{PlayerID}");
        Explode();
    }
}

public void Explode()
{
    if (hasHit) return;
    hasHit = true;

    SetDeferred("monitoring", false);
    SetProcess(false);
    SetPhysicsProcess(false);

    foreach (var shape in GetChildren())
    {
        if (shape is CollisionShape2D cs)
            cs.SetDeferred("disabled", true);
    }

    if (animPlayer.HasAnimation("hit"))
        animPlayer.Play("hit");
    else
        QueueFree();
}


}
