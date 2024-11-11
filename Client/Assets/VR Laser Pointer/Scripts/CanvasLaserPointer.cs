using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

// This script is made by Andrew's tutorial.
// https://www.youtube.com/watch?v=3mRI1hu9Y3w

public class CanvasLaserPointer : MonoBehaviour
{
    [Header("Settings")]
    public float defaultLength;
    public LayerMask layerMask;

    [Header("Links")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject dot;
    [SerializeField] private Camera _camera;
    public VR_EventSystemInputModule mEventSystemInputModule;
    [Header("Debug")] [SerializeField] private TMP_Text debugInfo;


    private void OnEnable()
    {
        FindObjectOfType<Lobby>().InitWorldCanvasCamera(_camera);
    }

    void Update() {
        UpdateLaserLine();
    }

    void UpdateLaserLine() {
        bool hitUI;
        float length;
        PointerEventData data = mEventSystemInputModule.GetData();
        if (data.pointerCurrentRaycast.distance == 0) {
            hitUI = false;
            length = defaultLength;
        }
        else {
            length = data.pointerCurrentRaycast.distance;
            hitUI = true;
        }

        //        debugInfo.text = "Dist: " + data.pointerCurrentRaycast.distance;
        if (debugInfo != null)
            debugInfo.text = data.ToString();
        //float length = data.pointerCurrentRaycast.distance == 0 ? defaultLength : data.pointerCurrentRaycast.distance;

        Vector3 endPos = transform.position + transform.forward * length;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, length, layerMask);

        if (hit.transform != null) {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hit.point);
            dot.transform.position = hit.point;
            SetDotSize();
            dot.SetActive(true);
        }
        else // No object under laser point.
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPos);
            if (hitUI) {
                dot.transform.position = endPos;
                SetDotSize();
                dot.SetActive(true);
            }
            else
                dot.SetActive(false);
        }
    }

    private void SetDotSize() {
        float distanceToEyes = Vector3.Distance(transform.position, dot.transform.position);
        if (distanceToEyes > 1f)
            dot.transform.localScale = Vector3.one * 0.04f;
        else
            dot.transform.localScale = Vector3.one * 0.01f;
    }

    public bool UiButtonIsStateDown() {

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                   OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            return true;
        return false;
    }

    public bool UiButtonIsStateUp() {
        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            return true;
        return false;
    }

}
