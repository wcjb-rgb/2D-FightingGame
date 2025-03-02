using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Cursor : Sprite2D
{
    private Dictionary<string, PackedScene> players = new Dictionary<string, PackedScene>()
    {
        { "Ken", (PackedScene)ResourceLoader.Load("res://Scenes/ken1.tscn") },
        { "Dan", (PackedScene)ResourceLoader.Load("res://Scenes/dan.tscn") }
    };
    // Object Array to store all selectable characters
    private List<Node> characters = new List<Node>();

    // Integer variables for cursor positioning
    private int currentSelected = 0;  // Current cursor position in the characters array
    private int currentColumnSpot = 0; // Cursor position in the column
    private int currentRowSpot = 0;    // Cursor position in the row

    // Exports (Set these in the Inspector)
    [Export] public Texture2D player1Text;
    [Export] public Texture2D player2Text;
    [Export] public int amountOfRows = 2; // Number of rows in character select grid
    [Export] public Vector2 portraitOffset; // Distance between portraits

    // Objects
    private GridContainer gridContainer;

    public override void _Ready()
    {
        // Get the GridContainer node
        gridContainer = GetParent().GetNode<GridContainer>("GridContainer");

        // Fetch all characters in the "Characters" group and store them in the list
        foreach (Node character in GetTree().GetNodesInGroup("Character"))
        {
            if (character is TextureRect characterNode)
            {
                characters.Add(characterNode);
            }
        }

        GD.Print(characters.Count + " characters loaded.");
        
        // Set initial texture (optional)
        Texture = player1Text;
    }

   public override void _Process(double delta)
{
    // Handle Right Input
    if (Input.IsActionJustPressed("p1_right"))
    {
        currentSelected++;
        currentColumnSpot++;

        if (currentColumnSpot > gridContainer.Columns - 1 && currentSelected < characters.Count - 1) 
        {
            Position -= new Vector2((currentColumnSpot - 1) * portraitOffset.X, 0);
            Position += new Vector2(0, portraitOffset.Y);

            currentColumnSpot = 0;
            currentRowSpot += 1;
        }
        else if (currentColumnSpot > gridContainer.Columns - 1 && currentSelected >= characters.Count) 
        {
            Position -= new Vector2((currentColumnSpot - 1) * portraitOffset.X, 0);
            Position -= new Vector2(0, currentRowSpot * portraitOffset.Y);

            currentColumnSpot = 0;
            currentRowSpot = 0;
            currentSelected = 0;
        }
        else
        {
            Position += new Vector2(portraitOffset.X, 0);
        }
    }

    // Handle Left Input
    else if (Input.IsActionJustPressed("p1_left"))
    {
        currentSelected--;
        currentColumnSpot--;

        if (currentColumnSpot < 0 && currentSelected > 0) // Wrap to previous row
        {
            Position += new Vector2((gridContainer.Columns - 1) * portraitOffset.X, 0);
            Position -= new Vector2(0, (amountOfRows - 1) * portraitOffset.Y);

            currentColumnSpot = gridContainer.Columns - 1;
            currentRowSpot--;
        }
        else if (currentColumnSpot < 0 && currentSelected < 0) // Wrap to last character
        {
            Position += new Vector2((gridContainer.Columns - 1) * portraitOffset.X, 0);
            Position += new Vector2(0, (amountOfRows - 1) * portraitOffset.Y);

            currentColumnSpot = gridContainer.Columns - 1;
            currentRowSpot = amountOfRows - 1;
            currentSelected = characters.Count - 1;
        }
        else
        {
            Position -= new Vector2(portraitOffset.X, 0);
        }
    }

    if (Input.IsActionJustPressed("p1_select"))
{
    Global global = GetNode<Global>("/root/Global");

    // Get the selected character's name
    string selectedCharacterName = characters[currentSelected].Name;

    // Ensure the selected character exists in the dictionary
    if (players.ContainsKey(selectedCharacterName))
    {
        if (global.Player1Character == null)
        {
            global.Player1Character = players[selectedCharacterName]; // Assign PackedScene
            Texture = player2Text;
        }
        else
        {
            global.Player2Character = players[selectedCharacterName]; // Assign Player 2's character
            GetTree().ChangeSceneToFile("res://Scenes/Stage.tscn");
        }
    }
    else
    {
        GD.PrintErr($"Character '{selectedCharacterName}' not found in dictionary!");
    }
}

}
}
