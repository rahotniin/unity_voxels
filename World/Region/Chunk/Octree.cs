using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each chunk is 8*8*8 blocks
// Each chunk contains an Octree which stores both voxel info and 4 levels of LODs (contained in the upper nodes of the tree)

// Eventually TO-DO
// groupd groups into 8*8*8 'subregions' (and think of a different nae for them)
// subregions contain another chunk, whose blocks are determined by the root node (biggest LOD) of its constituent chunks

// NEXT TO-DO:
// Get blocks in a format for meshing
// Implement pruning

// meshing can be done on the subregion level (to minimise draw calls?)

// When creating a chunk during terrain generation:
// 1. create a chunk with a maximal octree
// 2. place all the blocks (use a constant table detailing where blocks go in a maximal octant array)
// 3. prune the octree

// When the player edits a chunk:
// When the octants array reaches a load factor of 0.75, double its capacity

public class Octree {
	//public const int Size = 8; // this value should not be used. The size of a chunk is hard-coded as 8
	
	byte root; // TO-DO: move these two into the sub-region
	bool isHomogenous = false;
	
	Octant[] octants; // set to null when the tree is homogenous

	byte capacity; // the size of the octants array. Has a maximum of 73
	// Maybe increase the capacity by a constatn amount every time the array gets full? maybe by 8?
	byte load; // the number of octants stored
	//const float max_load_factor = 0.75f;
	
	byte last; // the index of the last octant in the array + 1, used to add new octants if there are no free indices
	List<byte> freeIndices; // indexes of pruned octants, so new octants can be put in their place

	public Octree() {
		root = 0;
		
		capacity = 73;
		octants = new Octant[capacity];
		
		octants[0] = new Octant();
		load = 1;
		last = 1;
		
		freeIndices = new List<byte>();
    }

	static Octree() {
		GenerateTemplate();
	}

	static Octree Template;
	static void GenerateTemplate() {
		Template = new Octree();
		for (int i = 0; i < 8; i++) {
			Template.Branch(0, i);
		}
		for (int i = 1; i < 9; i++) {
			for (int j = 0; j < 8; j++) {
				Template.Branch(i, j);
			}
		}

		//Debug.Log(Template.load);
		//Template.Log();
	}

	/// <summary>
	/// pos is in region coordinates
	/// </summary>
	public int GetBlock(Vector3Int pos) {
		if (isHomogenous) {
			return root;
		}
		
		// if the root is not a leaf, descend the tree
		int octant_index = 0; // starting with the first octant (the one belonging to the leaf)
		for (int lod = 2, size = 8; lod > -1; lod--, size /= 2) {
			// get the vector of the node containg pos
			Vector3Int node_vec = new Vector3Int((pos.x % size) >> lod, (pos.x % size) >> lod, (pos.x % size) >> lod);
			
			// get the nodes index from the vector
			int node_index = node_vec.x + node_vec.y * 2 + node_vec.z * 4;
			
			// get the suboctant index of that node
			int suboctant_index = octants[octant_index].nodes[node_index].suboctant;
			
			// check if the node is a leaf
			if (suboctant_index == 0) {
				return octants[octant_index].nodes[node_index].blockID;
			}
			// else descend to the next octant
			octant_index = suboctant_index;
		}
		
		return 0;
	}

	public void SetBlock(Vector3Int pos, int blockID) {
		if (isHomogenous) {
			// create the octants array
			// initlialise with the 0th octant
			// and enough space
		}

		int octant_index = 0; // starting with the first octant (the one belonging to the leaf)
		for (int lod = 2, size = 8; lod > -1; lod--, size /= 2) {
			// get the vector of the node containg pos
			Vector3Int node_vec = new Vector3Int((pos.x % size) >> lod, (pos.x % size) >> lod, (pos.x % size) >> lod);
			
			// get the nodes index from the vector
			int node_index = node_vec.x + node_vec.y * 2 + node_vec.z * 4;
			
			// get the suboctant index of that node
			int suboctant_index = octants[octant_index].nodes[node_index].suboctant;
			
			// check if the node is a leaf
			if (suboctant_index == 0) {
				if (size > 2) {
					suboctant_index = Branch(octant_index, node_index);
				} else {
					octants[octant_index].nodes[node_index].blockID = blockID;
					return;
				}
			}
			// else descend to the next octant
			octant_index = suboctant_index;
		}
	}

	public void Log() {
		for (int i = 0; i < last; i++) {
			if (octants[i] != null) {
				Octant octant = octants[i];
				string log = "Octant: " + i;
				for(int j = 0; j < 8; j++) {
					log += "\n Node " + j + ": BlockID = " + octant.nodes[j].blockID + ", Suboctant = " + octant.nodes[j].suboctant;
				}
				Debug.Log(log);
			}
		}
	}

	public int Branch(int octant_index, int node_index) {
		int blockID = octants[octant_index].nodes[node_index].blockID;
		int suboctant_index = Add(new Octant(blockID));
		octants[octant_index].nodes[node_index].suboctant = suboctant_index;
		
		return suboctant_index;
	}

	private int Add(Octant octant) {
		int index;
		
		if (freeIndices.Count > 0) {
			int f = freeIndices.Count - 1;
			index = freeIndices[f];
			freeIndices.RemoveAt(f);
		} else {
			index = last;
			last++;
		}

		octants[index] = octant;
		load++;
		
		return index;
	}

	private void Remove(int octant) {
		octants[octant] = null;
		load--;
		freeIndices.Add((byte)octant);
	}
	
	class Octant {
		public Node[] nodes;

		public Octant() {
			nodes = new Node[8] {
				new Node(),
				new Node(),
				new Node(),
				new Node(),
				new Node(),
				new Node(),
				new Node(),
				new Node(),
			};
		}
		
		public Octant(int blockID) {
			nodes = new Node[8] {
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
				new Node(blockID),
			};
		}
	}

	class Node {
		// 9 bits for blockID
		public const ushort blockID_mask   = 0b_111111111_0000000;
		public const int blockID_offset = 7;
		// 7 bits for suboctant
		public const ushort suboctant_mask = 0b_000000000_1111111;
		
		public ushort data;

		public Node() {
			data = 0;
		}

		public Node(int blockID) {
			//data = (ushort)(blockID << blockID_offset);
			this.blockID = blockID;
		}

		public int blockID {
			get {
				return (data & blockID_mask) >> blockID_offset;
			}
			set {
				data = (ushort)((data & suboctant_mask) + value << 7);
			}
		}

		public int suboctant {
			get {
				return data & suboctant_mask;
			}
			set {
				data = (ushort)((data & blockID_mask) + value);
			}
		}
	}
}
