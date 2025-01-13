using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Leguar.TotalJSON;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine.LowLevel;
using System.Net.Sockets;
//using System.Numerics;

// TO-DO:
// Chunk generation as the player move around, then:
// - LODs
// Smooth terrain, then:
// - Non-grid block placement
// Have chunk generation distributed accross all available cores, via DOTS
// Have chunk meshing done in a compute shader
// Player physics

public class World : MonoBehaviour {
	
	Region terrain;

	Player player;
	public GameObject playerObject;
	public Vector3Int renderVolume = new(1, 1, 1);

	bool paused = false;

	void Start() {
		Octree test = new();
		
		for (int i = 0; i < 10000; i++) {
			Vector3Int pos = new Vector3Int(Random.Range(0, 8), Random.Range(0, 8), Random.Range(0, 8));
			
			int blockID = Random.Range(0, 512);
			test.SetBlock(pos, blockID);
			int t = test.GetBlock(pos);
			if (t != blockID) {
				Debug.Log("Error at " + pos + "\n Expected " + i + "\n Got " + t);
			}
		}


		player = new Player(playerObject);

		GenerateTerrain();
		void GenerateTerrain() { // Temp
			terrain = Region.New(transform, Region.Type.Terrain, "Terrain", Vector3.zero, renderVolume);
			terrain.LoadChunks(Vector3.zero, renderVolume, true);
		}
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			paused = true;
		}

		if (!paused) {
			HandlePlayerMovement();
			HandlePlayerBlockInteraction();
		}

		if (paused) {
			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				paused = false;
			}
		}
	}

	Vector3 prev_playerPos;
	void HandlePlayerMovement() {
		player.TryMove();

		/*Vector3 playerPos = player.gameObject.transform.position;
		if (prev_playerPos != playerPos) {
			prev_playerPos = playerPos;
			StartCoroutine(terrain.LoadChunks(playerPos, renderVolume, true));
		}*/
	}

	void HandlePlayerBlockInteraction() {
		Chunk chunkToRemesh;
		if (player.HandleBlockInteraction(out chunkToRemesh)) {
			Meshf.MeshChunk(chunkToRemesh);
		}
	}

	private void OnDrawGizmos() {
		if (player != null) {
			if (player.isTargetingBlock) {
				Gizmos.color = new(1f, 0f, 1f, 1f);
				//Gizmos.DrawCube(player.globalTargetBlockPos, Vector3.one * 0.5f);
			}
		}
	}
}
