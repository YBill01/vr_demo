using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarControl : MonoBehaviour
{

    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;
    public Camera playerCamera;

    public float jumpPower = 5f;

    

    public float walkSpeed = 5f;

    public float maxVelocityChange = 10f;

    public AvatarModel avatar;

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float moveSprintMultiplier = 2.0f;
    [SerializeField]
    private float rotationSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private Vector3 gravity = Physics.gravity;
    [SerializeField]
    private Transform lookAtTarget;

    [SerializeField]
    private Rigidbody rb;

    protected CharacterController _characterController;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private Vector3 move;
    private Vector3 look;
    private bool sprint;
    private bool jump;
    private bool action;

    private Vector3 currentEulerAngles;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private bool isWalking = false;
    private bool isGrounded = false;



    // UNITY EVENTS
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();

        currentEulerAngles = transform.rotation.eulerAngles;
    }
    private void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        InputPlayerControlMediator.Movement += OnMovement;
        InputPlayerControlMediator.Look += OnLook;
        InputPlayerControlMediator.Sprint += OnSprint;
        InputPlayerControlMediator.Jump += OnJump;
        InputPlayerControlMediator.Action += OnAction;
#endif
    }
    private void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        InputPlayerControlMediator.Movement -= OnMovement;
        InputPlayerControlMediator.Look -= OnLook;
        InputPlayerControlMediator.Jump -= OnJump;
        InputPlayerControlMediator.Action -= OnAction;
#endif
    }

    // HANDLERS
    private void OnMovement(Vector2 value)
    {
        move = new Vector3(value.x, 0, value.y);
    }
    private void OnLook(Vector2 value)
    {
        look = new Vector3(value.x, value.y, 0);
        //Look(value);
    }
    private void OnSprint(bool value)
    {
        sprint = value;
    }
    private void OnJump(bool value)
    {
        jump = value;
    }
    private void OnAction(bool value)
    {
        action = value;
    }





    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        CheckGround();

        /*currentEulerAngles += look * Time.deltaTime * rotationSpeed;
        currentEulerAngles = new Vector3(currentEulerAngles.x, Mathf.Clamp(currentEulerAngles.y, -80f, 80f), 0);

        transform.localRotation = Quaternion.Euler(0, currentEulerAngles.x, 0);
        lookAtTarget.localRotation = Quaternion.Euler(-currentEulerAngles.y, 0, 0);


        groundedPlayer = _characterController.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 movement = (move.z * transform.forward) + (move.x * transform.right);
        _characterController.Move(movement * Time.deltaTime * playerSpeed * (sprint ? moveSprintMultiplier : 1.0f));

        if (move != Vector3.zero)
        {
            //gameObject.transform.forward = move;
        }

        if (jump && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity.y);
        }



        playerVelocity.y += gravity.y * Time.deltaTime;*/
        //_characterController.Move(playerVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        
    }

    private void CheckGround()
    {
        //Ray ray = new Ray(transform.position + Vector3.up * 0.01f, Vector3.down);
		//Physics.Raycast(ray, 0.1f);

		/*Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
		Vector3 direction = transform.TransformDirection(Vector3.down);
		float distance = .75f;*/

		/*if (Physics.Raycast(ray, 0.5f))
        {
            //Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
            isGrounded = false;
        }*/

        Vector3 origin = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .25f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = false;
        }


        /*Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }*/
    }

    private void FixedUpdate()
	{
        if (cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }


        Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed;

        if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }



        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        


    }

	private void LateUpdate()
	{
        ProcessCurrentAnimation();
    }

    private void ProcessCurrentAnimation()
	{
        AnimatorStateInfo info = avatar.animator.GetCurrentAnimatorStateInfo(0);

        if (isWalking && isGrounded)
		{
			if (info.IsName("Walk") == false)
			{
                avatar.PlayAnimation(AvatarModel.AnimType.Walk);
            }
            
		}
		else if (!isGrounded)
		{
            if (info.IsName("Jump") == false)
            {
                avatar.PlayAnimation(AvatarModel.AnimType.Jump);
            }
            
        }
		else
		{
            if (info.IsName("Idle") == false)
            {
                avatar.PlayAnimation(AvatarModel.AnimType.Idle);
            }
            

        }
    }

    /*private void MouseMove(Vector2 _input)
    {
        transform.Rotate(new Vector3(0, _input.x, 0));
        m_cameraRotation.x = Mathf.Clamp(m_cameraRotation.x + -_input.y, -90, 90);
        m_cameraRotation = new Vector3(m_cameraRotation.x, transform.eulerAngles.y, transform.eulerAngles.z);
        m_camera.transform.eulerAngles = m_cameraRotation;
    }*/

}