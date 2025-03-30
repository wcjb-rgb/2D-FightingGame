using Godot;
using System.Threading.Tasks;

public partial class SceneTransistion : CanvasLayer
{
	private AnimationPlayer animationPlayer;
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public async Task TransitionToScene(string scenePath)
    {

        animationPlayer.Play("fade_in");
        await ToSignal(animationPlayer, "animation_finished");

        GetTree().ChangeSceneToFile(scenePath);

        animationPlayer.Play("fade_out");
    }
}
