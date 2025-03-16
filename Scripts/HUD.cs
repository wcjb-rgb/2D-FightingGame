using Godot;
using System;

public partial class HUD : Control
{
    private int totalTime = 99;  
    private Timer timer;
    private TextureRect onesSprite;
    private TextureRect tensSprite;

	private TextureRect player1Portrait;
    private TextureRect player2Portrait;

    [Export] public Godot.Collections.Array<Texture> timeSprites;

    public override void _Ready()
    {
        timer = GetNode<Timer>("Time/Timer");

        onesSprite = GetNode<TextureRect>("Time/Ones");
        tensSprite = GetNode<TextureRect>("Time/Tens");

        timer.Timeout += OnTimerTimeout;

        timer.WaitTime = 1.0f;
        timer.Start();

        UpdateTimerDisplay();

		player1Portrait = GetNode<TextureRect>("Healthbar/Player1Portrait");
        player2Portrait = GetNode<TextureRect>("Healthbar/Player2Portrait");

		AssignPlayerPortraits();
    }

    private void OnTimerTimeout()
    {
        if (totalTime > 0)
        {
            totalTime -= 1; 
            UpdateTimerDisplay();
        }
        
        if (totalTime == 0)
        {
            timer.Stop();
        }
    }

    private void UpdateTimerDisplay()
    {
        int ones = totalTime % 10;
    int tens = totalTime / 10;

    if (ones >= 0 && ones < timeSprites.Count)
        onesSprite.Texture = (Texture2D)timeSprites[ones]; 

    if (tens >= 0 && tens < timeSprites.Count)
        tensSprite.Texture = (Texture2D)timeSprites[tens];
    }

	private void AssignPlayerPortraits()
    {
        Global global = GetNode<Global>("/root/Global");

        if (global.Player1Character != null)
        {
            Node tempInstance = global.Player1Character.Instantiate();
            if (tempInstance is Player player)
            {
                player1Portrait.Texture = player.GetPortrait();
                tempInstance.QueueFree();
            }
        }

        if (global.Player2Character != null)
        {
            Node tempInstance = global.Player2Character.Instantiate();
            if (tempInstance is Player player)
            {
                player2Portrait.Texture = player.GetPortrait();
                tempInstance.QueueFree();
            }
        }
    }
}
