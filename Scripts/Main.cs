using Godot;

public partial class Main : Control
{
    [Export] public AnimationPlayer AnimPlayer;
    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("Background").Play("default");
        AnimPlayer.Play("Float");

        GetNode<TextureButton>("VS").Pressed += OnVersusButtonPressed;
        GetNode<TextureButton>("SinglePlayer").Pressed += OnSinglePlayerButtonPressed; 
    }

    private async void OnVersusButtonPressed()
    {
        var transition = (SceneTransistion)GetNode("/root/Transition");
        await transition.TransitionToScene("res://Scenes/CharacterSelect.tscn");
    }

    private async void OnSinglePlayerButtonPressed()
    {
        var global = GetNode<Global>("/root/Global");
        global.CurrentGameMode = Global.GameMode.SinglePlayer;
        global.Player2IsAI = true;

        var transition = (SceneTransistion)GetNode("/root/Transition");
        await transition.TransitionToScene("res://Scenes/CharacterSelect.tscn");
    }
}

