using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Avatar;
using static AvatarModel;
using static UnityEditorInternal.ReorderableList;

[RequireComponent(typeof(CharacterController), typeof(Avatar))]
public class AvatarController : MonoBehaviour
{
	public bool Initialized { get; private set; }

	public ViewMode _viewMode = ViewMode.FirstPerson;
	public ViewMode viewMode {
		get =>_viewMode;
		set
		{
			ChangeViewMode(value);
		}
	}
	public float thirdPersonCameraDistance = 4f;

	public float mouseSensitivity = 2f;
	public float minLookAngle = -80f;
	public float maxLookAngle = 80f;

	[SerializeField]
	private float playerSpeed = 2.0f;
	[SerializeField]
	private float moveSprintMultiplier = 2.0f;

	[SerializeField]
	private float jumpHeight = 1.0f;
	[SerializeField]
	private Vector3 gravity = Physics.gravity;
	[SerializeField]
	private Transform lookAtTarget;
	[SerializeField]
	private Camera targetCamera;
	[SerializeField]
	private GameObject model;

	private CharacterController _characterController;
	private Avatar _avatar;
	private IAvatarInput _avatarInput;

	private Vector2 _currentLookEulerAngles;
	private Vector3 _moveVelocity;

	private bool _isLooking = false;
	private bool _isWalking = false;
	private bool _isRunning = false;
	private bool _isJumping = false;
	private bool _isGrounded = false;
	private bool _isGesture = false;

	private bool _isChangingViewMode = false;
	private Vector3 _isFirstPersonCameraPosition;





	// UNITY EVENTS
	private void Awake()
	{
		_characterController = GetComponent<CharacterController>();
		_avatar = GetComponent<Avatar>();
		
	}
	private void Start()
	{
		_currentLookEulerAngles = transform.rotation.eulerAngles;

		_isFirstPersonCameraPosition = targetCamera.transform.localPosition;
	}
	private void Update()
	{
		if (Initialized)
		{
			ProcessChangeViewMode();

			ProcessLook();
			ProcessGravity();
			ProcessWalking();
			ProcessJump();

			ProcessGesture();
			ProcessEmotion();
		}


		ResetStates();
	}

	

	private void ResetStates()
	{
		
	}

	private void LateUpdate()
	{
		if (Initialized && model.activeSelf)
		{
			ProcessCurrentAnimation();
		}

		if (viewMode == ViewMode.ThirdPerson)
		{
			Vector3 currentPos = new Vector3(_isFirstPersonCameraPosition.x, _isFirstPersonCameraPosition.y, _isFirstPersonCameraPosition.z - thirdPersonCameraDistance);
			RaycastHit hit;
			Vector3 dirTmp = lookAtTarget.TransformPoint(currentPos) - lookAtTarget.position;
			if (Physics.SphereCast(lookAtTarget.position, 0.3f, dirTmp, out hit, thirdPersonCameraDistance))
			{
				currentPos = new Vector3(_isFirstPersonCameraPosition.x, _isFirstPersonCameraPosition.y, _isFirstPersonCameraPosition.z - hit.distance);

				targetCamera.transform.localPosition = currentPos;

				if (hit.distance < 2.0f)
				{
					if (model.activeSelf)
					{
						model.SetActive(false);
					}
				}
				else if (!model.activeSelf)
				{
					model.SetActive(true);
				}
			}
			else
			{
				targetCamera.transform.localPosition = Vector3.Lerp(targetCamera.transform.localPosition, currentPos, Time.deltaTime * 15.0f);
			}
		}

	}

	

	public void Init()
	{
		Initialized = true;

		_avatarInput = GetComponent<IAvatarInput>();

		//ChangeViewMode(_viewMode);

	}


	private void ProcessChangeViewMode()
	{
		if (_avatarInput.changeViewMode)
		{
			switch (_viewMode)
			{
				case ViewMode.FirstPerson:
					ChangeViewMode(ViewMode.ThirdPerson);
					break;
				case ViewMode.ThirdPerson:
					ChangeViewMode(ViewMode.FirstPerson);
					break;
				default:
					break;
			}
		}
	}


	private void ProcessLook()
	{
		_isLooking = _avatarInput.lookVelocity != Vector2.zero;

		_currentLookEulerAngles += _avatarInput.lookVelocity * mouseSensitivity;
		_currentLookEulerAngles = new Vector3(_currentLookEulerAngles.x, Mathf.Clamp(_currentLookEulerAngles.y, minLookAngle, maxLookAngle), 0);

		lookAtTarget.localRotation = Quaternion.Euler(-_currentLookEulerAngles.y, 0, 0);
		if (_viewMode == ViewMode.FirstPerson)
		{
			_characterController.transform.localRotation = Quaternion.Euler(0, _currentLookEulerAngles.x, 0);
		}
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
		if (_isGesture)
		{
			return;
		}

		_isWalking = _avatarInput.movementDirection != Vector3.zero;
		_isRunning = _isWalking ? _avatarInput.isSprint : false;
		_isWalking = _isRunning ? false : _isWalking;

		Vector3 movement = (_avatarInput.movementDirection.z * transform.forward) + (_avatarInput.movementDirection.x * transform.right);
		float moveSpeed = playerSpeed;
		if (_avatarInput.isSprint)
		{
			moveSpeed *= moveSprintMultiplier;
		}
		_characterController.Move(movement * (Time.deltaTime * moveSpeed));
	}
	private void ProcessJump()
	{
		if (_isGesture)
		{
			return;
		}

		if (_avatarInput.isJump && _isGrounded)
		{
			_isJumping = true;
			_moveVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravity.y);
		}
		else
		{
			if (_isGrounded)
			{
				_isJumping = false;
			}
		}

		_moveVelocity.y += gravity.y * Time.deltaTime;
		_characterController.Move(_moveVelocity * Time.deltaTime);
	}






	private void ChangeViewMode(ViewMode newViewMode)
	{
		if (_isChangingViewMode || _viewMode == newViewMode)
		{
			return;
		}

		_viewMode = newViewMode;
		_isChangingViewMode = true;

		switch (_viewMode)
		{
			case ViewMode.FirstPerson:
				model.SetActive(false);
				targetCamera.transform.DOLocalMoveZ(_isFirstPersonCameraPosition.z, 0.15f).OnComplete(() => { _isChangingViewMode = false; });
				break;
			case ViewMode.ThirdPerson:
				model.SetActive(true);
				targetCamera.transform.DOLocalMoveZ(-thirdPersonCameraDistance, 0.2f).OnComplete(() => { _isChangingViewMode = false; });
				break;
			default:
				break;
		}

		


	}

	private void ProcessGesture()
	{
		if (_isGesture)
		{
			return;
		}

		if (_avatarInput.isGesture && _isGrounded)
		{
			_isGesture = true;


		}

	}

	private void ProcessEmotion()
	{
		if (_avatarInput.emotionIndex != -1)
		{
			_avatar.ChangeEmotion((Avatar.EmotionType)_avatarInput.emotionIndex, true);
		}
	}


	// ANIMATIONS
	private void ProcessRessetAnimation(AnimatorStateInfo stateInfo)
	{
		if (stateInfo.IsName("Idle"))
		{
			_avatar.animator.ResetTrigger("Idle");
		}
		else if (stateInfo.IsName("Walk"))
		{
			_avatar.animator.ResetTrigger("Walk");
		}
		else if (stateInfo.IsName("Run"))
		{
			_avatar.animator.ResetTrigger("Run");
		}
		else if (stateInfo.IsName("Jump"))
		{
			_avatar.animator.ResetTrigger("Jump");
		}


	}
	private void ProcessCurrentAnimation()
	{
		if (_viewMode == ViewMode.FirstPerson)
		{
			return;
		}

		AnimatorStateInfo stateInfo = _avatar.animator.GetCurrentAnimatorStateInfo(0);

		if (!Initialized)
		{
			if (stateInfo.IsName("Idle") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Idle);
			}

			return;
		}

		if (_isGesture && _isGrounded)
		{
			if (stateInfo.IsName("Hello") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Hello);
			}
			else if (stateInfo.IsName("Dance") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Dance);
			}
			else
			{
				_isGesture = false;
			}

			return;
		}

		if (_isWalking && _isGrounded)
		{
			if (stateInfo.IsName("Walk") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Walk);
			}
		}
		else if (_isRunning && _isGrounded)
		{
			if (stateInfo.IsName("Run") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				//_avatar.PlayAnimation(AnimationType.Run);
			}

		}
		else if (_isJumping && !_isGrounded)
		{
			if (stateInfo.IsName("Jump") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Jump);
			}

		}
		else
		{
			if (stateInfo.IsName("Idle") == false)
			{
				//ProcessRessetAnimation(stateInfo);
				_avatar.PlayAnimation(AnimationType.Idle);
			}


		}

		

		/*if (_isGrounded)
		{
			if (_isWalking)
			{
				if (_isRunning)
				{
					if (!stateInfo.IsName("Run"))
					{
						_avatar.PlayAnimation(AnimationType.Run);
					}
				}
				else
				{
					if (!stateInfo.IsName("Walk"))
					{
						_avatar.PlayAnimation(AnimationType.Walk);
					}
				}
			}
			else
			{
				if (!stateInfo.IsName("Idle"))
				{
					_avatar.animator.SetTrigger("Idle");
					//_avatar.PlayAnimation(AnimationType.Idle);
				}
			}




		}
		else
		{
			if (!stateInfo.IsName("Jump"))
			{
				//_avatar.animator.SetTrigger("Jump");
				_avatar.PlayAnimation(AnimationType.Jump);
			}



		}*/


	}



	public enum ViewMode : byte
	{
		FirstPerson,
		ThirdPerson
	}
}