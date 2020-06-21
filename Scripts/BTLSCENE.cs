using Godot;
using System;

using System.Collections.Generic;

public class BTLSCENE : Node2D
{
	AnimationPlayer AnimPlayer;
	AnimationPlayer PokePlayer;
	
	Sprite EPSprite;
	Sprite PPSprite;
	
	TextureProgress EHealth;
	RichTextLabel EName;
	RichTextLabel ELevel;
	
	TextureProgress PHealth;
	RichTextLabel PName;
	RichTextLabel PLevel;
	RichTextLabel PHPText;
	
	List<TextureButton> ACTIONS;
	List<TextureButton> ATTACKS;
	
	List<TextureButton> POKESELECT;
	
	Control InfoBox;
	Control ActionBox;
	Control BattleBox;
	Control PokeBox;
	
	TextureRect EBalls;
	TextureRect PBalls;
	
	AudioStreamPlayer MusicPlayer;
	
	static int EIndex = 0;
	static int PIndex = 0;
	
	bool PCalling = false;
	bool ECalling = false;
	
	byte PrevHighlight = 0;
	
	Pokemon EPokemon;
	Pokemon PPokemon;
	
	string[] AnimQueue = new string[] {"BTLINIT", "ESLIDE", "ESEND", "PSLIDE", "PSEND", "INITMENU"};
	
	public void Preload()
	{
		MusicPlayer = GetNode("ENGINES/MUSIC") as AudioStreamPlayer;
		MusicPlayer.Play(3.05F);
		
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
		
		ActionBox = GetNode("MAIN/MENUS/ACTBOX") as Control;
		BattleBox = GetNode("MAIN/MENUS/BTLBOX") as Control;
		InfoBox = GetNode("MAIN/MENUS/INFBOX") as Control;
		PokeBox = GetNode("MAIN/MENUS/POKBOX") as Control;
		
		PokePlayer = PokeBox.GetNode("POKEANIM") as AnimationPlayer;
		
		ACTIONS = new List<TextureButton>();
		ATTACKS = new List<TextureButton>();
		POKESELECT = new List<TextureButton>();
		
		for (int i = 0; i < 4; i++)
		{
			ACTIONS.Add(ActionBox.GetNode(string.Format("INPUT/BTN{0}", i + 1)) as TextureButton);
			ATTACKS.Add(BattleBox.GetNode(string.Format("ACTBOX/INPUT/BTN{0}", i + 1)) as TextureButton);
		}
		
		for (int i = 0; i < 6; i++)
		{
			POKESELECT.Add(PokeBox.GetNode(string.Format("INPUT/POKESEL{0}", i + 1)) as TextureButton);
			POKESELECT[i].Connect("pressed", this, "PokeSelect", new Godot.Collections.Array() {POKESELECT[i]});
			POKESELECT[i].Connect("mouse_entered", this, "PokeHover", new Godot.Collections.Array() {POKESELECT[i]});
		}
	}
	
	public void PokeHover(Node SomeNode)
	{
		string SomePath = SomeNode.GetPath();
		SomePath = SomePath.Replace("/root/BTLSCENE/MAIN/MENUS/POKBOX/", "");
		
		Animation PokeAnim = PokePlayer.GetAnimation("POKEPLAY");
		
		if (PokeAnim.FindTrack(SomePath + "/POKE:frame") == -1)
			PokeAnim.TrackSetPath(0, SomePath + "/POKE:frame");
			
		if (!PokePlayer.IsPlaying())
			PokePlayer.Play("POKEPLAY");
	}
	
	public void PokeSelect(Node SomeNode)
	{
		string SomePath = SomeNode.GetPath();
		SomePath = SomePath.Replace("/root/BTLSCENE/MAIN/MENUS/POKBOX/INPUT/POKESEL", "");
		int NPIndex = int.Parse(SomePath) - 1;
		
		if (NPIndex != PIndex)
		{
			InfoBox.Visible = true;
			PokeBox.Visible = false;
			
			PIndex = NPIndex;
			
			AnimPlayer.Play("PCALL");
			PCalling = true;
			
			AnimPlayer.Queue("PSEND");
			ProcessEnemy();
		}
	}
	
	public void ExecuteMenus()
	{
		if (ACTIONS[0].IsPressed())
		{
			ActionBox.Visible = false;
			
			RichTextLabel TYPETXT = BattleBox.GetNode("INFBOX/TEXT/TYPE") as RichTextLabel;
			RichTextLabel COUNTTXT = BattleBox.GetNode("INFBOX/TEXT/COUNT") as RichTextLabel;
			
			TYPETXT.BbcodeText = string.Format("[center]{0}[/center]", Pokemon.TypeNames[PPokemon.Moveset[0].Type]);
			COUNTTXT.BbcodeText = string.Format("[right]{0}/{1}[/right]", PPokemon.Moveset[0].CurrentCount, PPokemon.Moveset[0].Count);
			
			BattleBox.Visible = true;
		}
		
		if (ACTIONS[1].IsPressed())
		{
			ActionBox.Visible = false;
			InfoBox.Visible = false;
			
			PokeBox.Visible = true;
			
			for (int i = 0; i < GlobalVars.Player.Party.Count; i++)
			{
				Pokemon Current = GlobalVars.Player.Party[i];
				
				RichTextLabel NAME = POKESELECT[i].GetNode("NAME") as RichTextLabel;
				RichTextLabel LEVEL = POKESELECT[i].GetNode("LEVEL") as RichTextLabel;
				RichTextLabel VALUE = POKESELECT[i].GetNode("VALUE") as RichTextLabel;
				TextureProgress HPBAR = POKESELECT[i].GetNode("HPBAR") as TextureProgress;
				
				NAME.Text = Pokemon.PokeNames[Current.Species];
				LEVEL.Text = Current.Level.ToString();
				VALUE.BbcodeText = string.Format("[right]{0}/{1}[/right]", Current.HP, Current.Stats[0]);
				
				HPBAR.Value = Current.HP;
				HPBAR.MaxValue = Current.Stats[0];
				
				POKESELECT[i].Visible = true;
			}
			
			AnimPlayer.Play("PKMNINIT");
		}
		
		if (ACTIONS[3].IsPressed())
		{
			AnimPlayer.Play("NORUN");
			AnimPlayer.Queue("INITMENU");
		}
		
		if (!ActionBox.IsVisible() && Input.IsActionJustPressed("CANCEL"))
		{
			ActionBox.Visible = true;
			InfoBox.Visible = true;
			BattleBox.Visible = false;
			PokeBox.Visible = false;
		}
	}
	
	public void InfoLoop()
	{
		if (BattleBox.IsVisible())
		for (byte i = 0; i < ATTACKS.Count; i++)
		{
			if (ATTACKS[i].IsHovered() && !ATTACKS[i].IsDisabled())
			{
				if (i == PrevHighlight)
				break;
				
				else
				{
					PrevHighlight = i;
					
					RichTextLabel TYPETXT = BattleBox.GetNode("INFBOX/TEXT/TYPE") as RichTextLabel;
					RichTextLabel COUNTTXT = BattleBox.GetNode("INFBOX/TEXT/COUNT") as RichTextLabel;
					
					TYPETXT.BbcodeText = string.Format("[center]{0}[/center]", Pokemon.TypeNames[PPokemon.Moveset[i].Type]);
					COUNTTXT.BbcodeText = string.Format("[right]{0}/{1}[/right]", PPokemon.Moveset[i].CurrentCount, PPokemon.Moveset[i].Count);
					
					break;
				}
			}
		}
	}
	
	public void InitEnemy()
	{
		EPokemon = GlobalVars.Enemy.Party[EIndex];
		EPSprite.Frame = EPokemon.Species;
		EBalls.RectSize = new Vector2(35 * GlobalVars.Enemy.Party.Count, 35);
		
		var ESEND = AnimPlayer.GetAnimation("ESEND");
		ESEND.TrackInsertKey(0, 1, string.Format("$% TRAINER sent out {0}!", Pokemon.PokeNames[EPSprite.Frame]));
		
		ELevel.Text = EPokemon.Level.ToString();
		EHealth.MaxValue = EPokemon.Stats[0];
		EHealth.Value = EPokemon.HP;
		EName.Text = Pokemon.PokeNames[EPokemon.Species];
	}
	
	public void InitPlayer()
	{
		PPokemon = GlobalVars.Player.Party[PIndex];
		PPSprite.Frame = PPokemon.Species;
		PBalls.RectSize = new Vector2(35 * GlobalVars.Player.Party.Count, 35);
		
		var PSEND = AnimPlayer.GetAnimation("PSEND");
		var PCALL = AnimPlayer.GetAnimation("PCALL");
		
		PSEND.TrackInsertKey(0, 1, string.Format("Go,  {0}!", Pokemon.PokeNames[PPSprite.Frame]));
		PCALL.TrackInsertKey(0, 1, string.Format("That's enough, {0}. Come back!", Pokemon.PokeNames[PPSprite.Frame]));
		
		PLevel.Text = PPokemon.Level.ToString();
		PHealth.MaxValue = PPokemon.Stats[0];
		PHealth.Value = PPokemon.HP;
		PName.Text = Pokemon.PokeNames[PPokemon.Species];
		PHPText.BbcodeText = string.Format("[right]{0}/{1}[/right]", PPokemon.HP, PPokemon.Stats[0]);
		
		for (int i = 0; i < 4; i++)
		{
			Control ROOT = GetNode("MAIN/MENUS/BTLBOX") as Control;
			TextureButton ATK = ROOT.GetNode(string.Format("ACTBOX/INPUT/BTN{0}", i + 1)) as TextureButton;
			
			if (i < PPokemon.Moveset.Count)
			{
				ATK.Disabled = false;
				
				RichTextLabel TXT = ATK.GetNode("TXT") as RichTextLabel;
				TXT.Text = Pokemon.MoveNames[PPokemon.Moveset[i].ID];
			}
			
			else
			{
				ATK.Disabled = true;
				
				RichTextLabel TXT = ATK.GetNode("TXT") as RichTextLabel;
				TXT.Text = "-";
			}
		}
	}
	
	public void ProcessEnemy()
	{
		AnimPlayer.Queue("INITMENU");
	}
	
	public override void _Ready()
	{
		Preload();
		
		InitEnemy();
		InitPlayer();
		
		foreach (var Str in AnimQueue)
		AnimPlayer.Queue(Str);
	}
	
	public override void _Process(float delta)
	{
		ExecuteMenus();
		InfoLoop();
		
		if (PCalling && AnimPlayer.CurrentAnimation == "PSEND")
			InitPlayer();
	}
}
