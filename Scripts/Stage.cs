using Godot;
using System;
using System.Collections.Generic;

public partial class Stage : Node2D
{
    [Export] public AnimationPlayer AnimPlayer;
    private Player player1;
    private Player player2;
    private Dictionary<string, string> idleAnimations = new()
    {
        { "ken1", "KenIdle" },
        { "ryu", "RyuIdle" },
        { "honda", "HondaIdle" },
    };

   public override void _Ready()
    {
        GetNode<AnimatedSprite2D>("CanvasLayer2/Background").Play("default");

        PackedScene p1Scene = GetNode<Global>("/root/Global").Player1Character;
        PackedScene p2Scene = GetNode<Global>("/root/Global").Player2Character;

        if (p1Scene != null && p2Scene != null)
        {
            string p1Name = p1Scene.ResourcePath.GetFile().GetBaseName(); 
            string p2Name = p2Scene.ResourcePath.GetFile().GetBaseName(); 

            Player tempP1 = (Player)p1Scene.Instantiate();
            Player tempP2 = (Player)p2Scene.Instantiate();

            var intro1 = GetNode<AnimatedSprite2D>("IntroPlayer1");
            var intro2 = GetNode<AnimatedSprite2D>("IntroPlayer2");

            intro1.Position = GetNode<Marker2D>("Player1Spawn").Position;
            intro2.Position = GetNode<Marker2D>("Player2Spawn").Position;

            if (idleAnimations.ContainsKey(p1Name))
            {
                intro1.Play(idleAnimations[p1Name]);
            }

            if (idleAnimations.ContainsKey(p2Name))
            {
                intro2.Play(idleAnimations[p2Name]);
            }

            tempP1.QueueFree();
            tempP2.QueueFree();

            var global = GetNode<Global>("/root/Global");

            player1 = (Player)p1Scene.Instantiate();
            player2 = (Player)p2Scene.Instantiate();

            player1.IsAI = false;
            player2.IsAI = global.Player2IsAI;

            player1.VictoryAnimationFinished += OnVictoryAnimationFinished;
            player2.VictoryAnimationFinished += OnVictoryAnimationFinished;

            player1.PlayerID = 1;
            player2.PlayerID = 2;

            player1.ZIndex = 2;
            player2.ZIndex = 2;

            player1.Position = GetNode<Marker2D>("Player1Spawn").Position;
            player2.Position = GetNode<Marker2D>("Player2Spawn").Position;

            player1.Visible = false;
            player2.Visible = false;

            AddChild(player1);
            AddChild(player2);

            player1.canMove = false;
            player2.canMove = false;
        }

        AnimPlayer.AnimationFinished += OnAnimationFinished;
        AnimPlayer.Play("ReadyTextSlide");
    }

    private async void OnAnimationFinished(StringName animationName)
    {
        if (animationName == "Fight")
        {
            GetNode<AnimatedSprite2D>("IntroPlayer1").Visible = false;
            GetNode<AnimatedSprite2D>("IntroPlayer2").Visible = false;

            player1.canMove = true;
            player2.canMove = true;
            
            player1.Visible = true;
            player2.Visible = true;


            Timer hudTimer = GetNode<Timer>("CanvasLayer/HUD/Time/Timer");
            hudTimer.Start();
        }

        if (animationName == "ReadyTextSlide")
        {
            AnimPlayer.Play("Fight");
        }

        if (animationName == "Victory")
        {
            Global global = GetNode<Global>("/root/Global");
            global.ResetCharacterSelection();
            var transition = (SceneTransistion)GetNode("/root/Transition");
            await transition.TransitionToScene("res://main.tscn");
        }
    }
    private void OnVictoryAnimationFinished(Player winner)
    {
        AnimPlayer.Play("Victory");
    }
}
