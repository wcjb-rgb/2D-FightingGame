using Godot;
using System;

public partial class SoundManager : Node
{
    public void PlayHit()
    {
        GetNode<AudioStreamPlayer2D>("Hit").Play();
    }

    public void PlayBlock()
    {
        GetNode<AudioStreamPlayer2D>("Block").Play();
    }

    public void PlayAttack()
    {
        GetNode<AudioStreamPlayer2D>("Attack").Play();
    }

	public void PlayShoryu()
    {
        GetNode<AudioStreamPlayer2D>("Shoryu").Play();
    }

	public void PlayHadouken()
    {
        GetNode<AudioStreamPlayer2D>("Hadouken").Play();
    }

	public void PlayVictory()
    {
        GetNode<AudioStreamPlayer2D>("Victory").Play();
    }
}
