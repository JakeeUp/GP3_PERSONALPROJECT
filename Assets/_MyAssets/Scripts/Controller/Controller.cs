using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour, IShootable,IPointOfInterest
{

	public float currentHealth { get; private set; }
	public new Rigidbody rigidbody;
	public float wallCamXPos = 1;
	public Transform wallCamParent;
	public Vector3 startWallCamPos;
	public SkinnedMeshRenderer meshRenderer;

	[Header("Attributes")]
	[Space(5)]
	public float maxHealth = 100f;
	[Header("Movement")]
	[Space(5)]
	public float moveSpeed = .4f;
	public float grabSpeed = .4f;
	public float proneSpeed = .4f;
	public float wallSpeed = .4f;
	public float rotateSpeed = .2f;
	public float fpsRotateSpeed = .2f;
	public float wallCheckDis = .2f;
	

	[Header("Attacking")]
	[Space(5)]
	public float aimSpeed = 1;
	[HideInInspector]
	public Transform mTransform;
	[HideInInspector]
	public InventoryManager inventoryManager;
	public Animator animator;
	public float dmgNumber;

	[Header("Bools")]
	[Space(5)]
	public bool isWall;
	public bool isAiming;
	public bool isFreeLook;
	public bool isGrab;
	public bool isInteracting;
	public bool isFPS;
	public bool isCrouch
	{
		get
		{
			return _isCrouch;
		}
		set
		{
			animator.SetBool("isProne", false);
			_isCrouch = value;
		}
	}
	bool _isCrouch;
	public bool isProne;
	public float enemyDamage;
	public AIController enemy;
	AIController currentGrabbed;
	public PoseStats standing;
	public PoseStats crouching;
	public float getWallDetectOrigin
    {
        get { if (isCrouch)
			{
				return crouching.wallDetectHeight;
			}
			else
			{
				return standing.wallDetectHeight;
			}
		}
    }
	

	CapsuleCollider controllerCollider;

	private void Start()
	{
		mTransform = this.transform;
		rigidbody = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		inventoryManager = GetComponentInParent<InventoryManager>();
		currentHealth += maxHealth;
		controllerCollider = GetComponent<CapsuleCollider>();
		startWallCamPos = wallCamParent.localPosition;
		meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		enemy = FindObjectOfType<AIController>();

		UpdatePoseStats(standing);
		
	}
    private void Update()
    {
		
		//ApplyDamage(enemyDamage);
    }
 //   public void ApplyDamage(float damage)
	//{
	//	//currentHealth -= damage;
	//	//currentHealth = Mathf.Max(currentHealth, 0);

	//	if (currentHealth <= 0)
	//	{
	//		Debug.Log("DEAD");
	//		//HandleDeath();
	//	}
	//}
	void HandleDeath()
	{
		
	}
	public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta, LayerMask layerMask)
	{
		float dot = Vector3.Dot(moveDirection, Vector3.forward);
		Vector3 wallCamTargetPos = startWallCamPos;
		//Debug.Log(dot);
		//moveDirection *= (dot < -0.8f) ? -1 : 1;
		if(dot < 0)
        {
			moveDirection.x *= -1;
        }
		HandleRotation(normal, delta);

		//moveDirection = mTransform.InverseTransformDirection(moveDirection);//mTransform.right * horizontal;

		Vector3 projectVel = Vector3.ProjectOnPlane(moveDirection, normal);
		Debug.DrawRay(mTransform.position, projectVel, Color.blue);


		Vector3 relativeDir = mTransform.InverseTransformDirection(projectVel);

		Vector3 origin = mTransform.position;
		origin.y += 1;
		if ((Mathf.Abs(relativeDir.x) > 0.01f))
		{
			if (relativeDir.x > 0)
				origin += mTransform.right * wallCheckDis;
			if (relativeDir.x < 0)
				origin -= mTransform.right * wallCheckDis;

			Debug.DrawRay(origin, -normal, Color.red);
			if (Physics.Raycast(origin, -normal, out RaycastHit hit, 2, layerMask))
			{

			}
			else
			{
				projectVel = Vector3.zero;
				wallCamTargetPos.x = wallCamXPos * ((relativeDir.x < 0)?-1:1);
				relativeDir.x = 0;
			}
		}
		else
		{
			projectVel = Vector3.zero;
			relativeDir.x = 0;
		}

		rigidbody.velocity = projectVel * wallSpeed;

		float m = 0;

		m = relativeDir.x;

		if (m < 0.1f && m > -0.1f)
		{
			m = 0;
		}
		else
		{
			m = (m < 0) ? -1 : 1;
		}

		animator.SetFloat("movement", m, 0.1f, delta);

		wallCamParent.localPosition = Vector3.Lerp(wallCamParent.localPosition, wallCamTargetPos, delta / 0.2f);

	}
	public void GrabMove(Vector3 moveDirection, float delta)
	{
		rigidbody.velocity = moveDirection * grabSpeed;
	}

	public void Move(Vector3 moveDirection, float delta)
	{
		if (animator.GetBool("canRotate"))
		{
			moveDirection = Vector3.zero;
		}

			float speed = moveSpeed;
		if (isAiming)
			speed = aimSpeed;

		rigidbody.velocity = moveDirection * speed;
	}

	public void CrouchMovement(Vector3 moveDirection, float delta, float moveAmount)
	{
		float dot = Vector3.Dot(moveDirection, mTransform.forward);
		HandleMovementAnimations(moveAmount, delta);
		if (dot > 0)
		{
			Debug.DrawRay(mTransform.position, moveDirection);

			rigidbody.velocity = moveDirection * proneSpeed;

			if (moveAmount > 0)
			{
				isProne = true;
				HandleRotation(moveDirection, delta);
				animator.SetBool("canRotate", false);
			}
		}
		else
		{
			if (moveAmount > 0)
			{
				isProne = false;

				if (animator.GetBool("canRotate"))
				{
					rigidbody.velocity = Vector3.zero;
					HandleRotation(moveDirection, delta);
				}
			}
		}
	}

	public void HandleRotation(Vector3 lookDir, float delta)
	{
		if (lookDir == Vector3.zero)
			lookDir = mTransform.forward;
		Quaternion lookRotation = Quaternion.LookRotation(lookDir);
		mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
	}

	public void FPSRotate(float horizontal, float delta)
	{
		Vector3 targetEuler = mTransform.eulerAngles;
		targetEuler.y += horizontal * delta / fpsRotateSpeed;
		mTransform.eulerAngles = targetEuler;
	}
	public void HandleAnimationStates()
	{
		animator.SetBool("isCrouch", isCrouch);
		animator.SetBool("isWall", isWall);
		animator.SetBool("isAiming", isAiming);
		animator.SetBool("isProne", isProne);
		if(inventoryManager.currentWeaponHook != null)
        {
			inventoryManager.currentWeaponHook.gameObject.SetActive(isAiming);
		}
		//inventoryManager.currentWeapon.model.SetActive(isAiming);
	}

	public void HandleMovementAnimations(float moveAmount, float delta)
	{
		float m = moveAmount;
		if (m > 0.1f && m < 0.51f)
			m = 0.5f;
		if (m > 0.51f)
			m = 1;
		if (m < 0.1f)
			m = 0;

		animator.SetFloat("movement", m, 0.1f, delta);
	}

	float lastShot;

	public void HandleShooting()
	{
		if (enemy == null)
		{
			Debug.LogError("Enemy is not properly initialized in Controller.");
			return;
		}

		if (Time.realtimeSinceStartup - lastShot > inventoryManager.currentWeapon.fireRate)
		{
			lastShot = Time.realtimeSinceStartup;
			inventoryManager.currentWeaponHook.Shoot();

			GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeaponHook);
			if (enemy != null)
			{
				
			}
		}
	}
	public float grabOffset;
	public float grabDistance = 1;



	public void HandleGrab(bool isHolding, bool doubleGrab, bool isTrigger)
	{
		if (currentGrabbed != null)
		{
			if (doubleGrab && !isInteracting)
			{
				Debug.Log("struggle");
				animator.Play("p_grab_struggle");
				currentGrabbed.animator.Play("e_grab_struggle");
				currentGrabbed.timesStruggle++;

				if (currentGrabbed.timesStruggle > 2)
				{
					isGrab = false;
					animator.Play("p_grab_finish");
					currentGrabbed.KillByGrab();
					currentGrabbed = null;
					isHolding = false;
					return;
				}
			}
		}

		if (isHolding)
		{
			if (currentGrabbed == null && isTrigger)
			{
				Vector3 origin = mTransform.position;
				origin.y += 1.5f;
				RaycastHit hit;
				Debug.DrawRay(origin, mTransform.forward * grabDistance, Color.blue, 1, false);
				rigidbody.velocity = Vector3.zero;

				if (Physics.SphereCast(origin, 0.25f, mTransform.forward, out hit, grabDistance))
				{
					AIController aIController = hit.transform.GetComponentInParent<AIController>();

					if (aIController != null)
					{
						if (aIController.isDead == false)
						{
							Vector3 tp = mTransform.forward * grabOffset;
							tp += mTransform.position;
							aIController.StartGrab(tp, mTransform.rotation);
							animator.Play("p_grab_start");
							//animator.SetFloat("Movement", 0);
							isGrab = true;
							currentGrabbed = aIController;
						}
					}
					else
					{
						animator.Play("p_grab_empty");
					}
				}
				else
				{
					animator.Play("p_grab_empty");
				}
			}
			else
			{

			}
		}
		else
		{
			if (currentGrabbed != null)
			{
				isGrab = false;
				animator.Play("p_grab_cancel");
				currentGrabbed.StopGrab(this);
				currentGrabbed = null;
			}
		}
	}

	public void HandleEnemyPositionOnGrab()
	{
		Vector3 tp = mTransform.forward * grabOffset;
		tp += mTransform.position;
		currentGrabbed.transform.position = tp;
		currentGrabbed.transform.rotation = mTransform.rotation;
	}
	public void HandleGrabAnimation(float moveAmount, float delta)
	{
		float m = moveAmount;
		//if (moveAmount > 0)
		//{
		//	m = 1;
		//}

		animator.SetFloat("movement", m, 0.1f, delta);
		currentGrabbed.animator.SetFloat("movement", m, 0.1f, delta);
	}

	public void AttackEnemy(AIController enemy)
	{
		float damage = 20f; // Adjust the damage value as per your game's balance
		enemy.OnHit(damage);
	}

	public void OnHit(float damage)
	{
		if (enemy != null)
		{
			currentHealth -= enemy.DamageAmount;  // Accessing via property
			Debug.Log($"Player is taking: -{enemy.DamageAmount} HP");
		}
		else
		{
			Debug.LogError("AIController is not assigned!");
		}
		// Ensure health doesn't go below zero
		currentHealth = Mathf.Max(currentHealth, 0);
		// Handle other consequences of being hit
	}
	public void ShowHealthWhenDeadTest()
    {
		if(currentHealth <= 0)
        {
			Debug.Log("Player Dead");
        }
    }
	public string hitFx = "blood";
    public string GetHitFx()
    {
		return hitFx;
    }

    public bool OnDetect(AIController aIController)
    {
		aIController.OnDetectPlayer(this);
		return true;
    }

	public Transform GetTransform()
    {
		return mTransform;
    }

	public void UpdatePoseStats(PoseStats pose)
    {
		controllerCollider.height = pose.colliderHeight;
		Vector3 centerPosition = controllerCollider.center;
		centerPosition.y = pose.colliderPosY;
		controllerCollider.center = centerPosition;
    }

	[System.Serializable]
	public class PoseStats
	{
		public float colliderHeight = 2.7f;
		public float colliderPosY = 1.3f;
		public float wallDetectHeight;
	}
}

