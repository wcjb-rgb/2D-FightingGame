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
            return;
        }

        Node attacker = GetPlayerNodeFromHitbox(attackHitbox);

        IAttackSource attack = attackHitbox as IAttackSource;

        if (attack == null)
        {
            return;
        }

        int damage = attack.Damage;
        string attackerName = attack.PlayerName;

        bool isBlocked = playerScript.IsBlocking();
        bool isCrouching = playerScript.IsCrouching();
        

        if (isBlocked)
        {
            if (attack.CanDealChipDamage)
            {
                int chipDamage = 5; 

                if (isCrouching)
                    playerScript.PlayCrouchBlockReaction();
                else
                    playerScript.PlayBlockReaction();

                playerScript.TakeDamage(chipDamage);
            }
            else
            {
                if (isCrouching)
                    playerScript.PlayCrouchBlockReaction();
                else
                    playerScript.PlayBlockReaction();
                return;
            }
        }
        else
{
    if (!playerScript.IsOnFloor())
    {
        playerScript.PlayAirHitReaction(attack.PlayerName);
    }
    else
    {
        playerScript.PlayHitReaction();
    }
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

    }
}