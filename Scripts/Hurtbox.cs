using Godot;

public partial class Hurtbox : Area2D
{
    [Export] public int Health = 100;
    public string PlayerName;

    public override void _Ready()
    {
        AssignPlayerName();
        AssignCollisionLayers();
        Connect("area_entered", new Callable(this, nameof(OnHitReceived)));
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

        GD.Print($"✅ {PlayerName} Hurtbox -> Layer: {GetCollisionLayer()}, Mask: {GetCollisionMask()}");
    }

    private void AssignPlayerName()
    {
        Node playerNode = GetPlayerNode();
        if (playerNode == null) return;

        Global global = GetNode<Global>("/root/Global");

        if (playerNode.SceneFilePath == global.Player1Character.ResourcePath)
        {
            PlayerName = "Player1";
        }
        else if (playerNode.SceneFilePath == global.Player2Character.ResourcePath)
        {
            PlayerName = "Player2";
        }
        else
        {
            GD.PrintErr("❌ Could not determine player identity in Hurtbox!");
        }

        GD.Print($"✅ Assigned {PlayerName} to Hurtbox on {playerNode.Name}");
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
        Node attacker = GetPlayerNodeFromHitbox(attackHitbox);
        if (attacker == null)
        {
            GD.PrintErr("❌ Could not determine attacker Player node!");
            return;
        }

        int damage = attacker.Get("Damage").AsInt32();
        string attackerName = attacker.Get("PlayerName").AsString();

        TakeDamage(damage, attackerName);
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
