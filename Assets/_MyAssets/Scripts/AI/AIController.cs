using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;


public class AIController : MonoBehaviour, IShootable, IPointOfInterest
{
	NavMeshAgent agent;
	new Rigidbody rigidbody;
	public Animator animator;

	InventoryManager inventoryManager;
	public float currentHealth { get; private set; }
	public float maxHealth = 100f;
	[Header("Waypoint Index")]
	[Space(5)]
	public int index;
	public Waypoint[] waypoints;
	Waypoint currentWaypoint;
	Transform mTransform;

	[Header("Bools")]
	[Space(5)]
	[SerializeField]private bool isAgressive;
	[SerializeField] private bool isCaution;
	[SerializeField] private bool isGrab;
	public bool isDead;
	public bool isSpottedDead;

	[Header("Wait Timer")]
	[Space(5)]
	float waitTimer;
	float cautionTimer;
	public float alarmTimer;
	

	public float cautionTimerNormal = .7f;

	[Header("Attributes")]
	[Space(5)]
	public float normalSpeed = 2;
	public float aggressiveSpeed = 4;
	public float rotateSpeed = .5f;

 

    public float fovRadius = 20;
	public float fovAngle = 45;

	[Header("Attack Attributes")]
	[Space(5)]
	[SerializeField]private float damageAmount = 10f;
	public float weaponSpread = .3f;
	public int magBullets = 40;
	int bulletsToFire;
	int timesShot;
	public int timesStruggle;
	float lastCautionPlayed;
	public float DamageAmount
	{
		get { return damageAmount; }
		set { damageAmount = value; }
	}

	public float attackDistance = 5;
	Vector3 lastKnownPosition;
	Vector3 lastKnownDirection;

	Controller currentTarget;

	LayerMask controllerLayer;
	LayerMask ignoreForDetection;

	public TextMeshPro emotionText;
	public GameObject emotionObj;

    [Header("Sound Attributes")]
    [Space(5)]
    [SerializeField]private AudioSource hitSoundSource;
	[SerializeField]private AudioClip[] hitSoundClips;
	private void Start()
	{
		hitSoundSource = GetComponent<AudioSource>();
		agent = GetComponentInChildren<NavMeshAgent>();
		rigidbody = GetComponentInChildren<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		inventoryManager = GetComponentInChildren<InventoryManager>();
		if(waypoints.Length > 0)
			currentWaypoint = waypoints[index];

		mTransform = this.transform;
		animator.applyRootMotion = false;
		controllerLayer = (1 << 9 );
		ignoreForDetection = ~( 1 << 12 | 1 << 13);
		currentHealth = maxHealth;
		GameReferences.damage = damageAmount;

	}
	private void Update()
	{

		float delta = Time.deltaTime;
		if (currentHealth <= 0)
		{
			animator.Play("grab_death");
			this.enabled = false;
			Debug.Log("Enemy Dead");
			isDead = true;
		}
		
		if (isGrab)
		{
			//agent.isStopped = true;
			return;
		}

		if (animator.GetBool("isInteracting"))
		{
			agent.isStopped = true;
			if (animator.GetBool("canRotate"))
			{
				HandleLookAtTarget(delta);
			}

			return;
		}

		animator.SetBool("isAggressive", isAgressive);
		animator.SetBool("isCaution", isCaution);


		if (!isAgressive)
		{
			agent.speed = normalSpeed;
			HandleDetection();
			HandleNormalLogic(delta);
		}
		else
		{
			if (isCaution)
			{
				if (cautionTimer < 0)
				{
					isCaution = false;
					agent.isStopped = false;
				}
				else
				{
					if (animator.GetBool("canRotate"))
					{
						HandleLookAtTarget(delta);
					}

					agent.isStopped = true;
					cautionTimer -= delta;
				}
			}
			else
			{
				agent.speed = aggressiveSpeed;
				HandleAggressiveLogic(delta);
			}

			if(alarmTimer > 0)
            {
				alarmTimer -= delta;
            }
			else
            {
				alarmTimer = 0;
				isCaution = false;
				isAgressive = false;
				currentTarget = null;
            }
		}
	}

	void HandleNormalLogic(float delta)
	{
		if (waypoints.Length == 0)
			return;


		currentWaypoint = waypoints[index];

		float dis = Vector3.Distance(mTransform.position, currentWaypoint.targetPosition.position);
		if (dis > agent.stoppingDistance)
		{
			animator.SetFloat("movement", 1, 0.1f, delta);
			agent.updateRotation = true;

			if (agent.hasPath == false)
				agent.SetDestination(currentWaypoint.targetPosition.position);
		}
		else
		{
			animator.SetFloat("movement", 0, 0.1f, delta);

			agent.updateRotation = false;
			Quaternion targetRot = Quaternion.Euler(currentWaypoint.lookEulers);
			mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);

			if (waitTimer < currentWaypoint.waitTime)
			{
				waitTimer += delta;
			}
			else
			{
				waitTimer = 0;
				index++;
				if (index > waypoints.Length - 1)
				{
					index = 0;
				}
			}
		}
	}

	public float fireRate = .1f;
	float currentFire;
	bool initRange;

	void HandleAggressiveLogic(float delta)
	{
		if (currentTarget != null)
		{
			if (!RaycastToTarget(currentTarget))
			{
				lastKnownDirection = (currentTarget.mTransform.position - lastKnownPosition).normalized;
				hasTargetRotation = true;
				scanTime = Random.Range(minScanTime, maxScanTime);
				aIPhase = AIPhase.scanRan;
				currentTarget = null;
			}
		}

		bool inRange = false;

		float dis = Vector3.Distance(lastKnownPosition, mTransform.position);
		agent.SetDestination(lastKnownPosition);
		#region Handle Raycast to target
		if (currentTarget != null)
		{
			if (dis < attackDistance)
			{
				inRange = true;

				if (!initRange)
				{
					AssignRanomBulletsToFire();
					PlayCautionState(cautionTimerNormal, delta, false);
					currentFire = fireRate;
					initRange = true;
				}
				agent.isStopped = true;

				HandleLookAtTarget(delta);

				if (currentFire < 0)
				{
					currentFire = fireRate;
					HandleShooting();

					if (bulletsToFire <= 0)
					{
						AssignRanomBulletsToFire();
						PlayCautionState(cautionTimerNormal, delta, false);
					}
				}
				else
				{
					currentFire -= delta;
				}
			}
			else
			{
				initRange = false;
				agent.updateRotation = true;
				agent.isStopped = false;
				HandleDetection();
			}
		}
		else
		{
			initRange = false;
			agent.updateRotation = true;
			agent.isStopped = false;
			HandleDetection();

			if (agent.remainingDistance < agent.stoppingDistance || agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
			{
				if (hasTargetRotation)
				{
					aIPhase = AIPhase.scanRan;
					HandleRotation(lastKnownDirection, delta);

					scanTime -= delta;
					if (scanTime < 0)
					{
						hasTargetRotation = false;

						int ran = Random.Range(0, 100);
						if (ran > 50)
						{
							aIPhase = AIPhase.searchRan;
						}
					}
				}
				else
				{
					switch (aIPhase)
					{
						case AIPhase.scanRan:
							FindRandomLookDirection();
							break;
						case AIPhase.searchRan:
							SearchRandomPosition();
							FindRandomLookDirection();
							break;
						case AIPhase.searchPOI:
							break;
						default:
							break;
					}
				}
			}
		}
		#endregion

		#region Handle animations
		if (currentTarget != null)
		{
			if (!inRange)
			{
				animator.SetFloat("movement", 1, 0.1f, delta);
			}
			else
			{
				animator.SetFloat("movement", 0);
			}
		}
		else
		{
			if (agent.desiredVelocity.magnitude > 0)
			{
				animator.SetFloat("movement", 1, 0.1f, delta);
			}
			else
			{
				animator.SetFloat("movement", 0, 0.1f, delta);
			}
		}
		#endregion
	}

	void FindRandomLookDirection()
	{
		Vector2 r = Random.insideUnitCircle;
		lastKnownDirection.x = r.x;
		lastKnownDirection.z = r.y;

		scanTime = Random.Range(minScanTime, maxScanTime);
		hasTargetRotation = true;
	}

	void SearchRandomPosition()
	{
		Vector3 r = Random.insideUnitSphere * fovRadius;

		if (NavMesh.SamplePosition(mTransform.position + r, out NavMeshHit hit, 5, NavMesh.AllAreas))
		{
			lastKnownPosition = hit.position;
		}
	}

	public ParticleSystem muzzleFire;

	public enum AIPhase
	{
		scanRan, searchRan, searchPOI
	}

	[Header("Scan Settings")]
	[Space(5)]
	public AIPhase aIPhase;
	public float scanTime;
	public float minScanTime = 1;
	public float maxScanTime = 3;
	public bool hasTargetRotation;

	void AssignRanomBulletsToFire()
	{
		bulletsToFire = Random.Range(5, 20);
		int bl = magBullets - timesShot;

		if (bulletsToFire > bl)
		{
			bulletsToFire = bl;
		}
	}

	void HandleLookAtTarget(float delta)
	{
		//Vector3 dir = currentTarget.mTransform.position - mTransform.position;
		Vector3 dir = lastKnownPosition - mTransform.position;
		HandleRotation(dir, delta);
	}

	void HandleRotation(Vector3 dir, float delta)
	{
		dir.y = 0;
		if (dir == Vector3.zero)
			dir = mTransform.forward;

		Quaternion targetRot = Quaternion.LookRotation(dir);
		mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);
		agent.updateRotation = false;
	}

	void HandleShooting()
	{
		timesShot++;
		bulletsToFire--;
		//muzzleFire.Play();
		GameReferences.RaycastShoot(mTransform, inventoryManager.currentWeaponHook);
		inventoryManager.currentWeaponHook.Shoot();

		if (timesShot > magBullets)
		{
			timesShot = 0;
			animator.CrossFade("Reload", 0.2f);
			animator.CrossFade("Reload_Body", 0.2f);
		}
	}

	void PlayCautionState(float timer, float delta, bool crossfadeToState = true)
	{
		isCaution = true;
		cautionTimer = timer;

		if(!isGrab)
        {
			if (crossfadeToState)
				animator.CrossFade("caution", 0.2f);
		}

		
		animator.SetFloat("movement", 0, 0.1f, delta);
		isCaution = true;
	}

	bool RaycastToTarget(IPointOfInterest poi)
	{
		Vector3 dir = poi.GetTransform().position - mTransform.position;
		dir.Normalize();
		float angle = Vector3.Angle(mTransform.forward, dir);
		if (angle < fovAngle)
		{
			Vector3 o = mTransform.position;
			o.y += 1;

			Debug.DrawRay(o, dir * 50, Color.red);
			if (Physics.Raycast(o, dir, out RaycastHit hit, 100, ignoreForDetection))
			{
				IPointOfInterest pointOfInterest = hit.transform.GetComponentInParent<IPointOfInterest>();

				if (pointOfInterest != null)
				{
					return pointOfInterest.OnDetect(this);
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	public void OnDetectPlayer(Controller targetPlayer)
	{
		alarmTimer = 25;
		currentTarget = targetPlayer;
		lastKnownPosition = currentTarget.transform.position;
		SetToCautiousState();
	}

	public void SetToCautiousState(bool force = false)
	{
		if (!isAgressive || force)
		{
			emotionText.text = "?!";
			emotionObj.SetActive(true);


			cautionTimer = cautionTimerNormal;
			isCaution = true;
			isAgressive = true;
			alarmTimer = 25;

			if(!isGrab)
            {
				animator.CrossFade("caution", 0.2f);

			}
			GameReferences.UpdateLastKnownPositionOfCloseby(lastKnownPosition, 15);

		}
	}

	public void UpdateLastKnowPosition(Vector3 newPosition)
	{
		if (currentTarget == null)
		{
			lastKnownPosition = newPosition;

			if (!isAgressive || Time.realtimeSinceStartup - lastCautionPlayed > 4)
			{
				lastCautionPlayed = Time.realtimeSinceStartup;
				//cautionTimer = cautionTimerNormal;

				SetToCautiousState();
				//isCaution = true;
				//alarmTimer = 25;
				//isAgressive = true;
				//animator.SetBool("isCaution", true);
				//animator.CrossFade("caution", 0.2f);
			}
		}
	}

	void HandleDetection()
	{
		Collider[] colliders = Physics.OverlapSphere(mTransform.position, fovRadius, controllerLayer);

		for (int i = 0; i < colliders.Length; i++)
		{
			IPointOfInterest poi = colliders[i].transform.GetComponentInParent<IPointOfInterest>();
			if (poi != null)
			{
				if (poi.GetTransform() != poiTransform)
				{
					if (RaycastToTarget(poi))
					{
						break;
					}
				}
			}
		}
	}

	public void OnHit()
	{

	}

	public string hitFx = "blood";

	public string GetHitFx()
	{
		return hitFx;
	}

	public void StartGrab(Vector3 tp, Quaternion targetRotation)
	{
		agent.enabled = false;
		mTransform.position = tp;
		isGrab = true;
		animator.Play("e_grab_start");
		mTransform.rotation = targetRotation;

		emotionText.text = "?!";
		emotionObj.SetActive(true);

		GameReferences.UpdateLastKnownPositionOfCloseby(mTransform.position, 2);
	}

	public void KillByGrab()
	{
		animator.Play("grab_death");
		this.enabled = false;
		isDead = true;
	}

	public void StopGrab(Controller target)
	{
		currentTarget = target;
		lastKnownPosition = currentTarget.mTransform.position;
		agent.enabled = true;
		agent.updateRotation = true;
		isGrab = false;
		animator.Play("e_grab_cancel");
		PlayCautionState(cautionTimerNormal, Time.deltaTime, false);
	}

	public Transform poiTransform;

	public bool OnDetect(AIController aIController)
	{
		if (this.isDead)
		{
			if (!isSpottedDead)
			{
				aIController.emotionText.text = "?";
				aIController.emotionObj.SetActive(true);
				aIController.UpdateLastKnowPosition(mTransform.position);
				isSpottedDead = true;
			}
			return true;
		}
		else
		{
			return false;
		}
	}

	public Transform GetTransform()
	{
		return poiTransform;
	}

	public void OnHit(float dmgAmt)
	{
		if (currentTarget != null) // Check if currentTarget is not null
		{
			currentHealth -= currentTarget.dmgNumber;
            //sound here
            hitSoundSource.clip = hitSoundClips[index];
            hitSoundSource.Play();
            Debug.Log($"Enemy hit for {currentTarget.dmgNumber} HP. Current health: {currentHealth}");
			UpdateLastKnowPosition(transform.position);
			currentHealth = Mathf.Max(currentHealth, 0);
		}
		else
		{
			Debug.LogError("Current target is null in AIController.");
		}
	}
}



[System.Serializable]
public class Waypoint
{

	public Transform targetPosition;
	public Vector3 lookEulers;
	public float waitTime;
}
