using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
	[SerializeField]
	private RectTransform uiControl;

	[SerializeField]
	private RectTransform backButton;

	[SerializeField]
	private RectTransform menuMain;
	[SerializeField]
	private RectTransform menuGesture;
	[SerializeField]
	private RectTransform menuEmotion;

	[SerializeField]
	private UIJoystick joystickL;
	[SerializeField]
	private UIJoystick joystickR;
	[SerializeField]
	private RectTransform jumpButton;





	private void Start()
	{
		AvatarInput.Instance.OnInit += AvatarInputOnInit;
	}

	private void AvatarInputOnInit(AvatarInputType inputType)
	{
		InitControl(inputType);
	}

	public void InitControl(AvatarInputType inputType)
	{
		switch (inputType)
		{
			case AvatarInputType.Keyboard:
				uiControl.gameObject.SetActive(true);
				joystickL.gameObject.SetActive(false);
				joystickR.gameObject.SetActive(false);
				jumpButton.gameObject.SetActive(false);



				break;
			case AvatarInputType.Touch:
				uiControl.gameObject.SetActive(true);
				joystickL.gameObject.SetActive(true);
				joystickR.gameObject.SetActive(true);
				jumpButton.gameObject.SetActive(true);


				break;
			case AvatarInputType.VR:
				uiControl.gameObject.SetActive(false);

				break;
			default:
				break;
		}

		AvatarInput.Instance.OnInit -= AvatarInputOnInit;
	}

	public void SwithMenu(MenuType menuType)
	{
		switch (menuType)
		{
			case MenuType.Main:
				backButton.gameObject.SetActive(false);
				menuMain.gameObject.SetActive(true);
				menuGesture.gameObject.SetActive(false);
				menuEmotion.gameObject.SetActive(false);

				break;
			case MenuType.Emotion:
				backButton.gameObject.SetActive(true);
				menuMain.gameObject.SetActive(false);
				menuGesture.gameObject.SetActive(false);
				menuEmotion.gameObject.SetActive(true);

				break;
			case MenuType.Gesture:
				backButton.gameObject.SetActive(true);
				menuMain.gameObject.SetActive(false);
				menuGesture.gameObject.SetActive(true);
				menuEmotion.gameObject.SetActive(false);

				break;
			default:
				break;
		}
	}

	public void SwithMenuToMain()
	{
		SwithMenu(MenuType.Main);
	}
	public void SwithMenuToEmotion()
	{
		SwithMenu(MenuType.Emotion);
	}
	public void SwithMenuToGesture()
	{
		SwithMenu(MenuType.Gesture);
	}

	

	public void JoysticksLOnValueChange(Vector2 value)
	{
		AvatarInput.Instance.avatarInput.ProcessMoving(new Vector3(value.x, 0.0f, value.y));
	}
	public void JoysticksROnValueChange(Vector2 value)
	{
		AvatarInput.Instance.avatarInput.ProcessLook(value);
	}
	public void ChangeViewMode()
	{
		AvatarInput.Instance.avatarInput.ProcessChangeViewMode(true);
	}
	public void Jump()
	{
		AvatarInput.Instance.avatarInput.ProcessJump(true);
	}

}

public enum MenuType : byte
{
	Main,
	Emotion,
	Gesture
}