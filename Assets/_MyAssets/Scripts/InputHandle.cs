using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public class InputHandler : MonoBehaviour
	{
		public Transform camHolder;

		public ExecutionOrder movementOrder;
		public Controller controller;

		Vector3 moveDirection;

		public enum ExecutionOrder
		{
			fixedUpdate, update, lateUpdate
		}

		private void FixedUpdate()
		{
			if (movementOrder == ExecutionOrder.fixedUpdate)
			{
				controller.Move(moveDirection, Time.fixedDeltaTime);
			}
		}

		private void Update()
		{
			float horizontal = Input.GetAxis("Horizontal");
			float vertical = Input.GetAxis("Vertical");
			float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

			moveDirection = camHolder.forward * vertical;
			moveDirection += camHolder.right * horizontal;
			moveDirection.Normalize();

			float delta = Time.deltaTime;

			if (movementOrder == ExecutionOrder.update)
			{
				controller.Move(moveDirection, delta);
			}

			controller.HandleMovementAnimations(moveAmount, delta);

		}
	}
}

