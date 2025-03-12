using Godot;
using System.Collections.Generic;

public partial class Hitbox : Area2D
{
    [Export] public int Damage = 10;
    public string PlayerName;
    private List<CollisionShape2D> hitboxShapes = new List<CollisionShape2D>();

    public override void _Ready()
    {
        AssignPlayerName();
        AssignCollisionLayers();

        foreach (Node child in GetChildren())
        {
            if (child is CollisionShape2D shape)
            {
                hitboxShapes.Add(shape);
                shape.Disabled = true; // Disable hitbox at start
            }
        }
        
        Connect("area_entered", new Callable(this, nameof(OnHitDetected)));
    }

    private void AssignCollisionLayers()
    {
        // First, clear out any bits so we start fresh
        CollisionLayer = 0;
        CollisionMask  = 0;

        Node playerNode = GetParent();
        Global global = GetNode<Global>("/root/Global");

        // Decide which bits to enable based on Player1 or Player2
        if (playerNode.SceneFilePath == global.Player1Character.ResourcePath)
        {
            PlayerName = "Player1";
            // "Layer 2" in the editor = bit index 1 => decimal 2
            SetCollisionLayerValue(1, true);

            // "Mask 3" in the editor = bit index 2 => decimal 4
            SetCollisionMaskValue(4, true);
        }
        else if (playerNode.SceneFilePath == global.Player2Character.ResourcePath)
        {
            PlayerName = "Player2";
            // "Layer 4" in the editor = bit index 3 => decimal 8
            SetCollisionLayerValue(3, true);

            // "Mask 1" in the editor = bit index 0 => decimal 1
            SetCollisionMaskValue(2, true);
        }
        else
        {
            GD.PrintErr("❌ Could not determine player identity in Hitbox!");
        }

        // Force Godot to apply changes before physics step
        SetDeferred("collision_layer", GetCollisionLayer());
        SetDeferred("collision_mask", GetCollisionMask());

        GD.Print($"✅ {PlayerName} Hitbox -> Layer: {GetCollisionLayer()}, Mask: {GetCollisionMask()}");
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
            GD.PrintErr("❌ Could not determine player identity in Hitbox!");
        }

        GD.Print($"✅ Assigned {PlayerName} to Hitbox on {playerNode.Name}");
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

    private void OnHitDetected(Area2D hurtbox)
    {
        Node opponentPlayer = GetPlayerNodeFromHurtbox(hurtbox);

        if (opponentPlayer == null)
        {
            GD.PrintErr("❌ Could not determine opponent Player node from Hurtbox!");
            return;
        }

        string opponentName = opponentPlayer.Get("PlayerName").AsString();

        if (opponentName == PlayerName)
        {
            GD.Print($"🛑 Self-hit prevented: {PlayerName} hit their own hitbox.");
            return; // Prevent self-hit
        }

        // Apply damage
        hurtbox.Call("TakeDamage", Damage, PlayerName);
    }

    private Node GetPlayerNodeFromHurtbox(Area2D hurtbox)
    {
        // Ensure we get the correct Player node
        Node node = hurtbox;
        while (node != null && !(node is CharacterBody2D))
        {
            node = node.GetParent();
        }
        return node;
    }
}
