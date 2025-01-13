using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
	public Camera camera;
	public GameObject gameObject; // is this necessary?

	public float walkingSpeed = 5f;
	public float runningSpeed = 10f;
	public Vector2 sensitivity = new(1, 1);

	public bool isTargetingBlock = false;

	public Player(GameObject gameObject) {
		this.gameObject = gameObject;
		camera = Camera.main;
	}

	public Vector3Int RoundToInt(Vector3 v) {
		return new(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}

	public bool HandleBlockInteraction(out Chunk chunkToRemesh) {
		RaycastHit hit;
		bool isTargetingBlock = Physics.Raycast(camera.transform.position, camera.transform.forward, out hit);
		if (isTargetingBlock) {
			Region targetRegion = hit.transform.gameObject.GetComponent<Chunk>().Region;
			Transform regionTransform = targetRegion.transform;

			// w_ = world-space coordinates
			Vector3 w_point = hit.point;
			Vector3 w_normal = hit.normal;

			// r_ = region-space coordinates
			Vector3 r_point = regionTransform.InverseTransformPoint(w_point - w_normal * 0.5f);
			Vector3Int r_blockPos = Vector3Int.FloorToInt(r_point);
			
			if (Input.GetMouseButtonDown(0)) {
				return BreakOrPlaceBlock(false, out chunkToRemesh);
			} else if (Input.GetMouseButtonDown(1)) {
				return BreakOrPlaceBlock(true, out chunkToRemesh);
			}

			bool BreakOrPlaceBlock(bool isPlacing, out Chunk chunkToRemesh) {
				Region targetRegion = hit.transform.gameObject.GetComponent<Chunk>().Region;
				Transform regionTransform = targetRegion.transform;

				// w_ = world-space coordinates
				Vector3 w_point = hit.point;
				Vector3 w_normal = hit.normal;

				// r_ = region-space coordinates
				Vector3 r_point = regionTransform.InverseTransformPoint(w_point - w_normal * 0.5f);
				Vector3Int r_blockPos = Vector3Int.FloorToInt(r_point);

				int blockID = 0;
				if (isPlacing) {
					blockID = BlockRegister.GetBlockIndex("template");
					Vector3Int r_normal = RoundToInt(regionTransform.InverseTransformVector(w_normal));
					r_blockPos += r_normal;
				}

				return targetRegion.TrySetBlock(r_blockPos, blockID, out chunkToRemesh);
			}
		}

		chunkToRemesh = null;
		return false;
	}

	public void TryMove() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		Vector3 leftright = Input.GetAxisRaw("Left/Right") * gameObject.transform.right;
		Vector3 updown = Input.GetAxisRaw("Up/Down") * gameObject.transform.up;
		Vector3 forwardbackward = Input.GetAxisRaw("Forward/Backward") * gameObject.transform.forward;

		Vector3 input = leftright + updown + forwardbackward;

		float speed = Input.GetKey(KeyCode.LeftControl) ? runningSpeed : walkingSpeed;
		Vector3 velocity = input.normalized * speed;

		gameObject.transform.position += velocity * Time.deltaTime;

		float mouseX = Input.GetAxisRaw("Mouse X");
		float mouseY = -Input.GetAxisRaw("Mouse Y");
		camera.transform.Rotate(new Vector3(mouseY * sensitivity.y, 0, 0));
		gameObject.transform.Rotate(new Vector3(0, mouseX * sensitivity.x, 0));
	}
}
