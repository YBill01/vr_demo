using UnityEngine;
using UnityEngine.Serialization;

public class LaserPointerHolder : MonoBehaviour
{
    public static LaserPointerHolder Instance;
    
    [SerializeField] private CanvasLaserPointer _canvasLaserPointer;
    [FormerlySerializedAs("VrForDemo")] [SerializeField] private OVRPlayerController VRController;
    private void Awake() => Instance = this;
    
    public void SetPointerParent(CharacterController parent)
    {
        VRController.gameObject.transform.parent = parent.transform;
        VRController.transform.localPosition = Vector3.zero;
        VRController.SetController(parent);
    }
}
