using UnityEngine.EventSystems;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// This script is made by Andrew's tutorial.
// https://www.youtube.com/watch?v=3mRI1hu9Y3w

public class VR_EventSystemInputModule : BaseInputModule 
{
    public Camera m_Camera;

    private GameObject m_CurrentObject = null;
    private PointerEventData m_Data = null;

    protected override void Awake()
    {
        if (Application.platform != RuntimePlatform.Android)
            this.enabled = false;

        base.Awake();
        m_Data = new PointerEventData(eventSystem);
    }

    public override void Process() {
//#if !UNITY_EDITOR && !UNITY_STANDALONE
        // Reset data, set camera.
        m_Data.Reset();
        m_Data.position = new Vector2(m_Camera.pixelWidth / 2f, m_Camera.pixelHeight / 2f);

        // Raycast.
        eventSystem.RaycastAll(m_Data, m_RaycastResultCache);
        m_Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        m_CurrentObject = m_Data.pointerCurrentRaycast.gameObject;

        // Clear.
        m_RaycastResultCache.Clear();

        // Hover.
        HandlePointerExitAndEnter(m_Data, m_CurrentObject);

        // Press.
        if (UiButtonIsStateDown())
            ProcessPress(m_Data);

        // Release.
        if (UiButtonIsStateUp())
            ProcessRelease(m_Data);
//#endif
    }

    public PointerEventData GetData() {
        return m_Data;
    }

    private void ProcessPress(PointerEventData data) {
        // Set raycast.
        data.pointerPressRaycast = data.pointerCurrentRaycast;

        // Check for object hit, get the down handler, call.
        GameObject newPointPress =
            ExecuteEvents.ExecuteHierarchy(m_CurrentObject, data, ExecuteEvents.pointerDownHandler);

        // If no down handler, try and get click handler.
        if (newPointPress == null)
            newPointPress = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);

        // Set data.
        data.pressPosition = data.position;
        data.pointerPress = newPointPress;
        data.rawPointerPress = m_CurrentObject;
    }

    private void ProcessRelease(PointerEventData data) {
        // Execute pointer up.
        ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerUpHandler);

        // Check for click handler.
        GameObject pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(m_CurrentObject);

        // Check if actual.
        if (data.pointerPress == pointerUpHandler) {
            ExecuteEvents.Execute(data.pointerPress, data, ExecuteEvents.pointerClickHandler);
        }

        // Clear selected game object.
        eventSystem.SetSelectedGameObject(null);

        // Reset data.
        data.pressPosition = Vector2.zero;
        data.pointerPress = null;
        data.rawPointerPress = null;
    }

    public bool UiButtonIsStateDown() {
        
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                   OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) || Input.GetMouseButtonDown(0))
        
            return true;
        return false;

        
    }

    public bool UiButtonIsStateUp() {
        
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) || Input.GetMouseButtonUp(0))
        
            return false;
        return true;
    }

}

