using Godot;

public interface IAttackSource
{
    int Damage { get; }
    string PlayerName { get; }
    bool CanDealChipDamage { get; }
}