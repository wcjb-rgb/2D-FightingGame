using Godot;

public partial class Global : Node
{
    public PackedScene Player1Character { get; set; } // Stores Player 1’s selected character
    public PackedScene Player2Character { get; set; } // Stores Player 2’s selected character

    public override void _Ready()
    {
        // Keep this node persistent across scene changes
        SetProcess(false);
    }
}
