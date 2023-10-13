using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIController : MonoBehaviour, IShootable
{
	NavMeshAgent agent;
	new Rigidbody rigidbody;
	Animator animator;
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
	public bool isAgressive;
	public bool isCaution;
	public float cautionTimerNormal = .7f;
	public float cautionTimerAgrro = .4f;
	float cautionTimer;

	[Header("Wait Timer")]
	[Space(5)]
	float waitTimer;

	[Header("Attributes")]
	[Space(5)]
	public float normalSpeed = 2;
	public float aggressiveSpeed = 4;
	public float rotateSpeed = .5f;
	public float fovRadius = 20;
	public float fovAngle = 45;

	[Header("Attack Attributes")]
	[Space(5)]
	public float weaponSpread = .3f;
	[SerializeField]
	private float damageAmount;
	public int magBullets = 30;
	int timesShot;

	public float DamageAmount
	{
		get { return damageAmount; }
		set { damageAmount = value; }
	}

	public float attackDistance = 5;
	Vector3 lastKnownPosition;

	Controller currentTarget;

	LayerMask controllerLayer;

	private void Start()
	{
		agent = GetComponentInChildren<NavMeshAgent>();
		rigidbody = GetComponentInChildren<Rigidbody>();
		animator = GetComponentInChildren<Animator>();
		currentWaypoint = waypoints[index];
		mTransform = this.transform;
		currentHealth = maxHealth;
		controllerLayer = (1 << 6);
		animator.applyRootMotion = false;

		GameReferences.damage = damageAmount;

	}

	private void Update()
	{
		float delta = Time.deltaTime;
		if(currentHealth <= 0 )
        {
			Debug.Log("Enemy Dead");
        }



		if (!isAgressive)
		{
			agent.speed = normalSpeed;
			HandleDetection();
			HandleNormalLogic(delta);
		}
		else
		{
			if(isCaution)
            {
				if(cautionTimer < 0)
                {
					isCaution = false;
					animator.SetBool("isCaution", false);
					agent.isStopped = false;
				}
				else
                {
					HandleLookAtTarget(delta);
					agent.isStopped = true;
					cautionTimer -= delta;
                }
            }
            {
				agent.speed = aggressiveSpeed;
				HandleAggressiveLogic(delta);
			}
			
		}
	}

	public float fovHeight = 1.0f;

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, fovRadius);


		Vector3 startLinePosition = transform.position + Vector3.up * fovHeight;


		Vector3 fovLine1 = Quaternion.AngleAxis(-fovAngle * 0.5f, Vector3.up) * transform.forward * fovRadius;
		Vector3 fovLine2 = Quaternion.AngleAxis(fovAngle * 0.5f, Vector3.up) * transform.forward * fovRadius;

		Gizmos.color = Color.red;
		Gizmos.DrawLine(startLinePosition, startLinePosition + fovLine1);
		Gizmos.DrawLine(startLinePosition, startLinePosition + fovLine2);
	}
	void HandleNormalLogic(float delta)
	{
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

	void HandleAggressiveLogic(float delta)
	{
		if (currentTarget != null)
		{
			if (!RaycastToTarget(currentTarget))
			{
				currentTarget = null;
			}
		}

		float dis = Vector3.Distance(lastKnownPosition, mTransform.position);
		agent.SetDestination(lastKnownPosition);
		if (currentTarget != null)
		{
			if (dis < attackDistance)
			{
				agent.isStopped = true;

				HandleLookAtTarget(delta);

				if (currentFire < 0)
				{
					currentFire = fireRate;
					HandleShooting();
				}
				else
				{
					currentFire -= delta;
				}

			}
			else
			{
				agent.updateRotation = true;
				agent.isStopped = false;
				HandleDetection();
			}
		}
		else
		{
			agent.updateRotation = true;
			agent.isStopped = false;
			HandleDetection();
		}

		if (agent.desiredVelocity.magnitude > 0)
		{
			animator.SetFloat("movement", 1, 0.1f, delta);
		}
		else
		{
			animator.SetFloat("movement", 0, 0.1f, delta);
		}

	}

	void HandleLookAtTarget(float delta)
    {
		Vector3 dir = currentTarget.mTransform.position - mTransform.position;
		dir.y = 0;
		Quaternion targetRot = Quaternion.LookRotation(dir);
		mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);
		agent.updateRotation = false;
	}
	public ParticleSystem muzzleFire;
	
	void HandleShooting()
	{
		timesShot++;
		if (timesShot > magBullets)
		{
			timesShot = 0;
		}
		muzzleFire.Play();
		RaycastHit hit;

        if (Physics.Raycast(mTransform.position, mTransform.forward, out hit, attackDistance))
        {
            Controller targetController = hit.transform.GetComponentInParent<Controller>();
            if (targetController != null)
            {
				targetController.OnHit(damageAmount);
            }
        }

        GameReferences.RaycastShoot(mTransform, weaponSpread);
		
	}


	bool RaycastToTarget(Controller c)
	{
		Vector3 dir = c.mTransform.position - mTransform.position;
		dir.Normalize();
		float angle = Vector3.Angle(mTransform.forward, dir);
		if (angle < fovAngle)
		{
			Vector3 o = mTransform.position;
			o.y += 1;

			Debug.DrawRay(o, dir * 50, Color.red);
			if (Physics.Raycast(o, dir, out RaycastHit hit, 100))
			{
				Controller targetController = hit.transform.GetComponentInParent<Controller>();
				if (targetController != null)
				{
					if(!isAgressive || currentTarget == null)
                    {
						cautionTimer = cautionTimerNormal;
						isCaution = true;
						isAgressive = true;
						animator.SetBool("isCaution", true);
						animator.CrossFade("cautionAnim", .2f);
					}
					currentTarget = targetController;


					animator.SetBool("isAggressive", true);
					lastKnownPosition = currentTarget.transform.position;
					return true;
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

	void HandleDetection()
	{
		Collider[] colliders = Physics.OverlapSphere(mTransform.position, fovRadius, controllerLayer);

		for (int i = 0; i < colliders.Length; i++)
		{
			Controller c = colliders[i].transform.GetComponentInParent<Controller>();
			if (c != null)
			{
				if (RaycastToTarget(c))
				{
					break;
				}
			}
		}
	}

	public void OnHit(float dmgAmt)
	{
		//damageAmount = dmgAmt;
		//currentHealth -= damage;
	
		// Ensure health doesn't go below zero
		currentHealth = Mathf.Max(currentHealth, 0);

		// Handle other consequences of being hit
	}

	public string hitFx = "blood";

	public string GetHitFx()
	{
		return hitFx;
	}
}

[System.Serializable]
public class Waypoint
{

	public Transform targetPosition;
	public Vector3 lookEulers;
	public float waitTime;
}


