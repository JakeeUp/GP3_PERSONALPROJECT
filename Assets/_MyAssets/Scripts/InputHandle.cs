using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class InputHandler : MonoBehaviour
	{
		public Transform camHolder;

		public ExecutionOrder movementOrder;
		public Controller controller;
		public CameraManager cameraManager;

		Vector3 moveDirection;
		public float wallDetectDis = .5f;

		float horizontal;
		float vertical;
		float moveAmount;

		LayerMask ignoreForWall;

		public enum ExecutionOrder
		{
			fixedUpdate, update, lateUpdate
		}

		private void Start()
		{
			cameraManager.wallCameraObject.SetActive(false);
			cameraManager.mainCameraObject.SetActive(true);

			ignoreForWall = ~(1 << 6);
		}

		private void FixedUpdate()
		{
			if (movementOrder == ExecutionOrder.fixedUpdate)
			{
				HandleMovement(moveDirection, Time.fixedDeltaTime);
			}
		}

		private void Update()
		{
			horizontal = Input.GetAxis("Horizontal");
			vertical = Input.GetAxis("Vertical");

			if (Input.GetKeyDown(KeyCode.C))
			{
				controller.isCrouch = !controller.isCrouch;

			}

			moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

			moveDirection = Vector3.forward * vertical;
			moveDirection += Vector3.right * horizontal;
			moveDirection.Normalize();

			float delta = Time.deltaTime;

			if (movementOrder == ExecutionOrder.update)
			{
				HandleMovement(moveDirection, delta);
			}

			controller.HandleAnimationStates();
		}

		void HandleMovement(Vector3 moveDirection, float delta)
		{
			Vector3 origin = controller.transform.position;
			origin.y += 1;

			Debug.DrawRay(origin, moveDirection * wallDetectDis);
			if (Physics.SphereCast(origin, 0.25f, moveDirection, out RaycastHit hit, wallDetectDis, ignoreForWall))
			{
				cameraManager.wallCameraObject.SetActive(true);
				cameraManager.mainCameraObject.SetActive(false);

				controller.isWall = true;
				controller.WallMovement(moveDirection, hit.normal, delta, ignoreForWall);
			}
			else
			{
				controller.isWall = false;
				cameraManager.wallCameraObject.SetActive(false);
				cameraManager.mainCameraObject.SetActive(true);

				if (controller.isCrouch)
				{
					controller.CrouchMovement(moveDirection, delta, moveAmount);
				}
				else
				{
					controller.Move(moveDirection, delta);
					controller.HandleRotation(moveDirection, delta);
					controller.HandleMovementAnimations(moveAmount, delta);
				}
			}

		}
	}
}
