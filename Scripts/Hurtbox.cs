using Godot;

public partial class Hurtbox : Area2D
{
    [Export] public int Health = 100;
    public string PlayerName;
    private Player playerScript;

    public override void _Ready()
    {
        AssignPlayerName();
        AssignCollisionLayers();
        Connect("area_entered", new Callable(this, nameof(OnHitReceived)));

        playerScript = GetParent<Player>();

    }

    private void AssignCollisionLayers()
    {
        CollisionLayer = 0;
        CollisionMask  = 0;

        Node playerNode = GetParent();
        Global global = GetNode<Global>("/root/Global");

        if (playerNode.SceneFilePath == global.Player1Character.ResourcePath)
        {
            PlayerName = "Player1";
            SetCollisionLayerValue(2, true);
            SetCollisionMaskValue(3, true);
        }
        else if (playerNode.SceneFilePath == global.Player2Character.ResourcePath)
        {
            PlayerName = "Player2";
            SetCollisionLayerValue(4, true);
            SetCollisionMaskValue(1, true);
        }

        SetDeferred("collision_layer", GetCollisionLayer());
        SetDeferred("collision_mask", GetCollisionMask());

    }

    private void AssignPlayerName()
{
    Node playerNode = GetParent();
    if (playerNode == null)
    {
        return;
    }

    int playerID = playerNode.Get("PlayerID").AsInt32();

    if (playerID == 1)
    {
        PlayerName = "Player1";
    }
    else if (playerID == 2)
    {
        PlayerName = "Player2";
    }
}

    private Node GetPlayerNode()
    {
        Node node = this;
        while (node != null && !(node is CharacterBody2D))
        {
            node = node.GetParent();
        }
        return node;
    }

    private void OnHitReceived(Area2D attackHitbox)
{
    if (!attackHitbox.Monitoring)
    {
        GD.Print($"⚠️ Ignored attack: {attackHitbox.Name} is disabled.");
        return;
    }

    GD.Print($"📡 Hitbox detected: {attackHitbox.Name} - Checking damage...");

    Node attacker = GetPlayerNodeFromHitbox(attackHitbox);

    Hitbox hb = attackHitbox as Hitbox;

    int damage = hb.Damage;
    string attackerName = attacker.Get("PlayerName").AsString();

    GD.Print($"🔥 Player {PlayerName} detected attack from {attackerName}. Damage: {damage}");


    if (playerScript.IsBlocking() && playerScript.IsCrouching())
    {
        GD.Print($"🛡️ {PlayerName} crouch blocked an attack from {attackerName}!");
        playerScript.PlayCrouchBlockReaction();
    }
    else if (playerScript.IsBlocking())
    {
        GD.Print($"🛡️ {PlayerName} blocked an attack from {attackerName}!");
        playerScript.PlayBlockReaction();
    }
    else
    {
        GD.Print($"🔥 {PlayerName} took {damage} damage from {attackerName}.");
        playerScript.PlayHitReaction();
        playerScript.TakeDamage(damage);
    }
}


    private Node GetPlayerNodeFromHitbox(Area2D hitbox)
    {
        Node node = hitbox;
        while (node != null && !(node is CharacterBody2D))
        {
            node = node.GetParent();
        }
        return node;
    }

    public void TakeDamage(int damage, string attackerName)
    {
        Health -= damage;
        GD.Print($"🔥 {PlayerName} took {damage} damage from {attackerName}. Remaining HP: {Health}");

        if (Health <= 0)
        {
            GD.Print($"💀 {PlayerName} is KO!");
        }
    }
}