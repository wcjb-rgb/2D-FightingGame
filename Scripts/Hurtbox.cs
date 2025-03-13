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

        if (playerScript == null)
        {
            GD.PrintErr("❌ Hurtbox could not find Player.cs! Check node structure.");
        }   
    }

    private void AssignCollisionLayers()
    {
        // Clear any previous bits
        CollisionLayer = 0;
        CollisionMask  = 0;

        Node playerNode = GetParent();
        Global global = GetNode<Global>("/root/Global");

        if (playerNode.SceneFilePath == global.Player1Character.ResourcePath)
        {
            PlayerName = "Player1";
            // "Layer 2" => bit index 1 => decimal 2
            SetCollisionLayerValue(2, true);
            // "Mask 3" => bit index 2 => decimal 4
            SetCollisionMaskValue(3, true);
        }
        else if (playerNode.SceneFilePath == global.Player2Character.ResourcePath)
        {
            PlayerName = "Player2";
            // "Layer 4" => bit index 3 => decimal 8
            SetCollisionLayerValue(4, true);
            // "Mask 1" => bit index 0 => decimal 1
            SetCollisionMaskValue(1, true);
        }
        else
        {
            GD.PrintErr("❌ Could not determine player identity in Hurtbox!");
        }

        SetDeferred("collision_layer", GetCollisionLayer());
        SetDeferred("collision_mask", GetCollisionMask());

    }

    private void AssignPlayerName()
{
    Node playerNode = GetParent();
    if (playerNode == null)
    {
        GD.PrintErr("❌ Hurtbox: Could not find Player node!");
        return;
    }

    // ✅ Get Player ID dynamically instead of using SceneFilePath
    int playerID = playerNode.Get("PlayerID").AsInt32();

    if (playerID == 1)
    {
        PlayerName = "Player1";
    }
    else if (playerID == 2)
    {
        PlayerName = "Player2";
    }
    else
    {
        GD.PrintErr("❌ Hurtbox: Could not determine Player ID!");
    }

}


    private Node GetPlayerNode()
    {
        // Traverse up to find the Player node
        Node node = this;
        while (node != null && !(node is CharacterBody2D))
        {
            node = node.GetParent();
        }
        return node;
    }

    private void OnHitReceived(Area2D attackHitbox)
{
    // ✅ Check if the hitbox is enabled before processing the hit
    if (!attackHitbox.Monitoring)
    {
        GD.Print($"⚠️ Ignored attack: {attackHitbox.Name} is disabled.");
        return;
    }

    GD.Print($"📡 Hitbox detected: {attackHitbox.Name} - Checking damage...");

    Node attacker = GetPlayerNodeFromHitbox(attackHitbox);
    if (attacker == null)
    {
        GD.PrintErr("❌ Could not determine attacker Player node!");
        return;
    }

    Hitbox hb = attackHitbox as Hitbox;
    if (hb == null)
    {
        GD.PrintErr("❌ Could not cast attack hitbox to Hitbox!");
        return;
    }

    int damage = hb.Damage;
    string attackerName = attacker.Get("PlayerName").AsString();

    GD.Print($"🔥 Player {PlayerName} detected attack from {attackerName}. Damage: {damage}");

    if (damage == 0)
    {
        GD.PrintErr("⚠️ WARNING: Attack registered with 0 damage! Hitbox might be triggering early.");
    }

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
        // Ensure we get the correct Player node
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