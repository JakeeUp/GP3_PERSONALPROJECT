using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour, IShootable
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
	Animator animator;
	public float dmgNumber;

	[Header("Bools")]
	[Space(5)]
	public bool isWall;
	public bool isAiming;
	public bool isFreeLook;
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

	private void Start()
	{
		mTransform = this.transform;
		rigidbody = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		inventoryManager = GetComponentInParent<InventoryManager>();
		currentHealth += maxHealth;
		startWallCamPos = wallCamParent.localPosition;
		meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		enemy = FindObjectOfType<AIController>();
		
		
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
		inventoryManager.currentWeapon.model.SetActive(isAiming);
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
		if (Time.realtimeSinceStartup - lastShot > inventoryManager.currentWeapon.fireRate)
		{
			lastShot = Time.realtimeSinceStartup;
			inventoryManager.currentWeapon.muzzle.Play();

			GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeapon.weaponSpread);

		}
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
}

