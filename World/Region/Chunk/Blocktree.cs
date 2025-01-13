using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

// TO-DO:
// Implement a block palette
// Make the Node struct generic, to use byte, ushort, uint, depending on the size of the block palette.
// Make a way to save and load the blocktree to from a file

public class Blocktree {
	private Dictionary<int, Node> nodes;
	private int counter; // used to avoid key collisions when adding to the nodes dictionary
	private List<int> pruningHistory;

	public Blocktree() {
		//nodes = new List<Node>() { new Node(0, 0) };
		nodes = new() { { 0, new Node(0, 0) } };
		counter = 1;
		pruningHistory = new();
	}

	public bool IsEmpty { get {  return nodes.Count == 1 && nodes[0].blockID == 0; } }

	public struct Node {
		public int blockID;
		public int branchIndex; // when this == 0, the node is a leaf

		public Node(int _blockID, int _branchIndex = 0) {
			blockID = _blockID;
			branchIndex = _branchIndex;
		}
	}

	public struct Leaf {
		public Vector3Int position;
		public int size;
		public int blockID;
		public byte exposedFaces; // uses only 6 out of 8 bits
		public Leaf(Vector3Int position, int size, int blockID, byte exposedFaces) {
			this.position = position;
			this.size = size;
			this.blockID = blockID;
			this.exposedFaces = exposedFaces;
		}

		public void Log() {
			Debug.Log("Pos: " + position + " Size: " + size + " blockID: " + blockID);
		}
	}

	public void Log() {
		Debug.Log("There are " + nodes.Count + " nodes");
		for (int i = 0; i < nodes.Count; i++) {
			Log(i);
		}
	}

	public void Log(int nodeIndex) {
		Debug.Log("Node: " + nodeIndex + " branchIndex: " + nodes[nodeIndex].branchIndex + " blockID: " + nodes[nodeIndex].blockID);
	}

	public void SetBlock(Vector3Int blockPos, int blockID, bool pruningEnabled = true) {
		bool needsPruning = false;
		InternalSetBlock(blockPos, blockID, pruningEnabled, ref needsPruning);
		if (pruningEnabled) {
			if (needsPruning) {
				Prune();
			}
		}
	}

	private void InternalSetBlock(Vector3Int blockPos, int blockID, bool pruningEnabled, ref bool needsPruning, int nodeIndex = 0, int nodeSize = Chunk.Size) {
		int branchIndex = nodes[nodeIndex].branchIndex;
		
		if (branchIndex == 0) {
			branchIndex = Branch(nodeIndex);
		}

		int halfSize = nodeSize >> 1; // divide by 2
		int local_subnodeIndex = GetSubnodeContaining(blockPos, halfSize);
		int global_subnodeIndex = branchIndex + local_subnodeIndex;
		
		if (nodeSize > 2) {
			Vector3Int descent = Cube.Corners[local_subnodeIndex] * halfSize;
			InternalSetBlock(blockPos - descent, blockID, pruningEnabled, ref needsPruning, global_subnodeIndex, halfSize);
		} else {
			nodes[global_subnodeIndex] = new Node(blockID);
			//Debug.Log("Editing node " + global_subnodeIndex);
			needsPruning = CanPruneBranch(branchIndex);
		}
	}

	private int Branch(int nodeIndex) {
		int branchIndex = counter;
		if (pruningHistory.Count > 0) {
			branchIndex = pruningHistory[pruningHistory.Count - 1];
			pruningHistory.RemoveAt(pruningHistory.Count - 1);
		} else {
			counter += 8;
		}
		// need to implement a way to re-use pruned branchIndexes, so the counter will never overflow the integer limit.
		int blockID = nodes[nodeIndex].blockID;
		nodes[nodeIndex] = new Node(0, branchIndex);
		for (int i = 0; i < 8; i++) {
			nodes.Add(branchIndex + i, new Node(blockID, 0));
		}
		return branchIndex;
	}

	private bool CanPruneBranch(int branchIndex) {
		int standard = nodes[branchIndex].blockID;
		for (int i = 1; i < 8; i++) {
			int sample = nodes[branchIndex + i].blockID;
			if (sample != standard) {
				return false;
			}
		}
		return true;
	}

	public bool Prune(int nodeIndex = 0) {
		int branchIndex = nodes[nodeIndex].branchIndex;
		if (branchIndex == 0) {
			return true;
		}

		bool allSubnodesPruned = true;
		for (int i = 0; i < 8; i++) {
			int subnodeIndex = branchIndex + i;
			allSubnodesPruned &= Prune(subnodeIndex);
		}

		if (allSubnodesPruned) {
			if (CanPruneBranch(branchIndex)) {
				//Debug.Log("pruning node " + nodeIndex);

				int blockID = nodes[branchIndex].blockID;
				nodes[nodeIndex] = new Node(blockID);
				
				for (int i = 0; i < 8; i++) {
					nodes.Remove(branchIndex + i);
				}

				pruningHistory.Add(branchIndex);
				
				return true;
			}
		}
		return false;
	}

	public int GetLeaf(Vector3Int blockPos, int LOD = 1,int nodeIndex = 0, int nodeSize = Chunk.Size) {
		if (nodeSize > LOD) {
			int branchIndex = nodes[nodeIndex].branchIndex;
			if (branchIndex == 0) {
				return nodes[nodeIndex].blockID;
			}
			
			int halfSize = nodeSize >> 1;
			int subnodeIndex = GetSubnodeContaining(blockPos, halfSize);
			Vector3Int descent = Cube.Corners[subnodeIndex] * halfSize;

			return GetLeaf(blockPos - descent, LOD, branchIndex + subnodeIndex, halfSize);
		}
		return nodes[nodeIndex].blockID;
	}

	public List<Leaf> GetLeavesAndLog(int LOD = 1) {
		List<Leaf> leaves = GetLeaves(LOD);
		Debug.Log("There are " + leaves.Count + " leaves");
		foreach (Leaf leaf in leaves) {
			leaf.Log();
		}
		
		return leaves;
	}

	public List<Leaf> GetLeaves(int LOD = 1) {
		List<Leaf> leaves = new List<Leaf>();
		InternalGetLeaves(LOD, ref leaves, Vector3Int.zero); // Vector3Int.zero is only included because it isnt a compile-time constant and, so, cannot be default value
		return leaves;
	}

	private void InternalGetLeaves(int LOD, ref List<Leaf> leaves, Vector3Int nodePos, int nodeIndex = 0, int nodeSize = Chunk.Size) {
		if (nodeSize > LOD) {
			int branchIndex = nodes[nodeIndex].branchIndex;
			if (branchIndex != 0) {
				for (int i = 0; i < 8; i++) {
					int halfSize = nodeSize / 2;
					Vector3Int descent = Cube.Corners[i] * halfSize;
					InternalGetLeaves(LOD, ref leaves, nodePos + descent, branchIndex + i, halfSize);
				}
				return;
			}
		}
		
		int blockID = nodes[nodeIndex].blockID;
		if (blockID != 0) {
			leaves.Add(new Leaf(nodePos, nodeSize, blockID, GetExposedFaces(nodePos, nodeSize, LOD)));
		}
	}

	int GetSubnodeContaining(Vector3Int pos, int halfSize) { // returns 0~7, the subnode that the position is in            
		int subnodeIndex = 0;

		subnodeIndex += pos.x >= halfSize ? 1 : 0;
		subnodeIndex += pos.y >= halfSize ? 2 : 0;
		subnodeIndex += pos.z >= halfSize ? 4 : 0;

		return subnodeIndex;
	}

	byte GetExposedFaces(Vector3Int blockPos, int blockSize, int LOD) {
		byte exposedFaces = 0;

		if (blockSize == LOD) {
			for (int f = 0; f < 6; f++) {
				Vector3Int adjacentPos = blockPos + Cube.DirectionFromFace[f] * LOD;
				
				if (Chunk.ContainsPosition(adjacentPos)) {
					int blockIndex = GetLeaf(adjacentPos, LOD);
					Block adjacentBlock = BlockRegister.GetBlock(blockIndex);

					if (!Block.IsFaceFull(adjacentBlock, Cube.AdjacentFaces[f])) {
						exposedFaces += (byte)(1 << f);
					}
				} else {
					// This face is on the chunk border, so it must be meshed
					exposedFaces += (byte)(1 << f);
					// TO-DO: have this handled by the region, so adjacent blocks in neighboring chunks can be retrieved
				}
			}
		} else {
			for (int F = 0; F < 6; F++) {
				if (AreAnySubfacesExposed()) {
					exposedFaces += (byte)(1 << F);
				}
				
				bool AreAnySubfacesExposed() {
					Vector3Int[] axes = Cube.AxesFromFace[F];
					Vector3Int facePos = Cube.CalculateFacePositions(LOD, F, blockSize);

					for (int a = 0; a < blockSize / LOD; a++) {
						for (int b = 0; b < blockSize / LOD; b++) {
							Vector3Int local_subfacePos = (a * axes[0] + b * axes[1]) * LOD;

							Vector3Int global_subfacePos = local_subfacePos + facePos + blockPos;

							Vector3Int adjacentPos = global_subfacePos + Cube.DirectionFromFace[F] * LOD;
							if (Chunk.ContainsPosition(adjacentPos)) {
								int blockIndex = GetLeaf(adjacentPos, LOD);
								Block adjacentBlock = BlockRegister.GetBlock(blockIndex);
								
								if (!Block.IsFaceFull(adjacentBlock, Cube.AdjacentFaces[F])) {
									return true;
								}
							} else {
								// face is on the chunk border
								return true;
							}
						}
					}
					return false;
				}
			}
		}
		return exposedFaces;
	}
}
