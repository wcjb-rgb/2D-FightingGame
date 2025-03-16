using Godot;
using System.Collections.Generic;

public partial class HealthBarManager : Control
{
    private Dictionary<string, TextureProgressBar> healthBars = new Dictionary<string, TextureProgressBar>();

    public override void _Ready()
    {
        GD.Print("✅ HealthBarManager Loaded!");

        healthBars["Player1"] = GetNodeOrNull<TextureProgressBar>("Player1Healthbar");
        healthBars["Player2"] = GetNodeOrNull<TextureProgressBar>("Player2Healthbar");

        foreach (var bar in healthBars.Values)
        {
            bar.MaxValue = 100;
            bar.Value = 100;
 
        }
    }

    public void UpdateHealthBar(string playerName, int currentHealth, int maxHealth)
{
    if (healthBars.ContainsKey(playerName) && healthBars[playerName] != null)
    {
        healthBars[playerName].Value = currentHealth;
    }
}
}
