using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Controller : MonoBehaviour, IShootable, IPointOfInterest
{
	[Header("Attributes")]
	[Space(5)]
	public float maxHealth = 100f;
	[SerializeField]private float _currentHealth;


	[Header("Movement")]
	[Space(5)]
	[SerializeField] private float _moveSpeed = 6f;
	[SerializeField] private float _grabSpeed = .8f;
	[SerializeField] private float _proneSpeed = 1.2f;
	[SerializeField] private float _wallSpeed = 1f;
    [SerializeField] private float _rotateSpeed = .1f;
    [SerializeField] private float _fpsRotateSpeed = .01f;
    [SerializeField] private float _wallCheckDis = .7f;

    
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
	[SerializeField]private bool _isWall;
	[SerializeField]private bool _isAiming;
	[SerializeField]private bool _isFreeLook;
    [SerializeField]private bool _isGrab;
	[SerializeField]private bool _isInteracting;
	[SerializeField]private bool _isFPS;
	[SerializeField]private bool _isProne;
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

    [Header("Sound")]
    [Space(5)]
    public AudioSource spottedSoundSource;
    public AudioClip spottedSound;
	public float timeSinceLastPlay;

    [Header("Other")]
    [Space(5)]
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

	[HideInInspector]
	public GameObject storedObject;
	[HideInInspector]
	public Animator boxAnimator;


    public float currentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
        }
    }
    public new Rigidbody rigidbody;
    public float wallCamXPos = 1;
    public Transform wallCamParent;
    public Vector3 startWallCamPos;
    public SkinnedMeshRenderer meshRenderer;
    public enum ControllerState
    {
        normal, cardboardBox, prone
    }
    public ControllerState controllerState;

   
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
		spottedSoundSource = GetComponent<AudioSource>();
		timeSinceLastPlay = enemy.alarmTimer;
		UpdatePoseStats(standing);
		
	}
    private void Update()
    {

        Death();

		timeSinceLastPlay += Time.deltaTime;
    }

    private void Death()
    {
        if (currentHealth <= 0)
        {
            Debug.Log("dead emoji");
        }
    }

    private void _WallMovement(Vector3 moveDirection, Vector3 normal, float delta, LayerMask layerMask)
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
	private void _GrabMove(Vector3 moveDirection, float delta)
	{
		rigidbody.velocity = moveDirection * grabSpeed;
	}
	private void _Move(Vector3 moveDirection, float delta)
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
	private void _CrouchMovement(Vector3 moveDirection, float delta, float moveAmount)
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
	private void _HandleRotation(Vector3 lookDir, float delta)
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

    #region FunctionCalls
	public void WallMovement(Vector3 moveDirection, Vector3 normal, float delta, LayerMask layerMask)
	{
		_WallMovement(moveDirection, normal, delta, layerMask);
	}

	public void GrabMove(Vector3 moveDirection, float delta)
	{
		_GrabMove(moveDirection, delta);
	}
	public void Move(Vector3 moveDirection, float delta)
	{
		_Move(moveDirection, delta);
	}
	public void CrouchMovement(Vector3 moveDirection, float delta, float moveAmount)
	{
		_CrouchMovement(moveDirection, delta, moveAmount);
	}


	public void HandleRotation(Vector3 lookDir, float delta)
	{
		_HandleRotation(lookDir, delta);
	}
    #endregion
	public void HandleMovementAnimations(float moveAmount, float delta)
	{
		float m = moveAmount;
		if (m > 0.1f && m < 0.51f)
			m = 0.5f;
		if (m > 0.51f)
			m = 1;
		if (m < 0.1f)
			m = 0;
		switch(controllerState)
        {
			case ControllerState.normal:
				animator.SetFloat("movement", m, 0.1f, delta);
				break;
			case ControllerState.cardboardBox:
				boxAnimator.SetFloat("movement", m, 0.1f, delta);
				break;
			case ControllerState.prone:
				break;
			default:
				break;

        }
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

	private void _HandleEnemyPositionOnGrab()
	{
		Vector3 tp = mTransform.forward * grabOffset;
		tp += mTransform.position;
		currentGrabbed.transform.position = tp;
		currentGrabbed.transform.rotation = mTransform.rotation;
	}

	public void HandleEnemyPositionOnGrab()
	{
		_HandleEnemyPositionOnGrab();
	}
	private void _HandleGrabAnimation(float moveAmount, float delta)
	{
		float m = moveAmount;
		//if (moveAmount > 0)
		//{
		//	m = 1;
		//}

		animator.SetFloat("movement", m, 0.1f, delta);
		currentGrabbed.animator.SetFloat("movement", m, 0.1f, delta);
	}

	public void HandleGrabAnimation(float moveAmount, float delta)
	{
		_HandleGrabAnimation(moveAmount, delta);
	}
	private void AttackEnemy(AIController enemy)
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
	
	public string hitFx = "blood";
    public string GetHitFx()
    {
		return hitFx;
    }

    public bool OnDetect(AIController aIController)
    {
		Debug.Log("Player Spotted");

		if(timeSinceLastPlay >= enemy.alarmTimer)
		{
            spottedSoundSource.PlayOneShot(spottedSound);

            timeSinceLastPlay = 0;
		}
		if(controllerState == ControllerState.cardboardBox)
        {
			if (rigidbody.velocity.sqrMagnitude > 0.1f)
				aIController.OnDetectPlayer(this);
			else
				return false;
        }
		else
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


    #region Variable Calls
    public float moveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    public float grabSpeed { get { return _grabSpeed; } set { _grabSpeed = value; } }
    public float proneSpeed { get { return _proneSpeed; } set { _proneSpeed = value; } }
    public float wallSpeed { get { return _wallSpeed; } set { _wallSpeed = value; } }
    public float rotateSpeed { get { return _rotateSpeed; } set { _rotateSpeed = value; } }
    public float fpsRotateSpeed { get { return _fpsRotateSpeed; } set { _fpsRotateSpeed = value; } }
    public float wallCheckDis { get { return _wallCheckDis; } set { _wallCheckDis = value; } }
    public bool isWall { get { return _isWall; } set { _isWall = value; } }
    public bool isAiming { get { return _isAiming; } set { _isAiming = value; } }
    public bool isFreeLook { get { return _isFreeLook; } set { _isFreeLook = value; } }
    public bool isGrab { get { return _isGrab; } set { _isGrab = value; } }
    public bool isInteracting { get { return _isInteracting; } set { _isInteracting = value; } }
    public bool isFPS { get { return _isFPS; } set { _isFPS = value; } }
    public bool isProne { get { return _isProne; } set { _isProne = value; } }

    #endregion



    [System.Serializable]
	public class PoseStats
	{
		public float colliderHeight = 2.7f;
		public float colliderPosY = 1.3f;
		public float wallDetectHeight;
	}

}

