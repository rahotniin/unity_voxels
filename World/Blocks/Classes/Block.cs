using Leguar.TotalJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {
	public readonly string Name;
	public readonly int[] faceTextureIndexes;
	public Block() { }
	public Block(string name) {
		this.Name = name;
	}
	public Block(string name, int[] textureIndexes) : this(name) {
		this.faceTextureIndexes = textureIndexes;
	}

    public static bool TestDifference(Block a, Block b) {
        return a.Name != b.Name;
    }

	public static bool TestEquivalence(Block a, Block b) {
		return (a.Name == b.Name);
	}

	public static bool IsFaceFull(Block block, int face) {
		return (block.Name != "void");
	}
}
