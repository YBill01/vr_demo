using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
public class PlayerPCController : MonoBehaviour
{
	//public bool cameraCanMove = true;
	public float mouseSensitivity = 2f;
	public float minLookAngle = -80f;
	public float maxLookAngle = 80f;
	public Camera playerCamera;

	[SerializeField]
	private float playerSpeed = 2.0f;
	[SerializeField]
	private float moveSprintMultiplier = 2.0f;
	/*[SerializeField]
	private float rotationSpeed = 2.0f;*/
	[SerializeField]
	private float jumpHeight = 1.0f;
	[SerializeField]
	private Vector3 gravity = Physics.gravity;
	[SerializeField]
	private Transform lookAtTarget;

	[SerializeField] protected CharacterController _characterController;
	private PlayerPCInput _playerPCInput;




	private Vector3 playerVelocity;
	private bool groundedPlayer;

	private Vector2 _currentLookEulerAngles;
	private Vector3 _moveVelocity;

	private bool _isLooking = false;
	private bool _isWalking = false;
	private bool _isRunning = false;
	private bool _isGrounded = false;

	// UNITY EVENTS
	private void Awake()
	{
		//_characterController = GetComponent<CharacterController>();
		_playerPCInput = GetComponent<PlayerPCInput>();
	}
	private void Start()
	{
		_currentLookEulerAngles = transform.rotation.eulerAngles;
	}

	private void Update()
	{
		ProcessLook();
		ProcessGravity();
		ProcessWalking();
		ProcessJump();
	}

	private void ProcessLook()
	{
		_currentLookEulerAngles += _playerPCInput.lookVelocity * mouseSensitivity;
		_currentLookEulerAngles = new Vector3(_currentLookEulerAngles.x, Mathf.Clamp(_currentLookEulerAngles.y, minLookAngle, maxLookAngle), 0);

		//transform.localRotation = Quaternion.Euler(0, _currentLookEulerAngles.x, 0);
		lookAtTarget.localRotation = Quaternion.Euler(-_currentLookEulerAngles.y, 0, 0);
		_characterController.transform.localRotation = Quaternion.Euler(0, _currentLookEulerAngles.x, 0);
	}
	private void ProcessGravity()
	{
		_isGrounded = _characterController.isGrounded;
		if (_isGrounded && _moveVelocity.y < 0.0f)
		{
			_moveVelocity.y = 0.0f;
		}

		
	}
	private void ProcessWalking()
	{
		Vector3 movement = (_playerPCInput.movementDirection.z * transform.forward) + (_playerPCInput.movementDirection.x * transform.right);
		_characterController.Move(movement * Time.deltaTime * playerSpeed * (_playerPCInput.isSprint ? moveSprintMultiplier : 1.0f));


	}
	private void ProcessJump()
	{
		if (_playerPCInput.isJump && _isGrounded)
		{
			_moveVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity.y);
		}

		_moveVelocity.y += gravity.y * Time.deltaTime;
		_characterController.Move(_moveVelocity * Time.deltaTime);
	}



}