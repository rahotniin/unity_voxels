using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cube {

	//      Axes:
	//
	//      y
	//      |   z
	//      |  / 
	//      | /  
	//      |/   
	//      +-----------x
	//
	//      Corner Numbers:         Edge Numbers:           Face & Corner Numbers:
	//                                                      
	//          6-----------7           +-----6-----+           6-----------7           6-----------7           6           7
	//         /|          /|          /|          /|          /           /            |           |          /|   5 ->   /|
	//        / |         / |        11 |        10 |         /     1     /             |     3     |         / |         / |
	//       /  |        /  |        /  7        /  5        /           /              |           |        /  |        /  |
	//      2---|-------3   |       +---|-2-----+   |       2-----------3           2---|-------3   |       2   |       3   |
	//      |   4-------|---5       |   +-----4-|---+           4-----------5       |   4-------|---5       |   4       |   5
	//      |  /        |  /        3  /        1  /           /           /        |           |           |  /        |  / 
	//      | /         | /         | 8         | 9           /     0     /         |     2     |           | /         | /  
	//      |/          |/          |/          |/           /           /          |           |           |/   <-4    |/   
	//      0-----------1           +-----0-----+           0-----------1           0-----------1           0           1    

	//					6-----------7
	//					|         / |
	//					|    1 /    |
	//					|    /      |
	//					| /         |
	//      6-----------2-----------3-----------7-----------6
	//      |         / |         / |         / |         / |
	//      |   4  /    |    2 /    |    5 /    |    3 /    |
	//      |    /      |    /      |    /      |    /      |
	//      | /         | /         | /         | /         |
	//      4-----------0-----------1-----------5-----------4
	//					|         / |
	//					|    0 /    |
	//					|    /      |
	//					| /         |
	//					4-----------5
	
	public static void Draw(Vector3 pos, float size) {
		foreach (int[] edge in Cube.CornersFromEdge) {
			Debug.DrawLine(pos + (Vector3)Cube.Corners[edge[0]] * size, pos + (Vector3)Cube.Corners[edge[1]] * size, Color.white, 1000f);
		}
	}


	public static Vector3Int[] Corners = {
		new (0, 0, 0), // 0
		new (1, 0, 0), // 1
		new (0, 1, 0), // 2
		new (1, 1, 0), // 3
		new (0, 0, 1), // 4
		new (1, 0, 1), // 5
		new (0, 1, 1), // 6
		new (1, 1, 1), // 7
	};

	//		2-----------3
	//		|         / |
	//		|      /    |
	//		|    /      |
	//		| /         |
	//      0-----------1

	static List<int> faceTriangles = new() { 0, 2, 3, 0, 3, 1 };

	static readonly List<List<Vector3>> faceVertices = new() {
		new List<Vector3>() { // Face 0
			new(0, 0, 1), // 4
			new(1, 0, 1), // 5
			new(0, 0, 0), // 0
			new(1, 0, 0), // 1
		},
		new List<Vector3>() { // Face 1
			new(0, 1, 0), // 2
			new(1, 1, 0), // 3
			new(0, 1, 1), // 6
			new(1, 1, 1), // 7
		},
		new List<Vector3>() { // Face 2
			new(0, 0, 0), // 0
			new(1, 0, 0), // 1
			new(0, 1, 0), // 2
			new(1, 1, 0), // 3
		},
		new List<Vector3>() { // Face 3
			new(1, 0, 1), // 5
			new(0, 0, 1), // 4
			new(1, 1, 1), // 7
			new(0, 1, 1), // 6
		},
		new List<Vector3>() { // Face 4
			new(0, 0, 1), // 4
			new(0, 0, 0), // 0
			new(0, 1, 1), // 6
			new(0, 1, 0), // 2
		},
		new List<Vector3>() { // Face 5
			new(1, 0, 0), // 1
			new(1, 0, 1), // 5
			new(1, 1, 0), // 3
			new(1, 1, 1), // 7
		}
	};

	public struct Face {
		public List<Vector3> vertices;
		public List<int> triangles;

		public Face(List<Vector3> _vertices, List<int> _triangles) {
			this.vertices = _vertices;
			this.triangles = _triangles;
		}
	}

	public static Face GetFace(int face, Vector3Int position, int size) {
		List<Vector3> standards = faceVertices[face];
		List<Vector3> verts = new List<Vector3>();
		for (int i = 0; i < 4; i++) {
			verts.Add(standards[i] * size + position);
		}
		return new Face(verts, faceTriangles);
	}
	
	
	//don't know if there is a non-arbitrary way of indexing the edges and faces of a cube
	//don't know if this is used anymore
	public static int[][] CornersFromEdge = {
		new int[] {0, 1}, // 0
		new int[] {1, 3}, // 1
		new int[] {3, 2}, // 2
		new int[] {2, 0}, // 3
		new int[] {4, 5}, // 4
		new int[] {5, 7}, // 5
		new int[] {7, 6}, // 6
		new int[] {6, 4}, // 7
		new int[] {0, 4}, // 8
		new int[] {1, 5}, // 9
		new int[] {3, 7}, // 10
		new int[] {2, 6}, // 11
	};

	public static int[][] CornersFromFace = {
		new int[] {0, 1, 5, 4}, // 0
		new int[] {2, 6, 7, 3}, // 1
		new int[] {0, 2, 3, 1}, // 2
		new int[] {5, 7, 6, 4}, // 3
		new int[] {4, 6, 2, 0}, // 4
		new int[] {1, 3, 7, 5}, // 5
	};

	public static int[][] TrianglesFromFace = {
		new int[] {0, 1, 5, 0, 5, 4}, // 0
		new int[] {2, 6, 7, 2, 7, 3}, // 1
		new int[] {0, 2, 3, 0, 3, 1}, // 2
		new int[] {4, 5, 7, 4, 7, 6}, // 3
		new int[] {0, 4, 6, 0, 6, 2}, // 4
		new int[] {1, 3, 7, 1, 7, 5}, // 5
	};

	public static int[] FaceTriangles = {
		1, 2, 0, 2, 3, 0
	};

	public static Vector3Int[] DirectionFromFace = {
		Vector3Int.down,	// 0
		Vector3Int.up,      // 1
		Vector3Int.back,    // 2
		Vector3Int.forward, // 3
		Vector3Int.left,    // 4
		Vector3Int.right,   // 5
	};

	public static Color[] FaceColours = {
		Color.blue + Color.red,		// down
		Color.green,				// up
		Color.green + Color.red,	// back
		Color.blue,					// forward
		Color.blue + Color.green,	// left
		Color.red,					// right
	};

	public static Vector3Int CalculateFacePositions(int LOD, int face, int size) {
		Vector3Int[] offsets = {
			Vector3Int.zero,					// 0
			Vector3Int.up * (size - LOD),			// 1
			Vector3Int.zero,					// 2
			Vector3Int.forward * (size - LOD),	// 3
			Vector3Int.zero,					// 4
			Vector3Int.right * (size - LOD),		// 5
		};
		return offsets[face];
	}

	public static Vector3Int[][] AxesFromFace = {
		new Vector3Int[] {Vector3Int.forward, Vector3Int.right},	// 0
		new Vector3Int[] {Vector3Int.forward, Vector3Int.right},	// 1
		new Vector3Int[] {Vector3Int.up, Vector3Int.right},			// 2
		new Vector3Int[] {Vector3Int.up, Vector3Int.right},			// 3
		new Vector3Int[] {Vector3Int.up, Vector3Int.forward},		// 4
		new Vector3Int[] {Vector3Int.up, Vector3Int.forward},		// 5
	};

	public static int[] AdjacentFaces = {
		1, 0, 3, 2, 5, 4
	};

	public static string[] faceNames = new string[6] {
		"bottom", "top", "back", "forward", "left", "right"
	};
}
