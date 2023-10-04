using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;


namespace Enemy
{
    public class AIController : MonoBehaviour
    {
        NavMeshAgent agent;
        new Rigidbody rBody;
        Animator animator;

        public int index;
        public float waitTimer;
        public Waypoint[] waypoints;
        Waypoint currentWaypoint;
        Transform mTransform;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            rBody = GetComponent<Rigidbody>();
            currentWaypoint = waypoints[index];
            mTransform = this.transform;

        }
        private void Update()
        {
            float distance = Vector3.Distance(mTransform.position, currentWaypoint.targetPos.position);

            if(distance < agent.stoppingDistance)
            {
                if (agent.hasPath == false)
                {
                    agent.SetDestination(currentWaypoint.targetPos.position);
                }
                else
                {
                    if (waitTimer < currentWaypoint.waitTime);
                }
            }
        }
    }

    [System.Serializable]
    public class Waypoint
    {
        public Transform targetPos;
        public Vector3 lookRot;
        public float waitTime;
    }
}

