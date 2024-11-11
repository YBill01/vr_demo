using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInput : MonoBehaviour
{
	public static AvatarInput Instance;

	public IAvatarInput avatarInput;

	public Action<AvatarInputType> OnInit;

	public AvatarInputType inputType = AvatarInputType.Keyboard;

	private void Awake()
	{
		AvatarInput.Instance = this;

		Init();
	}

	private void Init()
	{
		switch (inputType)
		{
			case AvatarInputType.Keyboard:
				avatarInput = gameObject.AddComponent<AvatarInputKeyboard>();

				break;
			case AvatarInputType.Touch:
				avatarInput = gameObject.AddComponent<AvatarInputTouch>();

				break;
			case AvatarInputType.VR:
				break;
			default:
				break;
		}

		OnInit?.Invoke(inputType);

	}


}