using UnityEngine;

public class Grabber : MonoBehaviour
{
    public GameObject[] moleculePrefabs; // Array to hold references to different molecule prefabs

    private GameObject selectedObject; // Reference to the currently selected molecule prefab instance
    private Vector3 offset; // Offset between mouse click point and object center

    private bool isDragging = false; // Flag to track whether a molecule is being dragged
    private bool isRotating = false; // Flag to track whether a molecule is being rotated
    private bool isScaling = false; // Flag to track whether a molecule is being scaled

    private void Update()
    {
        // Check if left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = CastRay();

            if (hit.collider != null && hit.collider.CompareTag("Molecule"))
            {
                // Instantiate a new instance of the clicked molecule
                InstantiateMoleculeInstance(hit.point, hit.collider.gameObject);
            }
        }

        // Check if a molecule prefab is selected and left mouse button is pressed
        if (selectedObject != null && Input.GetMouseButton(0))
        {
            // If not rotating or scaling, move the molecule
            if (!isRotating && !isScaling)
            {
                // Calculate the new position of the selected molecule
                Vector3 newPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)) + offset;

                // Update the position of the selected molecule
                selectedObject.transform.position = newPosition;
            }
            else if (isRotating)
            {
                // Rotate the molecule based on mouse movement along X and Y axes
                float rotationSpeed = 2f;
                float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
                float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

                // Rotate around the X-axis (up/down)
                selectedObject.transform.Rotate(Vector3.right, mouseY, Space.World);

                // Rotate around the Y-axis (left/right)
                selectedObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
            }
        }

        // Check if a molecule prefab is selected and left mouse button is released
        if (selectedObject != null && Input.GetMouseButtonUp(0))
        {
            selectedObject = null; // Reset selected object
        }

        // Check for rotation input (R key)
        if (Input.GetKeyDown(KeyCode.R) && selectedObject != null)
        {
            isRotating = !isRotating;
            isScaling = false; // Ensure scaling is disabled
        }

        // Check for scaling input (mouse wheel)
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && selectedObject != null)
        {
            isScaling = true;
            isRotating = false; // Ensure rotating is disabled
        }

        // Perform scaling if scaling flag is true
        if (isScaling)
        {
            ScaleMolecule();
        }
    }

    // Function to instantiate an instance of the clicked molecule
    private void InstantiateMoleculeInstance(Vector3 position, GameObject clickedMolecule)
    {
        // Find the index of the clicked molecule in the molecule prefabs array
        int index = System.Array.IndexOf(moleculePrefabs, clickedMolecule);

        // Instantiate a new instance of the clicked molecule
        if (index != -1)
        {
            selectedObject = Instantiate(moleculePrefabs[index], position, Quaternion.identity);
            offset = selectedObject.transform.position - position;

            // Add Rigidbody component if not already added
            Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = selectedObject.AddComponent<Rigidbody>();
                rb.isKinematic = true; // Set Rigidbody to kinematic
            }
        }
    }

    // Function to scale the selected molecule based on mouse wheel movement
    private void ScaleMolecule()
    {
        float scaleFactor = 0.5f;
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        Vector3 newScale = selectedObject.transform.localScale + new Vector3(scrollWheel * scaleFactor, scrollWheel * scaleFactor, scrollWheel * scaleFactor);
        selectedObject.transform.localScale = newScale;
    }

    // Function to cast a ray from the mouse position
    private RaycastHit CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit;
        }

        return hit;
    }

    // Collision detection and response
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is a molecule
        if (collision.gameObject.CompareTag("Molecule"))
        {
            // Make the Rigidbody kinematic to prevent movement
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Rigidbody thisRb = selectedObject.GetComponent<Rigidbody>();
            if (thisRb != null)
            {
                thisRb.isKinematic = true;
            }
        }
    }
}
