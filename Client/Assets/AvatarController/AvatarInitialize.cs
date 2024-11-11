using System.Collections;
using System.Collections.Generic;
using System.Management;
using UnityEngine;

public class AvatarInitialize : MonoBehaviour
{
	[SerializeField]
	private Transform mainCamera;
	[SerializeField]
	private Transform avatarCamera;

	[SerializeField]
	private AvatarController avatarController;
	[SerializeField]
	private RectTransform loginUI;
	[SerializeField]
	private RectTransform avatarControlUI;


	public void Init()
	{
		mainCamera.gameObject.SetActive(false);
		avatarCamera.gameObject.SetActive(true);

		// set position avatar
		//avatarController.transform.position = transform.position;


		//avatarController.gameObject.SetActive(true);
		avatarController.Init();

		// set position avatar
		avatarController.transform.position = transform.position;

		loginUI.gameObject.SetActive(false);
		avatarControlUI.gameObject.SetActive(true);



		/*System.Management.ManagementObjectSearcher mos = new System.Management.ManagementObjectSearcher("select * from win32_videcontroller");
		System.Management.ManagementObjectCollection moc = mos.Get();
		foreach (System.Management.ManagementObject mo in moc)
		{
			Debug.Log(mo.GetPropertyValue("name").ToString());
		}*/
	}

}