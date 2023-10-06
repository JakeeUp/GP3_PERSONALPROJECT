using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace Player
{
	public class AIController : MonoBehaviour
	{
		NavMeshAgent agent;
		new Rigidbody rigidbody;
		Animator animator;

		public int index;
		public Waypoint[] waypoints;
		Waypoint currentWaypoint;
		Transform mTransform;

		public bool isAgressive;

		float waitTimer;

		public float normalSpeed = 2;
		public float aggressiveSpeed = 4;
		public float rotateSpeed = .5f;
		public float fovRadius = 20;
		public float fovAngle = 45;
		public float yOffset = 1.0f;  // Adjust this value to raise or lower the Gizmos
		public float attackDistance = 5;
		Vector3 lastKnownPosition;

		Controller currentTarget;

		LayerMask controllerLayer;


		private void Start()
		{
			agent = GetComponent<NavMeshAgent>();
			rigidbody = GetComponent<Rigidbody>();
			animator = GetComponentInChildren<Animator>();
			currentWaypoint = waypoints[index];
			mTransform = this.transform;

			controllerLayer = (1 << 6);
		}

		private void Update()
		{
			float delta = Time.deltaTime;

			if (!isAgressive)
			{
				agent.speed = normalSpeed;
				HandleDetection();
				HandleNormalLogic(delta);
			}
			else
			{
				agent.speed = aggressiveSpeed;
				HandleAggressiveLogic(delta);
			}

			
		}

		void HandleNormalLogic(float delta)
		{
			currentWaypoint = waypoints[index];

			float dis = Vector3.Distance(mTransform.position, currentWaypoint.targetPosition.position);

			if (dis > agent.stoppingDistance)
			{
				animator.SetFloat("movement", 1, 0.1f, delta);
				agent.updateRotation = true;

				if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
				{
					agent.SetDestination(currentWaypoint.targetPosition.position);
				}
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
					index = (index + 1) % waypoints.Length;  // Use modulo to wrap around the index

					// Explicitly set the destination to the next waypoint
					agent.SetDestination(waypoints[index].targetPosition.position);

					Debug.Log($"Switching to waypoint index: {index}, position: {waypoints[index].targetPosition.position}");
				}
			}
		}




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

					Vector3 dir = currentTarget.mTransform.position - mTransform.position;
					dir.y = 0;
					Quaternion targetRot = Quaternion.LookRotation(dir);
					mTransform.rotation = Quaternion.Slerp(mTransform.rotation, targetRot, delta / rotateSpeed);
					agent.updateRotation = false;

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
					Controller targetController = hit.transform.GetComponent<Controller>();
					if (targetController != null)
					{
						currentTarget = targetController;
						isAgressive = true;
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
		private void OnDrawGizmos()
		{
			if (transform == null)
			{
				return;
			}

			

			Vector3 gizmoPosition = transform.position + Vector3.up * yOffset;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(gizmoPosition, fovRadius);  // Draw FOV radius

			Vector3 fovLine1 = Quaternion.AngleAxis(-fovAngle / 2, Vector3.up) * transform.forward * fovRadius;
			Vector3 fovLine2 = Quaternion.AngleAxis(fovAngle / 2, Vector3.up) * transform.forward * fovRadius;

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(gizmoPosition, gizmoPosition + fovLine1);  // Draw one side of FOV
			Gizmos.DrawLine(gizmoPosition, gizmoPosition + fovLine2);  // Draw other side of FOV
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
	}

	[System.Serializable]
	public class Waypoint
	{

		public Transform targetPosition;
		public Vector3 lookEulers;
		public float waitTime;
	}
}
