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
                shape.Disabled = true; 
            }
        }
        
        Connect("area_entered", new Callable(this, nameof(OnHitDetected)));
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
            SetCollisionLayerValue(1, true);

            SetCollisionMaskValue(4, true);
        }
        else if (playerNode.SceneFilePath == global.Player2Character.ResourcePath)
        {
            PlayerName = "Player2";
            SetCollisionLayerValue(3, true);

            SetCollisionMaskValue(2, true);
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

        string opponentName = opponentPlayer.Get("PlayerName").AsString();

        if (opponentName == PlayerName)
        {
            return; 
        }

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
