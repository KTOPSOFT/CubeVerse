using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public float rotationSpeed = 5f;
    private Transform selectedObject;
    private bool isDragging = false;
    public new string tag = "";

    void Update()
    {
        // On mouse down (or first touch)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if(hit.transform.tag == tag)
                {
                    selectedObject = hit.transform;
                    isDragging = true;
                }         
            }
        }

        // On mouse up (or touch end)
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            selectedObject = null;
        }

        // Drag to rotate
        if (isDragging && selectedObject != null)
        {
            float deltaX = Input.GetAxis("Mouse X");
            selectedObject.Rotate(Vector3.up, -deltaX * rotationSpeed, Space.World);
        }
    }
}
