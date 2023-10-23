using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
	public Transform camHolder;

	public ExecutionOrder movementOrder;
	public Controller controller;

	public CameraManager cameraManager;
	Vector3 moveDirection;

	Vector2 moveInputDirection;
	Vector2 lookInputDirection;


	public float wallDetectDis = .5f;
	public float wallDetectDisOnWall = 1.2f;
	public float wallAngleThreshold = 35;

	float horizontal;
	float vertical;
	float moveAmount;
	bool freeLook;
	bool grabInput;
	bool isFPSinit;
	float grabDeadTimer;
	LayerMask ignoreForWall;
	PlayerControls inputActions;
	public Transform wallCameraTarget;

	public enum ExecutionOrder
	{
		fixedUpdate, update, lateUpdate
	}

	private void Start()
	{
		inputActions = new PlayerControls();
		inputActions.Player.Movement.performed += i => moveInputDirection = i.ReadValue<Vector2>();
		cameraManager.wallCameraObject.SetActive(false);
		cameraManager.mainCameraObject.SetActive(true);
		cameraManager.fpsCameraObject.SetActive(false);
		cameraManager.mainCamera.cullingMask = ~0;
		inputActions.Enable();
		ignoreForWall = ~(1 << 9 | 1 << 12 | 1 << 13);
		GameReferences.ignoreForShooting = ~(1 << 12 | 1 << 13);
		GameReferences.controllersLayer = (1 << 9);

		Debug.Log("Initializing UIManager.");
		UIManager.singleton.Init(controller.inventoryManager);  
		if (controller.inventoryManager == null)
		{
			Debug.LogError("InventoryManager is null in InputHandler.");
		}

		List<Jacob.Utilities.IIcon> l = new List<Jacob.Utilities.IIcon>();
		l.AddRange(ResourcesManager.singleton.GetAllItems());

		Jacob.Utilities.IconMaker.RequestIconForList(l, UpdateUIManagerWithItems);

		//DontDestroyOnLoad(this.gameObject);
	}
	void UpdateUIManagerWithItems()
	{
		List<Item> l = new List<Item>();
		l.AddRange(ResourcesManager.singleton.GetAllItems());
		UIManager.singleton.CreateSlotsForItemList(l);
	}
	private void OnDisable()
	{
		inputActions.Disable();
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
		float delta = Time.deltaTime;
		bool isLeftBumperPressed = Input.GetKey(KeyCode.LeftArrow);
		bool isRightBumperPressed = Input.GetKey(KeyCode.RightArrow);

		bool isInventory = UIManager.singleton.Tick(moveInputDirection.y, delta, isLeftBumperPressed, false);


		if (isInventory)
			return;

		horizontal = Input.GetAxis("Horizontal");
		vertical = Input.GetAxis("Vertical");
		controller.isAiming = Input.GetMouseButton(1);
		freeLook = Input.GetKey(KeyCode.F);
		bool rawGrabInputHold = Input.GetMouseButton(0);
		bool rawGrabInputDown = Input.GetMouseButtonDown(0);
		bool doubleGrab = false;
		bool switchWeapon = Input.GetKeyDown(KeyCode.Q);

		if(Input.GetKey(KeyCode.Escape))
		{
            SceneManager.LoadScene("MainMenuScene");
        }

		if (rawGrabInputHold)
		{
			grabInput = true;
			if (grabDeadTimer > 0)
			{
				doubleGrab = true;
			}

			grabDeadTimer = 0;
		}
		else
		{
			grabDeadTimer += delta;
			if (grabDeadTimer > 1)
			{
				grabInput = false;
			}
		}

		controller.isInteracting = controller.animator.GetBool("isInteracting");

		if (switchWeapon)
		{
			controller.inventoryManager.SwitchWeapon();
		}

		if (controller.isAiming)
		{
			grabInput = false;
			freeLook = false;
		}
		if (grabInput)
		{
			freeLook = false;
		}
		if (freeLook)
		{
			float rotationSpeed = 50f;
			if (Input.GetKey(KeyCode.O))
			{
				camHolder.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.Self);
			}
			if (Input.GetKey(KeyCode.P))
			{
				camHolder.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
			}
		}
		if (freeLook)
		{
			if (controller.isFreeLook == false)
			{
				controller.isFreeLook = true;
				controller.isAiming = false;
				controller.isProne = false;
				cameraManager.fpsCameraObject.SetActive(true);
				controller.rigidbody.velocity = Vector3.zero;
				cameraManager.mainCamera.cullingMask = ~(1 << 10);

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

		if (controller.isInteracting)
		{
			controller.rigidbody.velocity = Vector3.zero;
			return;
		}

		controller.HandleGrab(grabInput, doubleGrab, rawGrabInputDown);

		if (controller.isFPS)
		{
			if(!isFPSinit)
            {
				cameraManager.fpsCameraObject.SetActive(true);
				cameraManager.mainCamera.cullingMask = ~(1 << 7);
				isFPSinit = true;
            }
			//moveDirection = controller.mTransform.forward * vertical;
			//moveDirection += controller.mTransform.right * horizontal;
			moveDirection = controller.mTransform.forward * moveInputDirection.y;
			moveDirection += controller.mTransform.right * moveInputDirection.x;
			moveDirection.Normalize();
			controller.FPSRotate(horizontal, delta);
			controller.Move(moveDirection, delta);

			return;
		}

		isFPSinit = false;
		//moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
		//moveDirection = Vector3.forward * vertical;
		//moveDirection += Vector3.right * horizontal;
		moveAmount = Mathf.Clamp01(Mathf.Abs(moveInputDirection.x) + Mathf.Abs(moveInputDirection.y));
		moveDirection = Vector3.forward * moveInputDirection.y;
		moveDirection += Vector3.right * moveInputDirection.x;
		moveDirection.Normalize();

		if (Input.GetKeyDown(KeyCode.C))
		{
			controller.isCrouch = !controller.isCrouch;

			if(controller.isCrouch)
            {
				controller.UpdatePoseStats(controller.crouching);
            }
			else
            {
				controller.UpdatePoseStats(controller.standing);
            }
			if (!controller.isWall)
				moveDirection = Vector3.zero;
		}
		

		if (controller.isFreeLook)
		{
			controller.FPSRotate(horizontal, delta);
		}
		else
		{
			if (controller.inventoryManager.currentWeaponHook == null)
			{
				controller.isAiming = false;
			}

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
		if (controller.isGrab)
		{
			controller.HandleGrabAnimation(moveAmount, delta);

			if (moveAmount == 1)
			{
				controller.GrabMove(moveDirection, delta);
				controller.HandleRotation(-moveDirection, delta);
			}

			controller.HandleEnemyPositionOnGrab();
			return;
		}

		Vector3 origin = controller.transform.position;
		origin.y += controller.getWallDetectOrigin;


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
			wallCameraTarget.transform.position = controller.transform.position;
			wallCameraTarget.transform.rotation = Quaternion.LookRotation(wallNormal);
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