using Godot;

public partial class Global : Node
{
    public enum GameMode { Versus, SinglePlayer }
    public GameMode CurrentGameMode = GameMode.Versus; 
    public PackedScene Player1Character;
    public PackedScene Player2Character;
    public bool Player2IsAI = false;
    public override void _Ready()
    {
        SetProcess(false);
    }
    public void ResetCharacterSelection()
    {
        Player1Character = null;
        Player2Character = null;
    }
}
