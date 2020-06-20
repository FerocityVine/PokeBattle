using Godot;
using System;

using System.Collections.Generic;

public class Move
{
	// ORDER: 
	// Bug, Dragon, Electric, Fighting, Fire, 
	// Flying, Ghost, Grass, Ground, Ice, Normal, 
	// Poison, Psychic, Rock, Water
	
	public static List<byte[]> Weaknesses = new List<byte[]>
	{
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] { 0x03 },
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
	};
	
	public static List<byte[]> Immunities = new List<byte[]>
	{
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] { 0x0A },
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] { 0x06 },
		new byte[] {},
		new byte[] {},
		new byte[] {},
		new byte[] {},
	};
	
	public byte ID;
	public byte Type;
	public byte Count;
	public byte Power;
	public byte Accuracy;
	public byte Category;
	
	public byte CurrentCount;
	
	public Move(byte I, byte T, byte C, byte P, byte A, byte CAT)
	{
		ID = I;
		Type = T;
		Count = C;
		Power = P;
		Accuracy = A;
		Category = CAT;
		
		CurrentCount = Count;
	}
}
