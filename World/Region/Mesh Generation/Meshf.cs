using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

// TO-DO:
// Implement greedy meshing instead of LOD/blocktree based meshing
public static class Meshf {
	public static void MeshChunk(Chunk chunk, int LOD = 1) {
		//double start = Time.realtimeSinceStartupAsDouble;

		List<Vector3> vertices = new();
		List<Vector3> uvs = new();
		List<Vector3> normals = new();
		List<int> triangles = new();

		List<Blocktree.Leaf> leaves = chunk.GetBlocks(LOD);
		foreach (Blocktree.Leaf l in leaves) {
			Block block = BlockRegister.GetBlock(l.blockID);
			Vector3Int leafPos = l.position;
			int leafSize = l.size;
			
			for (int f = 0; f < 6; f++) {
				if ((l.exposedFaces & (1 << f)) == (1 << f)) {
					AddFace(leafPos, f, leafSize);
				}
			}

			void AddFace(Vector3Int blockPos, int face, int size) {
				int vertCount = vertices.Count;

				Cube.Face faceData = Cube.GetFace(face, blockPos, size);

				for (int i = 0; i < 4; i++) {
					vertices.Add(faceData.vertices[i]);
				}
				for (int i = 0; i < 6; i++) {
					triangles.Add(vertCount + faceData.triangles[i]);
				}

				// Where the blocks face textures are stored in the texture array
				int faceTextureIndex = BlockRegister.blockTextureIndexes[block.Name] + block.faceTextureIndexes[face];
				
				// Tile the face at LOD = 1
				/*List<Vector3> uv = new List<Vector3>() {
					new(  0f,   0f, faceTextureIndex),
					new(size,   0f, faceTextureIndex),
					new(  0f, size, faceTextureIndex),
					new(size, size, faceTextureIndex),
				};*/

				// Scale the texture to the size of the nodes LOD
				List<Vector3> uv = new List<Vector3>() {
					new(0f, 0f, faceTextureIndex),
					new(1f, 0f, faceTextureIndex),
					new(0f, 1f, faceTextureIndex),
					new(1f, 1f, faceTextureIndex),
				};
				uvs.AddRange(uv);
			}
		}

		Mesh mesh = new() {
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray(),
			
			//normals = normals.ToArray(),
		};
		//mesh.RecalculateNormals();

		mesh.SetUVs(0, uvs.ToArray());

		chunk.meshFilter.sharedMesh = mesh;
		chunk.meshRenderer.material = BlockRegister.material;

		chunk.meshCollider.sharedMesh = mesh;

		//double end = Time.realtimeSinceStartupAsDouble;
		//double dT = end - start;
		//Debug.Log("Chunk " + chunk.pos + ": MESH generation took " + dT * 1000f + "ms");
	}
}
