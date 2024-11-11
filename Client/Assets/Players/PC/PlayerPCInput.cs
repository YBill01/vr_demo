using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPCInput : MonoBehaviour
{
	public Vector2 lookVelocity = Vector2.zero;
	public Vector3 movementDirection = Vector3.zero;

	public bool isSprint = false;
	public bool isJump = false;
	public bool isAction = false;


	private void FixedUpdate()
	{
		ProcessLook();
		ProcessMoving();
		ProcessSprint();
		ProcessJump();
		ProcessAction();
	}
	
	private void ProcessLook()
	{
		lookVelocity.x = Input.GetAxis("Mouse X");
		lookVelocity.y = Input.GetAxis("Mouse Y");

		//Debug.Log($"look delta: {lookVelocity}");
	}
	private void ProcessMoving()
	{
		/*float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		float magnitude = Mathf.Sqrt();*/

		movementDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		movementDirection = movementDirection.normalized;

		//Debug.Log($"movement direction: {movementDirection}");
	}
	private void ProcessSprint()
	{
		isSprint = Input.GetKey(KeyCode.LeftShift);
		//isSprint = Input.GetAxis("Sprint") > 0.0f;

		//Debug.Log($"sprint : {isSprint}");
	}
	private void ProcessJump()
	{
		isJump = Input.GetKey(KeyCode.Space);
		//isJump = Input.GetAxis("Jump") > 0.0f;

		//Debug.Log($"jump : {isJump}");
	}
	private void ProcessAction()
	{
		isAction = Input.GetKey(KeyCode.E);
		//isAction = Input.GetAxis("Action") > 0.0f;

		//Debug.Log($"action : {isAction}");
	}

}