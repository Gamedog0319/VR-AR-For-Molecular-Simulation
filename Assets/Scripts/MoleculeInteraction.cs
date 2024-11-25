using UnityEngine;

public class MoleculeInteraction : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 screenPoint;
    private Vector3 offset;
    private bool isRotating = false;
    private bool canScale = false;

    void OnMouseDown()
    {
        if (gameObject.CompareTag("GeneratedMolecule"))
        {
            isDragging = true;
            screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }

    void Update()
    {
        if (gameObject.CompareTag("GeneratedMolecule"))
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isRotating = !isRotating;
            }

            if (isRotating)
            {
                float rotationSpeed = 100f;
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
                float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

                transform.Rotate(Vector3.up, -rotationX);
                transform.Rotate(Vector3.right, rotationY);
            }

            if (canScale && Input.mouseScrollDelta.y != 0)
            {
                float scaleChange = Input.mouseScrollDelta.y * 0.1f;
                transform.localScale += new Vector3(scaleChange, scaleChange, scaleChange);
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
