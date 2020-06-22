using Godot;

using System;
using System.Collections.Generic;

public class Pokemon
{
	Move TACKLE = new Move(0, 0x0A, 40, 40, 100, 0);
	Move POUND = new Move(1, 0x0A, 40, 40, 100, 0);
	
	public List<Move> Moveset;
	public List<byte> IVArray;
	public List<byte> Stats;
	public byte Level;
	public byte Species;
	public byte HP;
	public byte Type;
	
	public static string[] TypeNames = new string[]
	{
		"BUG",
		"DRAGON",
		"ELECTRIC",
		"FIGHTING",
		"FIRE",
		"FLYING",
		"GHOST",
		"GRASS",
		"GROUND",
		"ICE",
		"NORMAL",
		"POISON",
		"PSYCHIC",
		"ROCK",
		"WATER"
	};
	
	
	public static string[] MoveNames = new string[]
	{
		"TACKLE",
		"POUND"
	};
	
	public static string[] PokeNames = new string[] 
	{
		"BULBASAUR",
		"IVYSAUR",
		"VENUSAUR",
		"CHARMANDER",
		"CHARMELEON",
		"CHARIZARD",
		"SQUIRTLE",
		"WARTORTLE",
		"BLASTOISE"
	};
	
	public static List<byte[]> PokeBase = new List<byte[]>() 
	{
		new byte[] {45, 49, 49, 45, 65},
		new byte[] {60, 62, 63, 60, 80},
		new byte[] {80, 82, 83, 80, 100},
		new byte[] {39, 52, 43, 65, 50},
		new byte[] {58, 64, 58, 80, 65},
		new byte[] {78, 8, 78, 100, 85},
		new byte[] {44, 48, 65, 43, 50},
		new byte[] {59, 63, 80, 58, 65},
		new byte[] {79, 83, 100, 78, 85}
	};
	
	void CalcStats()
	{
		Random RNGEngine = new Random();
		
		IVArray = new List<byte>() { 0 };
		Stats = new List<byte>() { 0 };
		
		byte[] BaseStats = PokeBase[Species];
		
		for (int i = 1; i < 5; i++)
		{
			byte RandIV = (byte)RNGEngine.Next(0, 16);
			IVArray.Add(RandIV);
			
			byte StatCalc = (byte)Math.Floor(Math.Floor((double)((2 * BaseStats[i] + RandIV) * Level / 100 + 5)));
			Stats.Add(StatCalc);
		}
	}
	
	void CalcHP()
	{
		byte[] BaseStats = PokeBase[Species];
		
		IVArray[0] = (byte)(((IVArray[1] % 2 != 0) ? 8 : 0) + ((IVArray[2] % 2 != 0) ? 4 : 0) + ((IVArray[3] % 2 != 0) ? 2 : 0) + ((IVArray[4] % 2 != 0) ? 1 : 0));
		
		byte StatCalc = (byte)Math.Floor((double)((2 * BaseStats[0] + IVArray[0]) * Level / 100 + Level + 10));
		Stats[0] = StatCalc;
		
		HP = StatCalc;
	}
	
	public Pokemon(byte L, byte S)
	{
		Level = L;
		Species = S;
		
		CalcStats();
		CalcHP();
		
		Moveset = new List<Move> { TACKLE, POUND };
	}
}
