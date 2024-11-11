using UnityEngine;

/// <summary>
/// I have made this script because OVRPlayerController doesn't work.
/// </summary>
public class VrControl2 : MonoBehaviour {
    [SerializeField] private float moveSpeed = 2f;

    // Rotating variables.
    private const float rotationInterval = 0.5f; // Prevents too fast rotating.
    private float lastRotationTime;
    public float rotationAngle = 45f;

    [SerializeField] private Transform centerEye;
    [SerializeField] private Transform eyeHolder;

    void Update() {

        // Check if is currently child of Avatar.
        Transform affectedTransform;
        if (transform.parent != null) {
            affectedTransform = transform.parent;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            var badInput = transform.parent.GetComponent<SamplePlayerInput>();
            if (badInput != null)
                badInput.enabled = false;
            centerEye.parent = eyeHolder;
        }
        else {
            affectedTransform = transform;
        }

        // Move.
        Vector3 leftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 moveVector = Vector3.zero;
        moveVector += affectedTransform.transform.right * leftThumbstick.x;
        moveVector += affectedTransform.transform.forward * leftThumbstick.y;
        moveVector = moveVector.normalized * moveSpeed * Time.deltaTime;
        affectedTransform.transform.Translate(moveVector, Space.World);

        // Rotate.
        Vector3 rightThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        if (Mathf.Abs(rightThumbstick.x) > 0.666f && Time.time > lastRotationTime + rotationInterval) {
            lastRotationTime = Time.time;
            affectedTransform.Rotate(0f, Mathf.Sign(rightThumbstick.x) * rotationAngle, 0f, Space.Self);
        }
    }
}
