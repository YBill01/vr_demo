using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VR_PhysicalButton : MonoBehaviour
{
    //public GameObject cube;
    [SerializeField] private RectTransform rt;
    private BoxCollider collider;

    [ContextMenu("Create collider")]
    void CreatePhysicalCollider()
    {
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }

        if (collider != null)
        {
            Vector3 size = rt.sizeDelta;
            size.z = 0.5f;
            collider.size = size;
        }
        else
            Debug.Log($"ERROR: no collider for button.", gameObject);

        //cube.transform.position = transform.position;
        //Vector3 v = rt.sizeDelta;
        //v.z = 1f;
        //cube.transform.localScale = v;
    }
}
