using Godot;
using System.Collections.Generic;

public partial class CharacterSelect : Control
{
    private Dictionary<string, PackedScene> characters = new Dictionary<string, PackedScene>()
    {
        { "Ken", (PackedScene)ResourceLoader.Load("res://Scenes/ken1.tscn") },
        { "Dan", (PackedScene)ResourceLoader.Load("res://Scenes/dan.tscn") }
    };

    private string player1Selection = "Ken";  // Default selection
    private string player2Selection = "Ryu";  // Default selection

    public override void _Ready()
    {
        // Assign button events for Player 1
        GetNode<Button>("KenButtonP1").Pressed += () => SelectCharacter(1, "Ken");
        GetNode<Button>("DanButtonP1").Pressed += () => SelectCharacter(1, "Dan");

        // Assign button events for Player 2
        GetNode<Button>("KenButtonP2").Pressed += () => SelectCharacter(2, "Ken");
        GetNode<Button>("DanButtonP2").Pressed += () => SelectCharacter(2, "Dan");
        

        // Confirm Selection
        GetNode<Button>("ConfirmButton").Pressed += StartMatch;
    }

    private void SelectCharacter(int player, string characterName)
    {
        if (player == 1)
            player1Selection = characterName;
        else
            player2Selection = characterName;
    }

    private void StartMatch()
    {
        // Store selections in Global singleton
        GetNode<Global>("/root/Global").Player1Character = characters[player1Selection];
        GetNode<Global>("/root/Global").Player2Character = characters[player2Selection];

        // Switch to Stage scene
        GetTree().ChangeSceneToFile("res://Scenes/Stage.tscn");
    }
}
