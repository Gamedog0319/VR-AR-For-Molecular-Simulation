using UnityEngine;

public class Grabber3 : MonoBehaviour
{
    public Transform moleculesParent; // Reference to the parent transform containing all molecules
    public GameObject[] moleculePrefabs; // Array to hold references to different molecule prefabs
    private float inputSize = 1.0f; // Default size

    private GameObject selectedObject; // Reference to the currently selected molecule
    private Vector3 offset; // Offset between mouse click point and object center
    private bool isRotating = false; // Flag to track whether a molecule is being rotated
    private bool isScaling = false; // Flag to track whether a molecule is being scaled

    private void Start()
    {
        // Retrieve input value from PlayerPrefs
        string inputValue = PlayerPrefs.GetString("InputValue");
        if (!string.IsNullOrEmpty(inputValue))
        {
            // Convert input value to float
            float inputSizeValue = 1.0f;
            if (float.TryParse(inputValue, out inputSizeValue))
            {
                // Call SetInputSize method with converted input size
                SetInputSize(inputSizeValue);
            }
        }
    }

    // Function to set the input size from Scene 2
    public void SetInputSize(float size)
    {
        inputSize = size;
    }

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

        // Check if a molecule is selected and left mouse button is pressed
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
                // Rotate the molecule based on mouse movement
                RotateMoleculeWithMouse();
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
            ScaleMolecule();
        }

        // Check for saving input (S key)
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveMolecule();
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

            // Set the size of the molecule based on the input value from Scene 2
            selectedObject.transform.localScale = new Vector3(inputSize, inputSize, inputSize);
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

    private void RotateMoleculeWithMouse()
    {
        // Rotate the molecule based on mouse movement
        float rotationSpeed = 2f; // Adjust rotation speed as needed
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotate around the X-axis (up/down)
        selectedObject.transform.Rotate(Vector3.right, mouseY, Space.World);

        // Rotate around the Y-axis (left/right)
        selectedObject.transform.Rotate(Vector3.up, -mouseX, Space.World);
    }

    // Function to save the position, rotation, and scale of the selected molecule
    private void SaveMolecule()
    {
        // Check if there is a selected molecule
        if (selectedObject != null)
        {
            // Get the GameObject's name
            string moleculeName = selectedObject.name;

            // Save the name to PlayerPrefs
            PlayerPrefs.SetString("SelectedMolecule", moleculeName);
            PlayerPrefs.Save();

            Debug.Log("Molecule saved!"); // Log a message indicating that the molecule is saved
        }
    }
}

