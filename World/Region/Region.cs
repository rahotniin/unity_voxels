using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Implement a scale, so different regions can have different block sizes

public class Region : MonoBehaviour {
	Type type;
	
	public Dictionary<Vector3Int, Chunk> chunks = new();

	//public List<Vector3Int> loadedChunks;
	//public List<Vector2Int> unloadedChunks;

	public enum Type {
		Terrain
	}

	public static Region New(Transform parent, Type type, string name, Vector3 pos, Vector3Int size) {
		GameObject gameObject = new GameObject(name);
		gameObject.transform.parent = parent;
		gameObject.transform.rotation = parent.rotation;
		gameObject.transform.position = pos;

		Region region = gameObject.AddComponent<Region>();
		region.name = name;
		region.type = type;
		region.chunks = new Dictionary<Vector3Int, Chunk>();

		return region;
	}

	public void GenerateChunk(Vector3Int pos) {
		GameObject chunkObject = new();
		chunkObject.name = pos.ToString();
		chunkObject.transform.parent = gameObject.transform;
		chunkObject.transform.rotation = gameObject.transform.rotation;
		chunkObject.transform.localPosition = pos * Chunk.Size;
		

		
		Chunk chunk = chunkObject.AddComponent<Chunk>();
		chunk.Initialise(this, pos);

		chunks.Add(pos, chunk);
	}

	public Vector3Int prev_local_loaderPos = Vector3Int.one * int.MaxValue;

	/*public IEnumerator LoadChunks(Vector3 global_loaderPos, Vector3Int renderVolume, bool generateChunks) {
		Vector3Int local_loaderPos = Vector3Int.FloorToInt(transform.InverseTransformPoint(global_loaderPos) / Chunk.Size);
		if (local_loaderPos != prev_local_loaderPos) {
			prev_local_loaderPos = local_loaderPos;

			int xMin = renderVolume.x >> 1;
			int xMax = renderVolume.x - xMin;

			int yMin = renderVolume.y >> 1;
			int yMax = renderVolume.y - yMin;

			int zMin = renderVolume.z >> 1;
			int zMax = renderVolume.z - zMin;



			for (int y = -yMin; y < yMax; y++) {
				for (int x = -xMin; x < xMax; x++) {
					for (int z = -zMin; z < zMax; z++) {
						Vector3Int chunkPos = local_loaderPos + new Vector3Int(x, y, z);

						if (!chunks.ContainsKey(chunkPos)) {
							Chunk chunk = Chunk.New(this, chunkPos);
							WorldGen.GenerateTerrain(chunk);
							Meshf.MeshChunk(chunk);
							chunks.Add(chunkPos, chunk);
						}
						yield return new WaitForSeconds(.1f);
					}
				}
			}
		}
	}*/

	public void LoadChunks(Vector3 global_loaderPos, Vector3Int renderVolume, bool generateChunks) {
		// Get the chunk loaders position in this regions local chunk coordinates
		Vector3Int local_loaderPos = Vector3Int.FloorToInt(transform.InverseTransformPoint(global_loaderPos) / Chunk.Size);
		if (local_loaderPos != prev_local_loaderPos) {
			prev_local_loaderPos = local_loaderPos;

			int xMin = renderVolume.x >> 1;
			int xMax = renderVolume.x - xMin;

			int yMin = renderVolume.y >> 1;
			int yMax = renderVolume.y - yMin;

			int zMin = renderVolume.z >> 1;
			int zMax = renderVolume.z - zMin;


			for (int y = -yMin; y < yMax; y++) {
				for (int x = -xMin; x < xMax; x++) {
					for (int z = -zMin; z < zMax; z++) {
						Vector3Int chunkPos = local_loaderPos + new Vector3Int(x, y, z);

						if (!chunks.ContainsKey(chunkPos)) {
							Chunk chunk = Chunk.New(this, chunkPos);
							WorldGen.GenerateTerrain(chunk);
							Meshf.MeshChunk(chunk);
							chunks.Add(chunkPos, chunk);
						}
					}
				}
			}
		}
	}

	public bool TrySetBlock(Vector3Int blockPos, int blockID, out Chunk chunkToRemesh) {
		Vector3Int chunkPos = Vector3Int.FloorToInt((Vector3)blockPos / Chunk.Size);
		Vector3Int localBlockPos = blockPos - chunkPos * Chunk.Size;

		Chunk chunk;
		if (!chunks.TryGetValue(chunkPos, out chunk)) {
			chunk = Chunk.New(this, chunkPos);
			chunks.Add(chunkPos, chunk);
		}
		
		chunk.SetBlock(localBlockPos, blockID);
		if (chunk.IsEmpty) {
			Destroy(chunk.gameObject);
			chunks.Remove(chunkPos);
		}
		
		chunkToRemesh = chunk;
		return true;
	}
}
