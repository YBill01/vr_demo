using System;
using UnityEngine;
using static Chofu.RestApi.Url;

public class AvatarInputKeyboard : MonoBehaviour, IAvatarInput
{
	public bool _inputEnabled;

	public bool _changeViewMode;

	public Vector2 _lookVelocity = Vector2.zero;
	public Vector3 _movementDirection = Vector3.zero;

	public bool _isSprint = false;
	public bool _isJump = false;
	public bool _isAction = false;

	public bool _isGesture = false;
	public int _isGestureIndex = -1;

	public int _isEmotionIndex = -1;

	public int _isEmojiIndex = -1;

	public bool inputEnabled { get { return _inputEnabled; } set { _inputEnabled = value; } }

	public bool changeViewMode => _changeViewMode;

	public Vector2 lookVelocity => _lookVelocity;

	public Vector3 movementDirection => _movementDirection;

	public bool isSprint => _isSprint;

	public bool isJump => _isJump;

	public bool isAction => _isAction;

	public bool isGesture => _isGesture;

	public int gestureIndex => _isGestureIndex;

	public int emotionIndex => _isEmotionIndex;

	public int emojiIndex => _isEmojiIndex;


	private void Start()
	{
		_inputEnabled = true;



	}

	private void Update()
	{
		if (!_inputEnabled)
		{
			return;
		}

		ProcessChangeViewMode();
		ProcessLook();
		ProcessMoving();
		ProcessSprint();
		ProcessJump();
		ProcessAction();
		ProcessGesture();
		ProcessEmotion();
		ProcessEmoji();



	}

	

	private void ProcessChangeViewMode()
	{
		ProcessChangeViewMode(Input.GetKeyDown(KeyCode.Q));
	}

	private void ProcessLook()
	{
		ProcessLook(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
	}
	private void ProcessMoving()
	{
		_movementDirection = Vector3.zero;

		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			_movementDirection.x -= 1;
		}
		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			_movementDirection.x += 1;
		}
		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			_movementDirection.z += 1;
		}
		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			_movementDirection.z -= 1;
		}

		ProcessMoving(_movementDirection.normalized);
	}
	private void ProcessSprint()
	{
		_isSprint = Input.GetKey(KeyCode.LeftShift);
	}
	private void ProcessJump()
	{
		ProcessJump(Input.GetKey(KeyCode.Space));
	}
	private void ProcessAction()
	{
		_isAction = Input.GetKey(KeyCode.E);
	}

	private void ProcessGesture()
	{
		_isGesture = false;
		_isGestureIndex = -1;

		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			_isGesture = true;
			_isGestureIndex = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			_isGesture = true;
			_isGestureIndex = 1;
		}



	}
	private void ProcessEmotion()
	{
		_isEmotionIndex = -1;

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			_isEmotionIndex = 0;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			_isEmotionIndex = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			_isEmotionIndex = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			_isEmotionIndex = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			_isEmotionIndex = 4;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha6))
		{
			_isEmotionIndex = 5;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha7))
		{
			_isEmotionIndex = 6;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha8))
		{
			_isEmotionIndex = 7;
		}
	}

	private void ProcessEmoji()
	{
		



	}

	public void ProcessBack()
	{
		
	}

	public void ProcessLook(Vector2 velocity)
	{
		_lookVelocity = velocity;
	}

	public void ProcessMoving(Vector3 direction)
	{
		_movementDirection = direction;
	}

	public void ProcessChangeViewMode(bool value)
	{
		_changeViewMode = value;
	}

	public void ProcessJump(bool value)
	{
		_isJump = value;
	}
}