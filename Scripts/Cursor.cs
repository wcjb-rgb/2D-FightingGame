using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Cursor : Sprite2D
{
    private Dictionary<string, PackedScene> players = new Dictionary<string, PackedScene>()
    {
        { "Ken", (PackedScene)ResourceLoader.Load("res://Scenes/ken1.tscn") },
        { "Ryu", (PackedScene)ResourceLoader.Load("res://Scenes/ryu.tscn")}
    };

    private Dictionary<string, Texture2D> previewImages = new Dictionary<string, Texture2D>()
    {
     { "Ken", ResourceLoader.Load("res://Assets/CharSelect/KenPreview.png") as Texture2D },
     { "Ryu", ResourceLoader.Load("res://Assets/CharSelect/RyuPreview.png") as Texture2D },
    };

    private Dictionary<string, string> idleAnimations = new Dictionary<string, string>()
    {
        { "Ken", "KenIdle" },
        { "Ryu", "RyuIdle" }
    };

    private Dictionary<string, string> selectAnimations = new Dictionary<string, string>()
    {
        { "Ken", "KenSelect" },
        { "Ryu", "RyuSelect" }
    };

    private List<Node> characters = new List<Node>();
    private int currentSelected = 0;

    [Export] public Texture2D player1Text;
    [Export] public Texture2D player2Text;
    [Export] public Vector2 portraitOffset;

    private AnimatedSprite2D p1AnimatedSprite;
    private AnimatedSprite2D p2AnimatedSprite;
    private GridContainer gridContainer;
    private Vector2 initialPosition;

    private TextureRect p1Preview;
    private TextureRect p2Preview;

    private bool selectionComplete = false;


    public override void _Ready()
    {
        gridContainer = GetParent().GetNode<GridContainer>("GridContainer");
        p1AnimatedSprite = GetNode<AnimatedSprite2D>("../P1AnimatedSprite");
        p2AnimatedSprite = GetNode<AnimatedSprite2D>("../P2AnimatedSprite");

        p2AnimatedSprite.Visible = false;

        foreach (Node character in GetTree().GetNodesInGroup("Character"))
        {
            if (character is TextureRect characterNode)
            {
                characters.Add(characterNode);
            }
        }
        GD.Print(characters.Count + " characters loaded.");

        initialPosition = Position;
        Texture = player1Text; 

        p1Preview = GetNode<TextureRect>("../P1Preview");
        p2Preview = GetNode<TextureRect>("../P2Preview");

        if (characters.Count > 0)
        {
            string firstCharacterName = characters[0].Name;
            PlayIdleAnimation(p1AnimatedSprite, firstCharacterName);
        }

        currentSelected = 0;

        UpdateHoverAnimation();
        
    }

    public override void _Process(double delta)
    {
         if (selectionComplete)
        return;

        if (Input.IsActionJustPressed("p1_right"))
        {
            currentSelected++;
            if (currentSelected >= characters.Count)
                currentSelected = 0;
            UpdateHoverAnimation();
        }
        else if (Input.IsActionJustPressed("p1_left"))
        {
            currentSelected--;
            if (currentSelected < 0)
                currentSelected = characters.Count - 1;
            UpdateHoverAnimation();
        }

        Position = initialPosition + new Vector2(currentSelected * portraitOffset.X, 0);

        if (Input.IsActionJustPressed("p1_select"))
        {
            _ = SelectCharacter();
        }
    }

    private void UpdateHoverAnimation()
    {
        string highlightedCharacterName = characters[currentSelected].Name;

        Global global = GetNode<Global>("/root/Global");

        if (global.Player1Character == null)
        {
            PlayIdleAnimation(p1AnimatedSprite, highlightedCharacterName);
            if (previewImages.ContainsKey(highlightedCharacterName))
            {
                p1Preview.Texture = previewImages[highlightedCharacterName];
            }
        }
        else
        {
            PlayIdleAnimation(p2AnimatedSprite, highlightedCharacterName);
            if (previewImages.ContainsKey(highlightedCharacterName))
            {
                p2Preview.Texture = previewImages[highlightedCharacterName];
            }
        }
    }

    private async Task SelectCharacter()
    {
        string selectedCharacterName = characters[currentSelected].Name;
        Global global = GetNode<Global>("/root/Global");

        if (!players.ContainsKey(selectedCharacterName))
        {
            GD.PrintErr($"Character '{selectedCharacterName}' not found in dictionary!");
            return;
        }

        if (global.Player1Character == null)
        {
            global.Player1Character = players[selectedCharacterName];
            PlaySelectAnimation(p1AnimatedSprite, selectedCharacterName);

            Texture = player2Text;

            p2AnimatedSprite.Visible = true;
            UpdateHoverAnimation();
        }
        else
        {
            global.Player2Character = players[selectedCharacterName];
            PlaySelectAnimation(p2AnimatedSprite, selectedCharacterName);

            selectionComplete = true;

            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

            GetTree().ChangeSceneToFile("res://Scenes/Stage.tscn");
        }
    }

    private void PlayIdleAnimation(AnimatedSprite2D sprite, string characterName)
    {
        if (!idleAnimations.ContainsKey(characterName))
            return; 

        sprite.Animation = idleAnimations[characterName];
        sprite.Play();
    }

    private void PlaySelectAnimation(AnimatedSprite2D sprite, string characterName)
    {
        if (!selectAnimations.ContainsKey(characterName))
            return; 

        sprite.Animation = selectAnimations[characterName];
        sprite.Play();
    }
}
