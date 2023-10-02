using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class Controller : MonoBehaviour
	{
		new Rigidbody rigidbody;
		public float moveSpeed = .4f;
		public float rotateSpeed = .2f;
		Transform mTransform;
		Animator animator;

		private void Start()
		{
			mTransform = this.transform;
			rigidbody = GetComponent<Rigidbody>();
			animator = GetComponentInChildren<Animator>();
		}

		public void Move(Vector3 moveDirection, float delta)
		{
			rigidbody.velocity = moveDirection * moveSpeed;


			Vector3 lookDir = moveDirection;
			if (lookDir == Vector3.zero)
				lookDir = mTransform.forward;
			Quaternion lookRotation = Quaternion.LookRotation(lookDir);
			mTransform.rotation = Quaternion.Slerp(mTransform.rotation, lookRotation, delta / rotateSpeed);
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

	}
}
