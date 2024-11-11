using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicalLaserPointer : MonoBehaviour
{
    public float defaultLength;

    [SerializeField] private Transform pointedObject;

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject dot;

    void Update()
    {
        UpdateLaserLine();
    }

    void UpdateLaserLine()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, defaultLength);

        if (hit.transform != null)
        {
            // Visual.
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * hit.distance);
            dot.SetActive(true);
            dot.transform.position = hit.point;

            // Old pointed object (if exists).
            if (pointedObject != null && pointedObject != hit.transform)
                LeaveCurrentPointedObject();

            // New pointed object.
            pointedObject = hit.transform;
            Button btn = pointedObject.gameObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.OnPointerEnter(new PointerEventData(EventSystem.current)); //Works!
            }

            // Click?
            if (pointedObject == hit.transform)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    btn = pointedObject.gameObject.GetComponent<Button>();
                    btn.OnPointerDown(new PointerEventData(EventSystem.current));
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    btn = pointedObject.gameObject.GetComponent<Button>();
                    btn.OnPointerUp(new PointerEventData(EventSystem.current));
                    btn.OnPointerClick(new PointerEventData(EventSystem.current));
                }
            }
        }
        else // No object under laser point.
        {
            if (pointedObject != null)
                LeaveCurrentPointedObject();

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + transform.forward * defaultLength);

            dot.SetActive(false);
        }

        void LeaveCurrentPointedObject()
        {
            Button btn = pointedObject.gameObject.GetComponent<Button>();
            if (btn != null)
            {
                btn.OnPointerExit(new PointerEventData(EventSystem.current));
                pointedObject = null;
            }
        }
    }
}
