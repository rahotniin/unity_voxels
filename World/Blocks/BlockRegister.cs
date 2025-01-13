using Leguar.TotalJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class BlockRegister {
	private static readonly Block[] blocks;
	private static readonly Dictionary<string, int> nameIndexes;
	public static readonly Dictionary<string, int> blockTextureIndexes;

	public static readonly Material material;
	
	static BlockRegister() {
		List<Block> blockList = new();
		nameIndexes = new Dictionary<string, int>();
		blockTextureIndexes = new Dictionary<string, int>();
		
		material = Resources.Load<Material>("Materials/ChunkMaterial");
		
		// Load void block
		blockList.Add(new Block("void"));
		nameIndexes.Add("void", 0);
		//Debug.Log("Loaded block: void");

		// Iterate over every folder in the /Blocks directory
		string blocksFolder = Application.persistentDataPath + "/Blocks";
		string[] dirs = Directory.GetDirectories(blocksFolder);
		List<Texture2D> textureList = new List<Texture2D>();
		for (int i = 0; i < dirs.Length; i++) {
			string blockDir = dirs[i];
			string blockName = blockDir.Substring(blocksFolder.Length + 1);

			string jsonString = File.ReadAllText(blockDir + "/" + blockName + ".json");
			JSON json = JSON.ParseString(jsonString);
			Block block = json.Deserialize<Block>();

			blockTextureIndexes.Add(block.Name, textureList.Count);
			string[] texturePaths = Directory.GetFiles(blockDir + "/textures");
			for (int t = 0; t < texturePaths.Length; t++) {
				Texture2D texture2 = new(0, 0);
				byte[] textureData2 = File.ReadAllBytes(texturePaths[t]);
				texture2.LoadImage(textureData2);
				Vector2Int res2 = new(texture2.width,  texture2.height);
				textureList.Add(texture2);
			}

			int blockIndex = i + 1;  // +1 because Void has already been loaded
			nameIndexes.Add(block.Name, (ushort)blockIndex);
			blockList.Add(block);
		}

		blocks = blockList.ToArray();
		blockList.Clear();

		Vector2Int res = new(textureList[0].width, textureList[0].height);
		Texture2DArray textureArray = new(res.x, res.y, textureList.Count, TextureFormat.ARGB32, false) {
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Repeat
		};

		for (int i = 0; i < textureList.Count; i++) {
			textureArray.SetPixels(textureList[i].GetPixels(), i);
		}

		textureArray.Apply();
		material.SetTexture("_TextureArray", textureArray);

		
	}

	public static Block GetBlock(int blockID) {
		return blocks[blockID];
	}

	public static int GetBlockIndex(string name) {
		return nameIndexes[name];
	}

	public static Block GetBlock(string name) {
		return GetBlock(GetBlockIndex(name));
	}
}
