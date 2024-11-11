using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInputTouch : MonoBehaviour, IAvatarInput
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

	private void LateUpdate()
	{
		_changeViewMode = false;
		_isJump = false;


		//_lookVelocity = Vector2.zero;
		//_movementDirection = Vector3.zero;
	}

	public void ProcessChangeViewMode(bool value)
	{
		_changeViewMode = value;
	}

	public void ProcessLook(Vector2 velocity)
	{
		_lookVelocity = velocity;
	}
	public void ProcessMoving(Vector3 velocity)
	{
		_movementDirection = velocity;
	}

	public void ProcessBack()
	{
		
	}

	public void ProcessJump(bool value)
	{
		_isJump = value;
	}
}