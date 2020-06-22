using Godot;
using System;

using System.Linq;
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
	
	bool ProcessBool = false;
	int Processing = 0;
	int PAttackIndex = 0;
	
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
			ATTACKS[i].Connect("pressed", this, "AttackSelect", new Godot.Collections.Array() {ATTACKS[i]});
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
			Processing = 1;
		}
	}
	
	public void AttackSelect(Node SomeNode)
	{
		BattleBox.Visible = false;
		
		string SomePath = SomeNode.GetPath();
		SomePath = SomePath.Replace("/root/BTLSCENE/MAIN/MENUS/BTLBOX/ACTBOX/INPUT/BTN", "");
		PAttackIndex = int.Parse(SomePath) - 1;
		
		if (EPokemon.Stats[3] > PPokemon.Stats[3])
			Processing = 3;
		
		else
		Processing = 4;
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
				
				HPBAR.Value = ((float)Current.HP / (float)Current.Stats[0]) * 100F;
				HPBAR.MaxValue = 100;
				
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
		EHealth.MaxValue = 100;
		EHealth.Value = ((float)EPokemon.HP / (float)EPokemon.Stats[0]) * 100F;
		EName.Text = Pokemon.PokeNames[EPokemon.Species];
	}
	
	public void InitPlayer()
	{
		PCalling = false;
		
		PPokemon = GlobalVars.Player.Party[PIndex];
		PPSprite.Frame = PPokemon.Species;
		PBalls.RectSize = new Vector2(35 * GlobalVars.Player.Party.Count, 35);
		
		var PSEND = AnimPlayer.GetAnimation("PSEND");
		var PCALL = AnimPlayer.GetAnimation("PCALL");
		
		PSEND.TrackInsertKey(0, 1, string.Format("Go,  {0}!", Pokemon.PokeNames[PPSprite.Frame]));
		PCALL.TrackInsertKey(0, 1, string.Format("That's enough, {0}. Come back!", Pokemon.PokeNames[PPSprite.Frame]));
		
		PLevel.Text = PPokemon.Level.ToString();
		PHealth.MaxValue = 100;
		PHealth.Value = ((float)PPokemon.HP / (float)PPokemon.Stats[0]) * 100F;
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
		ProcessBool = true;
		
		if (EPokemon.HP > 0)
		{
			List<Move> ViableMoves = new List<Move>();
			
			bool CheckWeakness = true;
			bool CheckImmunity = true;
			bool UseStruggle = false;
			
			string Name = Pokemon.PokeNames[EPSprite.Frame];
			
			LoopLabel:
			for (int i = 0; i < EPokemon.Moveset.Count; i++)
			{
				Move a = EPokemon.Moveset[i];
				
				if (a.CurrentCount > 0)
					if (!CheckWeakness || !Move.Weaknesses[a.Type].Contains(PPokemon.Type))
						if (!CheckImmunity || !Move.Immunities[PPokemon.Type].Contains(a.Type))
							ViableMoves.Add(a);
			}
			
			if (ViableMoves.Count == 0)
			{
				if (CheckWeakness)
				{
					CheckWeakness = false;
					goto LoopLabel;
				}
				
				else if (CheckImmunity)
				{
					CheckImmunity = false;
					goto LoopLabel;
				}
				
				else
					UseStruggle = true;
			}
			
			if (UseStruggle)
			{
				byte Damage = (byte)(Math.Floor((double)Math.Floor((double)Math.Floor((double)(2 * EPokemon.Level / 5 + 2)) * EPokemon.Stats[1] * 50 / PPokemon.Stats[2]) / 50) + 2);
				byte Recoil = (byte)(Damage / 2);
				
				int PHP = PPokemon.HP - Damage;
				int EHP = EPokemon.HP - Recoil;
				
				PHP = PHP < 0 ? 0 : PHP;
				EHP = EHP < 0 ? 0 : EHP;
				
				var ESTRUGGLE = AnimPlayer.GetAnimation("ESTRUGGLE");
				
				ESTRUGGLE.TrackInsertKey(0, 1, string.Format("Foe {0} is out of usable moves.", Name));
				ESTRUGGLE.TrackInsertKey(0, 1.69F, string.Format("Foe {0} is out of usable moves.", Name));
				
				ESTRUGGLE.TrackInsertKey(0, 2.5F, string.Format("Foe {0} used STRUGGLE!", Name));
				ESTRUGGLE.TrackInsertKey(0, 4.99F, string.Format("Foe {0} used STRUGGLE!", Name));
				
				ESTRUGGLE.TrackInsertKey(0, 8, string.Format("Foe {0} is hurt by recoil.", Name));
				
				ESTRUGGLE.TrackInsertKey(3, 3.5F, ((float)PPokemon.HP / (float)PPokemon.Stats[0]) * 100F);
				ESTRUGGLE.TrackInsertKey(3, 4.5F, ((float)PHP / (float)PPokemon.Stats[0]) * 100F);
				ESTRUGGLE.TrackInsertKey(5, 4.5F, string.Format("[right]{0}/{1}[/right]", PHP, PPokemon.Stats[0]));
				
				ESTRUGGLE.TrackInsertKey(4, 5, ((float)EPokemon.HP / (float)EPokemon.Stats[0]) * 100F);
				ESTRUGGLE.TrackInsertKey(4, 6, ((float)EHP / (float)EPokemon.Stats[0]) * 100F);
				
				EPokemon.HP = (byte)EHP;
				PPokemon.HP = (byte)PHP;
				
				AnimPlayer.Play("ESTRUGGLE");
			}
			
			else
			{
				Random RNGEngine = new Random();
				
				int Idx = RNGEngine.Next(0, ViableMoves.Count);
				int Acc = RNGEngine.Next(0, 101);
				
				Move UsedMove = ViableMoves[Idx];
				string MoveName = Pokemon.MoveNames[UsedMove.ID];
				
				byte Damage = (byte)(Math.Floor((double)Math.Floor((double)Math.Floor((double)(2 * EPokemon.Level / 5 + 2)) * EPokemon.Stats[1] * UsedMove.Power / PPokemon.Stats[2]) / 50) + 2);
				UsedMove.CurrentCount -= 1;
				
				int PHP = PPokemon.HP - Damage;
				PHP = PHP < 0 ? 0 : PHP;
				
				if (Acc <= UsedMove.Accuracy)
				{
					var EATTACK = AnimPlayer.GetAnimation("EATTACK");
					
					EATTACK.TrackInsertKey(0, 1, string.Format("Foe {0} used {1}!", Name, MoveName));
					
					EATTACK.TrackInsertKey(3, 2, ((float)PPokemon.HP / (float)PPokemon.Stats[0]) * 100F);
					EATTACK.TrackInsertKey(3, 3F, ((float)PHP / (float)PPokemon.Stats[0]) * 100F);
					EATTACK.TrackInsertKey(4, 3F, string.Format("[right]{0}/{1}[/right]", PHP, PPokemon.Stats[0]));
					
					PPokemon.HP = (byte)PHP;
					
					AnimPlayer.Play("EATTACK");
				}
				
				else
				{
					var MISS = AnimPlayer.GetAnimation("MISS");
					
					MISS.TrackInsertKey(0, 1, string.Format("Foe {0} used {1}!", Name, MoveName));
					MISS.TrackInsertKey(0, 1.5F, string.Format("Foe {0} used {1}!", Name, MoveName));
					MISS.TrackInsertKey(0, 2.5F, string.Format("Foe {0}'s attack missed!", Name, MoveName));
					
					AnimPlayer.Play("MISS");
				}
			}
			
			if (Processing == 3)
				Processing = 2;
			
			else
			{
				Processing = 0;
				AnimPlayer.Queue("INITMENU");
			}
		}
		
		else
		{
			Processing = 0;
			AnimPlayer.Queue("INITMENU");
		}
		
		ProcessBool = false;
	}
	
	public void ProcessPlayer(int Idx)
	{
		ProcessBool = true;
		
		string Name = Pokemon.PokeNames[PPSprite.Frame];
		
		if (PPokemon.HP > 0)
		{
			if (Idx == -1)
			{
				byte Damage = (byte)(Math.Floor((double)Math.Floor((double)Math.Floor((double)(2 * PPokemon.Level / 5 + 2)) * PPokemon.Stats[1] * 50 / EPokemon.Stats[2]) / 50) + 2);
				byte Recoil = (byte)(Damage / 2);
				
				int PHP = PPokemon.HP - Damage;
				int EHP = EPokemon.HP - Recoil;
				
				PHP = PHP < 0 ? 0 : PHP;
				EHP = EHP < 0 ? 0 : EHP;
				
				var PSTRUGGLE = AnimPlayer.GetAnimation("PSTRUGGLE");
				
				PSTRUGGLE.TrackInsertKey(0, 1, string.Format("{0} is out of usable moves.", Name));
				PSTRUGGLE.TrackInsertKey(0, 1.69F, string.Format("{0} is out of usable moves.", Name));
				
				PSTRUGGLE.TrackInsertKey(0, 2.5F, string.Format("{0} used STRUGGLE!", Name));
				PSTRUGGLE.TrackInsertKey(0, 4.99F, string.Format("{0} used STRUGGLE!", Name));
				
				PSTRUGGLE.TrackInsertKey(0, 8, string.Format("{0} is hurt by recoil.", Name));
				
				PSTRUGGLE.TrackInsertKey(2, 5F, ((float)PPokemon.HP / (float)PPokemon.Stats[0]) * 100F);
				PSTRUGGLE.TrackInsertKey(2, 6F, ((float)PHP / (float)PPokemon.Stats[0]) * 100F);
				PSTRUGGLE.TrackInsertKey(2, 6F, string.Format("[right]{0}/{1}[/right]", PHP, PPokemon.Stats[0]));
				
				PSTRUGGLE.TrackInsertKey(3, 3.5F, ((float)EPokemon.HP / (float)EPokemon.Stats[0]) * 100F);
				PSTRUGGLE.TrackInsertKey(3, 4.5F, ((float)EHP / (float)EPokemon.Stats[0]) * 100F);
				
				EPokemon.HP = (byte)EHP;
				PPokemon.HP = (byte)PHP;
				
				AnimPlayer.Play("PSTRUGGLE");
			}
			
			else
			{
				Random RNGEngine = new Random();
				
				Move UsedMove = PPokemon.Moveset[Idx];
				int Acc = RNGEngine.Next(0, 101);
				
				string MoveName = Pokemon.MoveNames[UsedMove.ID];
				
				byte Damage = (byte)(Math.Floor((double)Math.Floor((double)Math.Floor((double)(2 * PPokemon.Level / 5 + 2)) * PPokemon.Stats[1] * UsedMove.Power / EPokemon.Stats[2]) / 50) + 2);
				UsedMove.CurrentCount -= 1;
				
				int EHP = EPokemon.HP - Damage;
				EHP = EHP < 0 ? 0 : EHP;
				
				if (Acc <= UsedMove.Accuracy)
				{
					var PATTACK = AnimPlayer.GetAnimation("PATTACK");
					
					PATTACK.TrackInsertKey(0, 1, string.Format("{0} used {1}!", Name, MoveName));
					
					PATTACK.TrackInsertKey(2, 2, ((float)EPokemon.HP / (float)EPokemon.Stats[0]) * 100F);
					PATTACK.TrackInsertKey(2, 3F, ((float)EHP / (float)EPokemon.Stats[0]) * 100F);
					
					EPokemon.HP = (byte)EHP;
					
					AnimPlayer.Play("PATTACK");
				}
				
				else
				{
					var MISS = AnimPlayer.GetAnimation("MISS");
					
					MISS.TrackInsertKey(0, 1, string.Format("{0} used {1}!", Name, MoveName));
					MISS.TrackInsertKey(0, 1.5F, string.Format("{0} used {1}!", Name, MoveName));
					MISS.TrackInsertKey(0, 2.5F, string.Format("{0}'s attack missed!", Name, MoveName));
					
					AnimPlayer.Play("MISS");
				}
			}
			
			if (Processing == 4)
				Processing = 1;
			
			else
			{
				Processing = 0;
				AnimPlayer.Queue("INITMENU");
			}
		}
		
		else
		{
			Processing = 0;
			AnimPlayer.Queue("INITMENU");
		}
		
		ProcessBool = false;
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
			
		if ((Processing == 3 || Processing == 1) && !ProcessBool && !AnimPlayer.IsPlaying())
			ProcessEnemy();
			
		else if ((Processing == 4 || Processing == 2)  && !ProcessBool && !AnimPlayer.IsPlaying())
			ProcessPlayer(PAttackIndex);
	}
}
