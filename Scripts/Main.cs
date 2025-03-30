using Godot;

public partial class Main : Control
{
    [Export] public AnimationPlayer AnimPlayer;
    public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("Background").Play("default");
        AnimPlayer.Play("Float");

        GetNode<TextureButton>("VS").Pressed += OnPlayButtonPressed;
    }

    private async void OnPlayButtonPressed()
    {
        var transition = (SceneTransistion)GetNode("/root/Transition");
        await transition.TransitionToScene("res://Scenes/CharacterSelect.tscn");

    }
}

