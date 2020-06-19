using Godot;
using System;

public class BTLSCENE : Node2D
{
	AnimationPlayer AnimPlayer;
	
	Sprite EPSprite;
	Sprite PPSprite;
	
	TextureProgress EHealth;
	RichTextLabel EName;
	RichTextLabel ELevel;
	
	TextureProgress PHealth;
	RichTextLabel PName;
	RichTextLabel PLevel;
	RichTextLabel PHPText;
	
	TextureRect EBalls;
	TextureRect PBalls;
	
	static int EIndex = 0;
	static int PIndex = 0;
	
	Pokemon EPokemon = GlobalVars.Enemy.Party[EIndex];
	Pokemon PPokemon = GlobalVars.Player.Party[PIndex];
	
	string[] AnimQueue = new string[] {"BTLINIT", "ESLIDE", "ESEND", "PSLIDE", "PSEND"};
	
	public override void _Ready()
	{
		AnimPlayer = GetNode("ENGINES/ANIM") as AnimationPlayer;
		
		EPSprite = GetNode("MAIN/POKEMON/ENEMY") as Sprite;
		PPSprite = GetNode("MAIN/POKEMON/PLAYER") as Sprite;
		
		EHealth = GetNode("MAIN/BARS/ENEMY/VALUE") as TextureProgress;
		EName = GetNode("MAIN/BARS/ENEMY/NAME") as RichTextLabel;
		ELevel = GetNode("MAIN/BARS/ENEMY/LEVEL") as RichTextLabel;
		
		PHealth = GetNode("MAIN/BARS/PLAYER/VALUE") as TextureProgress;
		PName = GetNode("MAIN/BARS/PLAYER/NAME") as RichTextLabel;
		PLevel = GetNode("MAIN/BARS/PLAYER/LEVEL") as RichTextLabel;
		PHPText = GetNode("MAIN/BARS/PLAYER/HEALTH") as RichTextLabel;
		
		EBalls = GetNode("MAIN/POKEBALLS/ENEMY/VALUE") as TextureRect;
		PBalls = GetNode("MAIN/POKEBALLS/PLAYER/VALUE") as TextureRect;
		
		EPSprite.Frame = EPokemon.Species;
		PPSprite.Frame = PPokemon.Species;
		
		EBalls.RectSize = new Vector2(35 * GlobalVars.Enemy.Party.Count, 35);
		PBalls.RectSize = new Vector2(35 * GlobalVars.Player.Party.Count, 35);
		
		var ESEND = AnimPlayer.GetAnimation("ESEND");
		var PSEND = AnimPlayer.GetAnimation("PSEND");
		
		ESEND.TrackInsertKey(0, 1, string.Format("$% TRAINER sent out {0}!", Pokemon.PokeNames[EPSprite.Frame]));
		PSEND.TrackInsertKey(0, 1, string.Format("Go,  {0}!", Pokemon.PokeNames[PPSprite.Frame]));
		
		ELevel.Text = EPokemon.Level.ToString();
		EHealth.MaxValue = EPokemon.Stats[0];
		EHealth.Value = EPokemon.HP;
		EName.Text = Pokemon.PokeNames[EPokemon.Species];
		
		PLevel.Text = PPokemon.Level.ToString();
		PHealth.MaxValue = PPokemon.Stats[0];
		PHealth.Value = PPokemon.HP;
		PName.Text = Pokemon.PokeNames[PPokemon.Species];
		PHPText.BbcodeText = string.Format("[right]{0}/{1}[/right]", PPokemon.HP, PPokemon.Stats[0]);
		
		foreach (var Str in AnimQueue)
		AnimPlayer.Queue(Str);
	}
}
