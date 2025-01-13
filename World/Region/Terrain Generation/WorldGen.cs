using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public static class WorldGen {
	public static void GenerateTerrain(Chunk chunk) {
		//double start = Time.realtimeSinceStartupAsDouble;

		

		for (int x = 0; x < Chunk.Size; x++) {
			for (int z = 0; z < Chunk.Size; z++) {
				for (int y = 0; y < Chunk.Size; y++) {
					string blockType = "void";

					float global_y = chunk.Pos.y * Chunk.Size + y;

					if (global_y < 7) {
						blockType = "template";
					}

					Vector3Int pos = new(x, y, z);
					int blockID = BlockRegister.GetBlockIndex(blockType);

					
					chunk.SetBlock(pos, blockID, false);
					chunk.Prune();
				}
			}
		}


		//double end = Time.realtimeSinceStartupAsDouble;
		//double dT = end - start;
		//Debug.Log("Chunk " + chunk.pos + ": TERRAIN generation took " + dT * 1000f + "ms");
	}
}
