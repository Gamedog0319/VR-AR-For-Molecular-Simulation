using UnityEngine;

public class MoleculeChildInteraction : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 initialMousePosition;
    private Transform parentTransform;

    void OnMouseDown()
    {
        if (transform.CompareTag("GeneratedMolecule"))
        {
            isDragging = true;
            initialMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            initialMousePosition.z = 0f;
            parentTransform = transform.parent != null ? transform.parent : transform;
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentMousePosition.z = 0f;
            Vector3 offset = currentMousePosition - initialMousePosition;

            if (parentTransform != null)
            {
                parentTransform.position += offset;

                foreach (Transform child in parentTransform)
                {
                    if (child != parentTransform)
                    {
                        child.position += offset;
                    }
                }
            }

            initialMousePosition = currentMousePosition;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
