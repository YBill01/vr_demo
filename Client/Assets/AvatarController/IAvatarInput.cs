using UnityEngine;

public interface IAvatarInput
{
	bool inputEnabled { get; set; }
	bool changeViewMode { get; }

	Vector2 lookVelocity { get; }
	Vector3 movementDirection { get; }

	bool isSprint { get; }
	bool isJump { get; }
	bool isAction { get; }

	bool isGesture { get; }
	int gestureIndex { get; }

	int emotionIndex { get; }

	int emojiIndex { get; }


	void ProcessBack();
	void ProcessLook(Vector2 velocity);
	void ProcessMoving(Vector3 direction);
	void ProcessChangeViewMode(bool value);
	void ProcessJump(bool value);





}