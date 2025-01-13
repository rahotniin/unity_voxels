using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using System;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour {
	// MUST be a power of 2
	public const int Size = 16;

	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;

	public Region Region;
	public Vector3Int Pos;

	public bool editable = true;

	/////////////////////////////////////////////////////////////////////////////////////////////////

	private Blocktree oldblocks;
	private Octree blockDict;

	public void SetBlock(Vector3Int blockPos, int blockID, bool pruningEnabled = true) {
		oldblocks.SetBlock(blockPos, blockID, pruningEnabled);
		//Debug.Log(blockDict.GetBlockID(blockPos));
	}

	public void Prune() {
		oldblocks.Prune();
	}

	public List<Blocktree.Leaf> GetBlocks(int LOD) {
		return oldblocks.GetLeaves(LOD);
	}
	public bool IsEmpty { get { return oldblocks.IsEmpty; } }


	/////////////////////////////////////////////////////////////////////////////////////////////////////

	public static Chunk New(Region region, Vector3Int pos) {
		string name = pos.ToString();
		GameObject gameObject = new GameObject(name);
		
		gameObject.transform.parent = region.transform;
		gameObject.transform.rotation = region.transform.rotation;
		gameObject.transform.localPosition = pos * Size;

		Chunk chunk = gameObject.AddComponent<Chunk>();
		chunk.Initialise(region, pos);

		chunk.meshFilter = chunk.GetComponent<MeshFilter>();
		chunk.meshRenderer = chunk.GetComponent<MeshRenderer>();
		chunk.meshCollider = chunk.GetComponent<MeshCollider>();

		return chunk;
	}

	public void Initialise(Region region, Vector3Int pos) {
		Region = region;
		Pos = pos;
		
		oldblocks = new Blocktree();
		blockDict = new Octree();

		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();
	}

	public static bool ContainsPosition(Vector3Int pos) {
		return (pos.x > -1 && pos.y > -1 && pos.z > -1 && pos.x < Chunk.Size && pos.y < Chunk.Size && pos.z < Chunk.Size);
	}
}
