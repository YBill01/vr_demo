using UnityEngine;

public class PlayerChose : MonoBehaviour
{
	[SerializeField] private CharacterController parent;

	private void OnEnable()
	{
#if UNITY_ANDROID
		InitVRController();
#endif
	}

	private void InitVRController()
	{
		LaserPointerHolder.Instance.SetPointerParent(parent);
	}
	
}