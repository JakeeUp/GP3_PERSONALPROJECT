using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
	public Transform camHolder;

	public ExecutionOrder movementOrder;
	public Controller controller;
	public CameraManager cameraManager;
	Vector3 moveDirection;
	public float wallDetectDis = .5f;
	public float wallDetectDisOnWall = 1.2f;
	public float wallAngleThreshold = 35;

	float horizontal;
	float vertical;
	float moveAmount;
	bool freeLook;
	LayerMask ignoreForWall;

	public enum ExecutionOrder
	{
		fixedUpdate, update, lateUpdate
	}

	private void Start()
	{
		cameraManager.wallCameraObject.SetActive(false);
		cameraManager.mainCameraObject.SetActive(true);
		cameraManager.fpsCameraObject.SetActive(false);
		cameraManager.mainCamera.cullingMask = ~0;

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
		controller.isAiming = Input.GetMouseButton(1);
		freeLook = Input.GetKey(KeyCode.F);

		if (freeLook)
		{
			if (controller.isFreeLook == false)
			{
				controller.isFreeLook = true;
				controller.isAiming = false;
				controller.isProne = false;
				cameraManager.fpsCameraObject.SetActive(true);
				controller.rigidbody.velocity = Vector3.zero;
				cameraManager.mainCamera.cullingMask = ~(1 << 7);

			}
		}
		else
		{
			if (controller.isFreeLook)
			{
				controller.isFreeLook = false;
				cameraManager.fpsCameraObject.SetActive(false);
				cameraManager.mainCamera.cullingMask = ~0;
			}
		}

		moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

		moveDirection = Vector3.forward * vertical;
		moveDirection += Vector3.right * horizontal;
		moveDirection.Normalize();

		if (Input.GetKeyDown(KeyCode.C))
		{
			controller.isCrouch = !controller.isCrouch;

			if (!controller.isWall)
				moveDirection = Vector3.zero;
		}

		float delta = Time.deltaTime;

		if (controller.isFreeLook)
		{
			controller.FPSRotate(horizontal, delta);
		}
		else
		{
			if (controller.isAiming)
			{
				//controller.isWall = false;
				controller.isCrouch = false;
				controller.HandleRotation(moveDirection, delta);

				if (Input.GetMouseButton(0))
				{
					controller.HandleShooting();
				}

				if (controller.inventoryManager.currentWeapon.canMoveWithWeapon)
				{
					controller.Move(moveDirection, delta);
					controller.HandleMovementAnimations(moveAmount, delta);
				}
				else
				{
					controller.HandleMovementAnimations(0, delta);
					controller.rigidbody.velocity = Vector3.zero;
				}
			}
			else
			{
				if (movementOrder == ExecutionOrder.update)
				{
					HandleMovement(moveDirection, delta);
				}
			}
		}

		controller.HandleAnimationStates();
	}

	void HandleMovement(Vector3 moveDirection, float delta)
	{
		Vector3 origin = controller.transform.position;
		origin.y += 1;


		bool willStickToWall = false;
		Vector3 wallNormal = Vector3.zero;

		float detectDis = wallDetectDis;
		if (controller.isWall)
		{
			detectDis = wallDetectDisOnWall;
		}

		Debug.DrawRay(origin, moveDirection * detectDis);

		if (Physics.SphereCast(origin, 0.25f, moveDirection, out RaycastHit hit, detectDis, ignoreForWall))
		{
			willStickToWall = true;
			wallNormal = hit.normal;
		}

		if (willStickToWall)
		{
			controller.isProne = false;
			controller.isWall = true;
			controller.WallMovement(moveDirection, wallNormal, delta, ignoreForWall);
			cameraManager.wallCameraObject.SetActive(true);
			cameraManager.mainCameraObject.SetActive(false);
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