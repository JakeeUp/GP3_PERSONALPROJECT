using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Controller : MonoBehaviour
{

	public new Rigidbody rigidbody;
	public float moveSpeed = .4f;
	public float proneSpeed = .4f;
	public float wallSpeed = .4f;
	public float rotateSpeed = .2f;
	public float wallCheckDis = .2f;
	public float aimSpeed = 1;
	[HideInInspector]
	public Transform mTransform;
	[HideInInspector]
	public InventoryManager inventoryManager;
	Animator animator;

	public bool isWall;
	public bool isAiming;
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

	private void Start()
	{
		mTransform = this.transform;
		rigidbody = GetComponent<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		inventoryManager = GetComponentInParent<InventoryManager>();
	}

	public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta, LayerMask layerMask)
	{
		//float dot = Vector3.Dot(moveDirection, Vector3.forward);
		//Debug.Log(dot);
		//moveDirection *= (dot < -0.8f) ? -1 : 1;

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
				relativeDir.x = 0;
			}
		}
		else
		{
			projectVel = Vector3.zero;
			relativeDir.x = 0;
		}

		rigidbody.velocity = projectVel * wallSpeed;
		HandleRotation(-normal, delta);

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

	}

	public void Move(Vector3 moveDirection, float delta)
	{
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
				animator.SetBool("isProne", true);
				HandleRotation(moveDirection, delta);
				animator.SetBool("canRotate", false);
			}
		}
		else
		{
			if (moveAmount > 0)
			{
				animator.SetBool("isProne", false);

				if (animator.GetBool("canRotate"))
				{
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

	public void HandleAnimationStates()
	{
		animator.SetBool("isCrouch", isCrouch);
		animator.SetBool("isWall", isWall);
		animator.SetBool("isAiming", isAiming);
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

			RaycastHit hit;
			Vector3 origin = Random.insideUnitCircle * inventoryManager.currentWeapon.weaponSpread;
			origin = mTransform.TransformPoint(origin);
			origin.y += 1.3f;
			//origin += randomPosition;
			Debug.DrawRay(origin, mTransform.forward * 100, Color.white);

			if (Physics.Raycast(origin, mTransform.forward, out hit, 100))
			{
				IShootable shootable = hit.transform.GetComponent<IShootable>();
				if (shootable != null)
				{
					Debug.Log("Hit");
					GameObject fx = GameReferences.objectPooler.GetObject(shootable.GetHitFx());
					fx.transform.position = hit.point;
					fx.transform.rotation = Quaternion.LookRotation(hit.normal);
					fx.SetActive(true);
				}
				else
				{
					Debug.Log("Hit Other");
					GameObject fx = GameReferences.objectPooler.GetObject("default");
					fx.transform.position = hit.point;
					fx.transform.rotation = Quaternion.LookRotation(hit.normal);
					fx.SetActive(true);
				}
			}
		}
	}

}

