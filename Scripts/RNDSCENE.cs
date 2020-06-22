using Godot;

using System;
using System.Collections.Generic;

public class RNDSCENE : Node2D
{
	AnimationPlayer AnimPlayer;
	AudioStreamPlayer MusicPlayer;
	
	bool Initialized = false;
	
	// RNG-Manuplation Factors //
	
	byte MaxPokemon = 2;
	byte MinPokemon = 2;
	
	byte MaxLevel = 5;
	byte MinLevel = 1;
	
	byte MaxSpecies = 9;
	
	bool GiveItems = false;
	
	// ---------------------- //
	
	// Game Info Section //
	
	Trainer Enemy = new Trainer();
	Trainer Player = new Trainer();
	
	// ----------------- //
	
	string[] AnimQueue = new string[] {"INIT", "TRNS"};
	
	public bool GenerateBattle()
	{
		Random RNGEngine = new Random();
		
		List<Pokemon>[] Lists = new List<Pokemon>[] { new List<Pokemon>(), new List<Pokemon>() };
		int[] Counts = new int[] { RNGEngine.Next(MinPokemon, MaxPokemon + 1), RNGEngine.Next(MinPokemon, MaxPokemon + 1) };
		
		for (int z = 0; z < 2; z++)
		for (int i = 0; i < Counts[z]; i++)
		{
			byte PokeLevel = (byte)RNGEngine.Next(MinLevel, MaxLevel + 1);
			byte PokeSpecies = (byte)RNGEngine.Next(0, MaxSpecies);
			
			Pokemon TempPoke = new Pokemon(PokeLevel, PokeSpecies);
			Lists[z].Add(TempPoke);
		}
		
		Enemy.Party = Lists[0];
		Player.Party = Lists[1];
		
		return true;
	}
	
	public override void _Ready()
	{
		OS.SetWindowTitle("PokéFight v0.5 - [FerocityVine]");
		
		AnimPlayer = GetNode("ENGINES/ANIM") as AnimationPlayer;
		MusicPlayer = GetNode("ENGINES/MUSIC") as AudioStreamPlayer;
		
		AnimPlayer.Play(AnimQueue[0]);
	}
	
	public override void _Process(float delta)
	{
		if (!AnimPlayer.IsPlaying())
		{
			if (Initialized)
				GetTree().ChangeScene("res://Scenes/BTLSCENE.tscn");
				
			else if (GenerateBattle())
			{
				MusicPlayer.Play(0);
				AnimPlayer.Play(AnimQueue[1]);
				Initialized = true;
				
				GlobalVars.Enemy = Enemy;
				GlobalVars.Player = Player;
			}
		}
	}
}
