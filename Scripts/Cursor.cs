using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Cursor : Sprite2D
{
    private AudioStreamPlayer moveSound;
    private AudioStreamPlayer selectSound;  
    private Dictionary<string, PackedScene> players = new Dictionary<string, PackedScene>()
    {
        { "Ken", (PackedScene)ResourceLoader.Load("res://Scenes/ken1.tscn") },
        { "Ryu", (PackedScene)ResourceLoader.Load("res://Scenes/ryu.tscn")},
        { "Honda", (PackedScene)ResourceLoader.Load("res://Scenes/honda.tscn") }
    };

    private Dictionary<string, Texture2D> previewImages = new Dictionary<string, Texture2D>()
    {
     { "Ken", ResourceLoader.Load("res://Assets/CharSelect/KenPreview.png") as Texture2D },
     { "Ryu", ResourceLoader.Load("res://Assets/CharSelect/RyuPreview.png") as Texture2D },
     { "Honda", ResourceLoader.Load("res://Assets/CharSelect/HondaPreview.png") as Texture2D },
    };

    private Dictionary<string, string> idleAnimations = new Dictionary<string, string>()
    {
        { "Ken", "KenIdle" },
        { "Ryu", "RyuIdle" },
        { "Honda", "HondaIdle" },
    };

    private Dictionary<string, string> selectAnimations = new Dictionary<string, string>()
    {
        { "Ken", "KenSelect" },
        { "Ryu", "RyuSelect" },
        { "Honda", "HondaSelect" }
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

        moveSound = GetNode<AudioStreamPlayer>("../Hover");
        selectSound = GetNode<AudioStreamPlayer>("../Select");

        p2AnimatedSprite.Visible = false;

        foreach (Node character in GetTree().GetNodesInGroup("Character"))
        {
            if (character is TextureRect characterNode)
            {
                characters.Add(characterNode);
            }
        }

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
            moveSound.Play();
            UpdateHoverAnimation();
        }
        else if (Input.IsActionJustPressed("p1_left"))
        {
            currentSelected--;
            if (currentSelected < 0)
                currentSelected = characters.Count - 1;
            moveSound.Play();
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
        return;
    }

    if (global.Player1Character == null)
    {
        global.Player1Character = players[selectedCharacterName];
        selectSound.Play();
        PlaySelectAnimation(p1AnimatedSprite, selectedCharacterName);
        
        if (global.CurrentGameMode == Global.GameMode.SinglePlayer)
        {
            global.Player2Character = players["Ken"];
            global.Player2IsAI = true;
            selectionComplete = true;

            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

            var transition = (SceneTransistion)GetNode("/root/Transition");
            await transition.TransitionToScene("res://Scenes/Stage.tscn");
        }
        else
        {
            Texture = player2Text;
            p2AnimatedSprite.Visible = true;
            UpdateHoverAnimation();
        }
    }
    else
    {
        global.Player2Character = players[selectedCharacterName];
        global.Player2IsAI = false;

        selectSound.Play();
        PlaySelectAnimation(p2AnimatedSprite, selectedCharacterName);

        selectionComplete = true;

        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        var transition = (SceneTransistion)GetNode("/root/Transition");
        await transition.TransitionToScene("res://Scenes/Stage.tscn");
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
