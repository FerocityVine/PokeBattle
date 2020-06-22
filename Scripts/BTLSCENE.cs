using Godot;

using System;
using System.Linq;
using System.Collections.Generic;

public class BTLSCENE : Node2D
{
	AnimationPlayer AnimEngine_Main;
	AnimationPlayer AnimEngine_Poke;

	AudioStreamPlayer MusicEngine;
	
	List<Sprite> PokeSprite; // Player, Enemy
	List<Pokemon> LPokemon;
	
	TextureProgress EnemyBar;
	List<RichTextLabel> EnemyInfo; // Name, Level
	
	TextureProgress PlayerBar;
	List<RichTextLabel> PlayerInfo; // Name, Level, Health
	
	List<TextureButton> Action;
	List<TextureButton> Battle;
	
	List<TextureButton> PokemonEntry;
	
	List<Control> Menu; // Info, Action, Battle, Pokemon, Item
	
	List<TextureRect> PokeIndicator;
	List<Trainer> Trainers;

	List<int> PokeIndex;
	List<bool> PokeSwitch;
	
	byte PastSelection = 0;
	
	bool Battling = false;
	int BattleOrder = 0; // 4 = Player -> Enemy | 2 = Enemy -> Player | 3 = Enemy | 1 = Player 

	bool AcceptInput = true;

	int AttackIndex;
	string[] AnimQueue = new string[] {"BTLINIT", "ESLIDE", "ESEND", "PSLIDE", "PSEND", "INITMENU"};
	
	/* Initializers */

	public void Preload()
	{
		Trainers = new List<Trainer>() { GlobalVars.Enemy, GlobalVars.Player };
		
		PokeIndex = new List<int>() { 0, 0 };
		PokeSwitch = new List<bool>() { false, false };
		
		LPokemon = new List<Pokemon>() { Trainers[0].Party[PokeIndex[0]], Trainers[1].Party[PokeIndex[1]] };
		
		MusicEngine = GetNode("ENGINES/MUSIC") as AudioStreamPlayer;
		MusicEngine.Play(3.05F);
		
		AnimEngine_Main = GetNode("ENGINES/ANIM") as AnimationPlayer;

		PokeSprite = new List<Sprite>();
		PokeSprite.Add(GetNode("MAIN/POKEMON/ENEMY") as Sprite);
		PokeSprite.Add(GetNode("MAIN/POKEMON/PLAYER") as Sprite);

		EnemyBar = GetNode("MAIN/BARS/ENEMY/VALUE") as TextureProgress;
		PlayerBar = GetNode("MAIN/BARS/PLAYER/VALUE") as TextureProgress;

		EnemyInfo = new List<RichTextLabel>();
		EnemyInfo.Add(GetNode("MAIN/BARS/ENEMY/NAME") as RichTextLabel);
		EnemyInfo.Add(GetNode("MAIN/BARS/ENEMY/LEVEL") as RichTextLabel);
		
		PlayerInfo = new List<RichTextLabel>();
		PlayerInfo.Add(GetNode("MAIN/BARS/PLAYER/NAME") as RichTextLabel);
		PlayerInfo.Add(GetNode("MAIN/BARS/PLAYER/LEVEL") as RichTextLabel);
		PlayerInfo.Add(GetNode("MAIN/BARS/PLAYER/HEALTH") as RichTextLabel);

		PokeIndicator = new List<TextureRect>();
		PokeIndicator.Add(GetNode("MAIN/POKEBALLS/ENEMY/VALUE") as TextureRect);
		PokeIndicator.Add(GetNode("MAIN/POKEBALLS/PLAYER/VALUE") as TextureRect);
		
		Menu = new List<Control>();
		Menu.Add(GetNode("MAIN/MENUS/INFBOX") as Control);
		Menu.Add(GetNode("MAIN/MENUS/ACTBOX") as Control);
		Menu.Add(GetNode("MAIN/MENUS/BTLBOX") as Control);
		Menu.Add(GetNode("MAIN/MENUS/POKBOX") as Control);
		
		AnimEngine_Poke = Menu[3].GetNode("POKEANIM") as AnimationPlayer;
		
		Action = new List<TextureButton>();
		Battle = new List<TextureButton>();
		PokemonEntry = new List<TextureButton>();
		
		for (int i = 0; i < 4; i++)
		{
			Action.Add(Menu[1].GetNode(string.Format("INPUT/BTN{0}", i + 1)) as TextureButton);
			Battle.Add(Menu[2].GetNode(string.Format("ACTBOX/INPUT/BTN{0}", i + 1)) as TextureButton);
			Battle[i].Connect("pressed", this, "AttackSelect", new Godot.Collections.Array() {Battle[i]});
		}
		
		for (int i = 0; i < 6; i++)
		{
			PokemonEntry.Add(Menu[3].GetNode(string.Format("INPUT/POKESEL{0}", i + 1)) as TextureButton);
			PokemonEntry[i].Connect("pressed", this, "PokeSelect", new Godot.Collections.Array() {PokemonEntry[i]});
			PokemonEntry[i].Connect("mouse_entered", this, "PokeHover", new Godot.Collections.Array() {PokemonEntry[i]});
		}
	}

	public void InitEnemy()
	{
		PokeSwitch[0] = false;

		LPokemon[0] = Trainers[0].Party[PokeIndex[0]];

		PokeSprite[0].Frame = LPokemon[0].Species;
		PokeIndicator[0].RectSize = new Vector2(35 * Trainers[0].Party.Count, 35);
		
		var Animation = AnimEngine_Main.GetAnimation("ESEND");
		Animation.TrackInsertKey(0, 1, string.Format("$% TRAINER sent out {0}!", Pokemon.PokeNames[PokeSprite[0].Frame]));
		
		EnemyInfo[0].Text = Pokemon.PokeNames[LPokemon[0].Species];
		EnemyInfo[1].Text = LPokemon[0].Level.ToString();

		EnemyBar.MaxValue = 100;
		EnemyBar.Value = ((float)LPokemon[0].Health / (float)LPokemon[0].Stats[0]) * 100F;
	}

	public void InitPlayer()
	{
		PokeSwitch[1] = false;
		
		LPokemon[1] = Trainers[1].Party[PokeIndex[1]];

		PokeSprite[1].Frame = LPokemon[1].Species;
		PokeIndicator[1].RectSize = new Vector2(35 * Trainers[1].Party.Count, 35);
		
		var Animation1 = AnimEngine_Main.GetAnimation("PSEND");
		var Animation2 = AnimEngine_Main.GetAnimation("PCALL");

		Animation1.TrackInsertKey(0, 1, string.Format("Go,  {0}!", Pokemon.PokeNames[PokeSprite[1].Frame]));
		Animation2.TrackInsertKey(0, 1, string.Format("That's enough, {0}. Come back!", Pokemon.PokeNames[PokeSprite[1].Frame]));

		PlayerInfo[0].Text = Pokemon.PokeNames[LPokemon[1].Species];
		PlayerInfo[1].Text = LPokemon[1].Level.ToString();
		PlayerInfo[2].BbcodeText = string.Format("[right]{0}/{1}[/right]", LPokemon[1].Health, LPokemon[1].Stats[0]);

		PlayerBar.MaxValue = 100;
		PlayerBar.Value = ((float)LPokemon[1].Health / (float)LPokemon[1].Stats[0]) * 100F;

		for (int i = 0; i < 4; i++)
		{
			if (i < LPokemon[1].Moveset.Count)
			{
				Battle[i].Disabled = false;
				
				RichTextLabel Label = Battle[i].GetNode("TXT") as RichTextLabel;
				Label.Text = Pokemon.MoveNames[LPokemon[1].Moveset[i].ID];
			}
			
			else
			{
				Battle[i].Disabled = true;
				
				RichTextLabel Label = Battle[i].GetNode("TXT") as RichTextLabel;
				Label.Text = "-";
			}
		}
	}

	/* Input Handlers */

	public void ExecuteMenus()
	{
		if (AcceptInput)
		{
			if (Action[0].Pressed)
			{
				Menu[1].Visible = false;
				Menu[2].Visible = true;

				InfoLoop();
			}
			
			if (Action[1].Pressed)
			{
				Menu[0].Visible = false;
				Menu[1].Visible = false;
				
				foreach (var PokeButton in PokemonEntry)
					PokeButton.Visible = false;

				Menu[3].Visible = true;
				
				for (int i = 0; i < Trainers[1].Party.Count; i++)
				{
					Pokemon Poke = Trainers[1].Party[i];
					
					RichTextLabel NAME = PokemonEntry[i].GetNode("NAME") as RichTextLabel;
					RichTextLabel LEVEL = PokemonEntry[i].GetNode("LEVEL") as RichTextLabel;
					RichTextLabel VALUE = PokemonEntry[i].GetNode("VALUE") as RichTextLabel;
					TextureProgress HPBAR = PokemonEntry[i].GetNode("HPBAR") as TextureProgress;
					
					NAME.Text = Pokemon.PokeNames[Poke.Species];
					LEVEL.Text = Poke.Level.ToString();
					VALUE.BbcodeText = string.Format("[right]{0}/{1}[/right]", Poke.Health, Poke.Stats[0]);
					
					HPBAR.Value = ((float)Poke.Health / (float)Poke.Stats[0]) * 100F;
					HPBAR.MaxValue = 100;
					
					PokemonEntry[i].Visible = true;
				}
				
				AnimEngine_Main.Play("PKMNINIT");
			}
			
			if (Action[3].Pressed)
			{
				AnimEngine_Main.Play("NORUN");
				AnimEngine_Main.Queue("INITMENU");
			}
			
			if (!Menu[1].Visible && Input.IsActionJustPressed("CANCEL"))
			{
				Menu[0].Visible = true;
				Menu[1].Visible = true;
				Menu[2].Visible = false;
				Menu[3].Visible = false;
			}
		}
	}

	public void InfoLoop()
	{
		if (Menu[2].Visible)
		{
			int PassCount = 0;
			
			for (byte i = 0; i < Battle.Count; i++)
			{
				if (Battle[i].IsHovered() && !Battle[i].Disabled)
				{
					if (i == PastSelection)
						break;
					
					else
					{
						Move HMove = LPokemon[1].Moveset[i];
						PastSelection = i;
						
						RichTextLabel TypeLabel = Menu[2].GetNode("INFBOX/TEXT/TYPE") as RichTextLabel;
						RichTextLabel CountLabel = Menu[2].GetNode("INFBOX/TEXT/COUNT") as RichTextLabel;
						
						TypeLabel.BbcodeText = string.Format("[center]{0}[/center]", Pokemon.TypeNames[HMove.Type]);
						CountLabel.BbcodeText = string.Format("[right]{0}/{1}[/right]", HMove.CurrentCount, HMove.Count);
						
						break;
					}
				}
				
				else
					PassCount++;
			}
			
			if (PassCount == Battle.Count)
			{
				Move HMove = LPokemon[1].Moveset[0];
				PastSelection = 0;
				
				RichTextLabel TypeLabel = Menu[2].GetNode("INFBOX/TEXT/TYPE") as RichTextLabel;
				RichTextLabel CountLabel = Menu[2].GetNode("INFBOX/TEXT/COUNT") as RichTextLabel;
				
				TypeLabel.BbcodeText = string.Format("[center]{0}[/center]", Pokemon.TypeNames[HMove.Type]);
				CountLabel.BbcodeText = string.Format("[right]{0}/{1}[/right]", HMove.CurrentCount, HMove.Count);
			}
		}
	}

	public void AttackSelect(Node Sender)
	{
		Menu[2].Visible = false;
		
		string Path = Sender.GetPath();
		AttackIndex = int.Parse(Path.Substring(Path.Length - 1)) - 1;
		
		if (LPokemon[0].Stats[3] > LPokemon[1].Stats[3])
			BattleOrder = 2;
		
		else
			BattleOrder = 4;
	}

	public void PokeHover(Node Sender)
	{
		string Path = Sender.GetPath();
		int Index = int.Parse(Path.Substring(Path.Length - 1)) - 1;
		
		Pokemon HPokemon = Trainers[1].Party[Index];
		Path = Path.Replace("/root/BTLSCENE/MAIN/MENUS/POKBOX/", "");
		
		Animation Event = AnimEngine_Poke.GetAnimation("POKEPLAY");
		
		if (Event.FindTrack(Path + "/POKE:frame") == -1)
			Event.TrackSetPath(0, Path + "/POKE:frame");
			
		if (!AnimEngine_Poke.IsPlaying())
			AnimEngine_Poke.Play("POKEPLAY");
	}
	
	public void PokeSelect(Node Sender)
	{
		string Path = Sender.GetPath();
		int Index = int.Parse(Path.Substring(Path.Length - 1)) - 1;

		if (Index != PokeIndex[1])
		{
			Menu[0].Visible = true;
			Menu[3].Visible = false;
			
			PokeIndex[1] = Index;
			
			if (LPokemon[1].Health > 0)
			{
				AnimEngine_Main.Play("PCALL");
				AnimEngine_Main.Queue("PSEND");

				PokeSwitch[1] = true;
				BattleOrder = 3;
			}

			else 
				AnimEngine_Main.Play("PSEND");
		}
	}

	/* Mechanic Handlers */

	public void ProcessEnemy()
	{
		Battling = true;
		AcceptInput = false;
		
		if (LPokemon[0].Health > 0)
		{
			List<Move> ViableMoves = new List<Move>();
			
			bool CWeakness = true;
			bool CImmunity = true;
			bool Struggling = false;
			
			string Name = Pokemon.PokeNames[PokeSprite[0].Frame];
			
			JumpPoint1:
			for (int i = 0; i < LPokemon[0].Moveset.Count; i++)
			{
				Move HMove = LPokemon[0].Moveset[i];
				
				if (HMove.CurrentCount > 0)
					if (!CWeakness || !Move.Weaknesses[HMove.Type].Contains(LPokemon[1].Type))
						if (!CImmunity || !Move.Immunities[LPokemon[1].Type].Contains(HMove.Type))
							ViableMoves.Add(HMove);
			}
			
			if (ViableMoves.Count == 0)
			{
				if (CWeakness)
				{
					CWeakness = false;
					goto JumpPoint1;
				}
				
				else if (CImmunity)
				{
					CImmunity = false;
					goto JumpPoint1;
				}
				
				else
					Struggling = true;
			}
			
			if (Struggling)
			{
				double DF1 = Math.Floor((2D * (double)LPokemon[0].Level / 5D + 2D));
				double DF2 = Math.Floor(DF1 * (double)LPokemon[0].Stats[1] * 50D / (double)LPokemon[1].Stats[2]);
				double DF3 = Math.Floor(DF2 / 50D) + 2;

				byte Damage = (byte)DF3;
				byte Recoil = (byte)(Damage / 2);

				int EnemyHealth = LPokemon[0].Health - Recoil;
				int PlayerHealth = LPokemon[1].Health - Damage;

				EnemyHealth = EnemyHealth < 0 ? 0 : EnemyHealth;
				PlayerHealth = PlayerHealth < 0 ? 0 : PlayerHealth;
				
				var Animation = AnimEngine_Main.GetAnimation("ESTRUGGLE");
				
				Animation.TrackInsertKey(0, 1, string.Format("Foe {0} is out of usable moves.", Name));
				Animation.TrackInsertKey(0, 1.69F, string.Format("Foe {0} is out of usable moves.", Name));
				
				Animation.TrackInsertKey(0, 2.5F, string.Format("Foe {0} used STRUGGLE!", Name));
				Animation.TrackInsertKey(0, 4.99F, string.Format("Foe {0} used STRUGGLE!", Name));
				
				Animation.TrackInsertKey(0, 8, string.Format("Foe {0} is hurt by recoil.", Name));
				
				Animation.TrackInsertKey(3, 4.5F, ((float)PlayerHealth / (float)LPokemon[1].Stats[0]) * 100F);
				Animation.TrackInsertKey(3, 3.5F, ((float)LPokemon[1].Health / (float)LPokemon[1].Stats[0]) * 100F);
				Animation.TrackInsertKey(5, 4.5F, string.Format("[right]{0}/{1}[/right]", PlayerHealth, LPokemon[1].Stats[0]));
				
				
				Animation.TrackInsertKey(4, 6, ((float)EnemyHealth / (float)LPokemon[0].Stats[0]) * 100F);
				Animation.TrackInsertKey(4, 5, ((float)LPokemon[0].Health / (float)LPokemon[0].Stats[0]) * 100F);

				LPokemon[0].Health = (byte)EnemyHealth;
				LPokemon[1].Health = (byte)PlayerHealth;
				
				AnimEngine_Main.Play("ESTRUGGLE");
			}
			
			else
			{
				Random RNGEngine = new Random();
				
				int Index = RNGEngine.Next(0, ViableMoves.Count);
				int HitRange = RNGEngine.Next(0, 101);
				
				Move HMove = ViableMoves[Index];
				string MoveName = Pokemon.MoveNames[HMove.ID];

				double DF1 = Math.Floor((2D * (double)LPokemon[0].Level / 5D + 2D));
				double DF2 = Math.Floor(DF1 * (double)LPokemon[0].Stats[1] * (double)HMove.Power / (double)LPokemon[1].Stats[2]);
				double DF3 = Math.Floor(DF2 / 50D) + 2;
				
				byte Damage = (byte)DF3;

				HMove.CurrentCount -= 1;
				
				int PlayerHealth = LPokemon[1].Health - Damage;
				PlayerHealth = PlayerHealth < 0 ? 0 : PlayerHealth;
				
				if (HitRange <= HMove.Accuracy)
				{
					var Animation = AnimEngine_Main.GetAnimation("EATTACK");
					
					Animation.TrackInsertKey(0, 1, string.Format("Foe {0} used {1}!", Name, MoveName));

					Animation.TrackInsertKey(3, 3F, ((float)PlayerHealth / (float)LPokemon[1].Stats[0]) * 100F);
					Animation.TrackInsertKey(3, 2, ((float)LPokemon[1].Health / (float)LPokemon[1].Stats[0]) * 100F);
					Animation.TrackInsertKey(4, 3F, string.Format("[right]{0}/{1}[/right]", PlayerHealth, LPokemon[1].Stats[0]));
					
					LPokemon[1].Health = (byte)PlayerHealth;
					
					AnimEngine_Main.Play("EATTACK");
				}
				
				else
				{
					var Animation = AnimEngine_Main.GetAnimation("MISS");
					
					Animation.TrackInsertKey(0, 1, string.Format("Foe {0} used {1}!", Name, MoveName));
					Animation.TrackInsertKey(0, 1.5F, string.Format("Foe {0} used {1}!", Name, MoveName));
					Animation.TrackInsertKey(0, 2.5F, string.Format("Foe {0}'s attack missed!", Name, MoveName));
					
					AnimEngine_Main.Play("MISS");
				}
			}
			
			if (BattleOrder == 2)
				BattleOrder--;
			
			else
			{
				BattleOrder = 0;
				AnimEngine_Main.Queue("INITMENU");
			}
		}
		
		else
		{
			BattleOrder = 0;
			AnimEngine_Main.Queue("INITMENU");
		}
		
		Battling = false;
	}

	public void ProcessPlayer(int Index)
	{
		Battling = true;
		AcceptInput = false;
		
		string Name = Pokemon.PokeNames[PokeSprite[1].Frame];
		
		if (LPokemon[1].Health > 0)
		{
			if (Index == -1)
			{
				double DF1 = Math.Floor(2D * (double)LPokemon[1].Level / 5D + 2D);
				double DF2 = Math.Floor(DF1 * (double)LPokemon[1].Stats[1] * 50D / (double)LPokemon[0].Stats[2]);
				double DF3 = Math.Floor(DF2 / 50D) + 2;
								
				byte Damage = (byte)DF3;
				byte Recoil = (byte)(Damage / 2);
				
				int PlayerHealth = LPokemon[1].Health - Damage;
				int EnemyHealth = LPokemon[0].Health - Recoil;
				
				PlayerHealth = PlayerHealth < 0 ? 0 : PlayerHealth;
				EnemyHealth = EnemyHealth < 0 ? 0 : EnemyHealth;
				
				var Animation = AnimEngine_Main.GetAnimation("PSTRUGGLE");
				
				Animation.TrackInsertKey(0, 1, string.Format("{0} is out of usable moves.", Name));
				Animation.TrackInsertKey(0, 1.69F, string.Format("{0} is out of usable moves.", Name));
				
				Animation.TrackInsertKey(0, 2.5F, string.Format("{0} used STRUGGLE!", Name));
				Animation.TrackInsertKey(0, 4.99F, string.Format("{0} used STRUGGLE!", Name));
				
				Animation.TrackInsertKey(0, 8, string.Format("{0} is hurt by recoil.", Name));

				Animation.TrackInsertKey(2, 6F, ((float)PlayerHealth / (float)LPokemon[1].Stats[0]) * 100F);
				Animation.TrackInsertKey(2, 5F, ((float)LPokemon[1].Health / (float)LPokemon[1].Stats[0]) * 100F);
				Animation.TrackInsertKey(2, 6F, string.Format("[right]{0}/{1}[/right]", PlayerHealth, LPokemon[1].Stats[0]));
				
				Animation.TrackInsertKey(3, 4.5F, ((float)EnemyHealth / (float)LPokemon[0].Stats[0]) * 100F);
				Animation.TrackInsertKey(3, 3.5F, ((float)LPokemon[0].Health / (float)LPokemon[0].Stats[0]) * 100F);
				
				LPokemon[0].Health = (byte)EnemyHealth;
				LPokemon[1].Health = (byte)PlayerHealth;
				
				AnimEngine_Main.Play("PSTRUGGLE");
			}
			
			else
			{
				Random RNGEngine = new Random();
				
				Move HMove = LPokemon[1].Moveset[Index];
				int HitRange = RNGEngine.Next(0, 101);
				
				string MoveName = Pokemon.MoveNames[HMove.ID];
				
				double DF1 = Math.Floor(2D * (double)LPokemon[1].Level / 5D + 2D);
				double DF2 = Math.Floor(DF1 * (double)LPokemon[1].Stats[1] * 50D / (double)LPokemon[0].Stats[2]);
				double DF3 = Math.Floor(DF2 / 50D) + 2;
				
				byte Damage = (byte)DF3;
				HMove.CurrentCount -= 1;
				
				int EnemyHealth = LPokemon[0].Health - Damage;
				EnemyHealth = EnemyHealth < 0 ? 0 : EnemyHealth;
				
				if (HitRange <= HMove.Accuracy)
				{
					var Animation = AnimEngine_Main.GetAnimation("PATTACK");
					
					Animation.TrackInsertKey(0, 1, string.Format("{0} used {1}!", Name, MoveName));

					Animation.TrackInsertKey(2, 3F, ((float)EnemyHealth / (float)LPokemon[0].Stats[0]) * 100F);
					Animation.TrackInsertKey(2, 2, ((float)LPokemon[0].Health / (float)LPokemon[0].Stats[0]) * 100F);
					
					LPokemon[0].Health = (byte)EnemyHealth;
					
					AnimEngine_Main.Play("PATTACK");
				}
				
				else
				{
					var Animation = AnimEngine_Main.GetAnimation("MISS");
					
					Animation.TrackInsertKey(0, 1, string.Format("{0} used {1}!", Name, MoveName));
					Animation.TrackInsertKey(0, 1.5F, string.Format("{0} used {1}!", Name, MoveName));
					Animation.TrackInsertKey(0, 2.5F, string.Format("{0}'s attack missed!", Name, MoveName));
					
					AnimEngine_Main.Play("MISS");
				}
			}
			
			if (BattleOrder == 4)
				BattleOrder--;
			
			else
			{
				BattleOrder = 0;
				AnimEngine_Main.Queue("INITMENU");
			}
		}
		
		else
		{
			BattleOrder = 0;
			AnimEngine_Main.Queue("INITMENU");
		}
		
		Battling = false;
	}
	
	/* Engine Events */
	
	public override void _Ready()
	{
		Preload();
		
		InitEnemy();
		InitPlayer();
		
		foreach (var Str in AnimQueue)
		AnimEngine_Main.Queue(Str);
	}
	
	public override void _Process(float delta)
	{
		ExecuteMenus();
		InfoLoop();
		
		if (PokeSwitch[1] && AnimEngine_Main.CurrentAnimation == "PSEND")
			InitPlayer();
			
		if ((BattleOrder == 3 || BattleOrder == 2) && !Battling && !AnimEngine_Main.IsPlaying())
			ProcessEnemy();
			
		else if ((BattleOrder == 4 || BattleOrder == 1)  && !Battling && !AnimEngine_Main.IsPlaying())
			ProcessPlayer(AttackIndex);
		
		else if (BattleOrder == 0 && !AnimEngine_Main.IsPlaying())
			AcceptInput = true;
	}
}
