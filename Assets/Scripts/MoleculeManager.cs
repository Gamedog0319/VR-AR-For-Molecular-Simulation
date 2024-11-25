using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MoleculeManager : MonoBehaviour
{
    public InputField redSigmaInput;
    public InputField blueSigmaInput;
    public InputField blueEpsilonInput;
    public InputField blueLambdaInput;
    public Button redSubmitButton;
    public Button blueSubmitButton;
    public Button mergeButton;
    public Button duplicateButton;
    public Button deleteButton;
    public GameObject redInputPanel;
    public GameObject blueInputPanel;
    public Text sigmaText; // Reference to the UI Text component

    private List<GameObject> molecules = new List<GameObject>();
    private List<GameObject> selectedMolecules = new List<GameObject>();
    private GameObject firstClickedMolecule = null;
    private GameObject lastClickedMolecule = null;

    private Dictionary<(GameObject, GameObject), float> separationDistances = new Dictionary<(GameObject, GameObject), float>();
    private Dictionary<GameObject, float> sigmaValues = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> epsilonValues = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> lambdaValues = new Dictionary<GameObject, float>();

    private bool isDragging = false;
    private bool isRotating = false;
    private Vector3 initialPosition;
    private Vector3 initialRotation;

    void Start()
    {
        redSubmitButton.onClick.AddListener(OnRedSubmit);
        blueSubmitButton.onClick.AddListener(OnBlueSubmit);
        mergeButton.onClick.AddListener(MergeSelectedMolecules);
        duplicateButton.onClick.AddListener(DuplicateSelectedMolecule);
        deleteButton.onClick.AddListener(DeleteSelectedMolecule);
        redInputPanel.SetActive(false);
        blueInputPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("InitialRedMolecule") && !molecules.Contains(hit.transform.gameObject))
                {
                    redInputPanel.SetActive(true);
                    blueInputPanel.SetActive(false);
                    lastClickedMolecule = hit.transform.gameObject;
                }
                else if (hit.transform.CompareTag("InitialBlueMolecule") && !molecules.Contains(hit.transform.gameObject))
                {
                    blueInputPanel.SetActive(true);
                    redInputPanel.SetActive(false);
                    lastClickedMolecule = hit.transform.gameObject;
                }
                else if (hit.transform.CompareTag("GeneratedMolecule"))
                {
                    lastClickedMolecule = hit.transform.gameObject;
                    if (sigmaValues.ContainsKey(lastClickedMolecule))
                    {
                        UpdateSigmaText(sigmaValues[lastClickedMolecule]);
                    }
                }
            }
        }

        if (isDragging && firstClickedMolecule != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            Vector3 offset = mousePos - initialPosition;
            firstClickedMolecule.transform.position += offset;

            initialPosition = mousePos;

            // Update separation distances during continuous movement
            UpdateSeparationDistances();
        }

        if (isRotating && lastClickedMolecule != null)
        {
            float rotationSpeed = 5.0f;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

            lastClickedMolecule.transform.Rotate(Vector3.up, -rotationX, Space.World);
            lastClickedMolecule.transform.Rotate(Vector3.right, rotationY, Space.World);

            // Update separation distances after rotation
            UpdateSeparationDistances();
        }

        if (Input.GetMouseButtonDown(1)) // Right-click to select a molecule
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("GeneratedMolecule"))
                {
                    if (firstClickedMolecule == null)
                    {
                        firstClickedMolecule = hit.transform.gameObject;
                    }
                    lastClickedMolecule = hit.transform.gameObject;
                    if (sigmaValues.ContainsKey(lastClickedMolecule))
                    {
                        UpdateSigmaText(sigmaValues[lastClickedMolecule]);
                    }
                }
            }
        }

        // Handle mouse wheel input for scaling the last clicked molecule
        if (lastClickedMolecule != null && Input.mouseScrollDelta.y != 0)
        {
            float scaleFactor = 1.0f + Input.mouseScrollDelta.y * 0.1f;
            lastClickedMolecule.transform.localScale *= scaleFactor;

            // Update sigma value based on new scale
            if (sigmaValues.ContainsKey(lastClickedMolecule))
            {
                sigmaValues[lastClickedMolecule] *= scaleFactor;
                // Update the displayed sigma value
                UpdateSigmaText(sigmaValues[lastClickedMolecule]);
            }

            // Update separation distances after scaling
            UpdateSeparationDistances();
        }

        if (Input.GetKeyDown(KeyCode.R) && lastClickedMolecule != null)
        {
            isRotating = true;
            initialRotation = Input.mousePosition;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            isRotating = false;
        }

        // Continuously display separation distances and interaction energies during runtime
        DisplaySeparationDistancesAndInteractionEnergies();
    }

    void OnRedSubmit()
    {
        float sigma = 0f;
        bool isValid = true;

        if (!float.TryParse(redSigmaInput.text, out sigma))
        {
            Debug.LogError("Invalid input for Sigma (Red)");
            isValid = false;
        }

        if (isValid && lastClickedMolecule != null)
        {
            GameObject newMolecule = Instantiate(lastClickedMolecule, lastClickedMolecule.transform.position, Quaternion.identity);
            newMolecule.transform.localScale = Vector3.one * sigma;
            newMolecule.tag = "GeneratedMolecule";

            newMolecule.AddComponent<MoleculeChildInteraction>();

            molecules.Add(newMolecule);
            sigmaValues[newMolecule] = sigma * 0.5f; // Adjust sigma value
            redInputPanel.SetActive(false);
            blueInputPanel.SetActive(false);

            // Update separation distances and interaction energies
            UpdateSeparationDistances();
            DisplaySeparationDistancesAndInteractionEnergies();
        }
    }

    void OnBlueSubmit()
    {
        float sigma = 0f, epsilon = 0f, lambda = 0f;
        bool isValid = true;

        if (!float.TryParse(blueSigmaInput.text, out sigma))
        {
            Debug.LogError("Invalid input for Sigma (Blue)");
            isValid = false;
        }

        if (!float.TryParse(blueEpsilonInput.text, out epsilon))
        {
            Debug.LogError("Invalid input for Epsilon (Blue)");
            isValid = false;
        }

        if (!float.TryParse(blueLambdaInput.text, out lambda))
        {
            Debug.LogError("Invalid input for Lambda (Blue)");
            isValid = false;
        }

        if (isValid && lastClickedMolecule != null)
        {
            GameObject newMolecule = Instantiate(lastClickedMolecule, lastClickedMolecule.transform.position, Quaternion.identity);
            newMolecule.transform.localScale = Vector3.one * sigma;
            newMolecule.tag = "GeneratedMolecule";

            newMolecule.AddComponent<MoleculeChildInteraction>();

            molecules.Add(newMolecule);
            sigmaValues[newMolecule] = sigma * 0.5f; // Adjust sigma value
            epsilonValues[newMolecule] = epsilon;
            lambdaValues[newMolecule] = lambda;
            redInputPanel.SetActive(false);
            blueInputPanel.SetActive(false);

            // Update separation distances and interaction energies
            UpdateSeparationDistances();
            DisplaySeparationDistancesAndInteractionEnergies();
        }
    }

    public void MergeSelectedMolecules()
    {
        if (firstClickedMolecule != null)
        {
            for (int i = 0; i < molecules.Count; i++)
            {
                if (molecules[i] != firstClickedMolecule && molecules[i].transform.parent == null)
                {
                    molecules[i].transform.SetParent(firstClickedMolecule.transform, true);
                }
            }

            // Clear the clicked molecule after merging
            firstClickedMolecule = null;

            redInputPanel.SetActive(false);
            blueInputPanel.SetActive(false);

            // Update separation distances and interaction energies
            UpdateSeparationDistances();
            DisplaySeparationDistancesAndInteractionEnergies();
        }
    }

    public void DuplicateSelectedMolecule()
    {
        if (lastClickedMolecule != null)
        {
            float offsetDistance = 2.0f; // Distance below the original molecule
            Vector3 offsetPosition = lastClickedMolecule.transform.position - new Vector3(0, offsetDistance, 0);

            GameObject duplicateMolecule = Instantiate(lastClickedMolecule, offsetPosition, lastClickedMolecule.transform.rotation);
            duplicateMolecule.transform.localScale = lastClickedMolecule.transform.localScale;
            duplicateMolecule.tag = "GeneratedMolecule";

            duplicateMolecule.AddComponent<MoleculeChildInteraction>();

            molecules.Add(duplicateMolecule);
            sigmaValues[duplicateMolecule] = sigmaValues[lastClickedMolecule];
            epsilonValues[duplicateMolecule] = epsilonValues[lastClickedMolecule];
            lambdaValues[duplicateMolecule] = lambdaValues[lastClickedMolecule];

            // Clear the clicked molecule after duplication
            lastClickedMolecule = null;

            // Update separation distances and interaction energies
            UpdateSeparationDistances();
            DisplaySeparationDistancesAndInteractionEnergies();
        }
    }

    public void DeleteSelectedMolecule()
    {
        if (lastClickedMolecule != null)
        {
            molecules.Remove(lastClickedMolecule);
            sigmaValues.Remove(lastClickedMolecule);
            epsilonValues.Remove(lastClickedMolecule);
            lambdaValues.Remove(lastClickedMolecule);
            Destroy(lastClickedMolecule);

            // Clear the clicked molecule after deletion
            lastClickedMolecule = null;

            // Update separation distances and interaction energies
            UpdateSeparationDistances();
            DisplaySeparationDistancesAndInteractionEnergies();
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        initialPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        initialPosition.z = 0;

        // Update separation distances when molecule dragging starts
        UpdateSeparationDistances();
    }

    void OnMouseUp()
    {
        isDragging = false;

        // Update separation distances when molecule dragging ends
        UpdateSeparationDistances();
    }

    // Method to calculate separation distance between two molecules
    private float CalculateSeparationDistance(GameObject moleculeA, GameObject moleculeB)
    {
        Vector3 positionA = moleculeA.transform.position;
        Vector3 positionB = moleculeB.transform.position;
        return Vector3.Distance(positionA, positionB);
    }

    // Method to calculate interaction energy between two molecules
    private float CalculateInteractionEnergy(GameObject moleculeA, GameObject moleculeB, float separationDistance)
    {
        float sigmaA = sigmaValues[moleculeA];
        float sigmaB = sigmaValues[moleculeB];
        float sigma = Mathf.Min(sigmaA, sigmaB);
        float interactionEnergy = 0f;

        if (moleculeA.CompareTag("RedMolecule") && moleculeB.CompareTag("RedMolecule"))
        {
            // Red-Red interaction
            interactionEnergy = separationDistance < sigma ? float.PositiveInfinity : 0f;
        }
        else if ((moleculeA.CompareTag("RedMolecule") && moleculeB.CompareTag("BlueMolecule")) ||
                 (moleculeA.CompareTag("BlueMolecule") && moleculeB.CompareTag("RedMolecule")))
        {
            // Red-Blue interaction
            interactionEnergy = separationDistance < sigma ? float.PositiveInfinity : 0f;
        }
        else if (moleculeA.CompareTag("BlueMolecule") && moleculeB.CompareTag("BlueMolecule"))
        {
            // Blue-Blue interaction
            float epsilonA = epsilonValues[moleculeA];
            float epsilonB = epsilonValues[moleculeB];
            float lambdaA = lambdaValues[moleculeA];
            float lambdaB = lambdaValues[moleculeB];

            bool differentSigma = sigmaA != sigmaB;
            bool differentEpsilon = epsilonA != epsilonB;
            bool differentLambda = lambdaA != lambdaB;

            if (differentSigma)
            {
                sigma = (sigmaA + sigmaB) / 2;
                interactionEnergy = CalculateSquareWellEnergy(separationDistance, sigma, epsilonA, lambdaA);
            }
            else if (differentEpsilon)
            {
                float epsilon = Mathf.Sqrt(epsilonA * epsilonB);
                interactionEnergy = CalculateSquareWellEnergy(separationDistance, sigmaA, epsilon, lambdaA);
            }
            else if (differentLambda)
            {
                float lambdaSigma = Mathf.Max(lambdaA * sigmaA, lambdaB * sigmaB);
                interactionEnergy = CalculateSquareWellEnergy(separationDistance, sigmaA, epsilonA, lambdaSigma);
            }
            else
            {
                interactionEnergy = CalculateSquareWellEnergy(separationDistance, sigmaA, epsilonA, lambdaA);
            }
        }

        return interactionEnergy;
    }

    // Method to calculate square well interaction energy
    private float CalculateSquareWellEnergy(float r, float sigma, float epsilon, float lambdaSigma)
    {
        if (r <= sigma)
        {
            return float.PositiveInfinity;
        }
        else if (r <= lambdaSigma)
        {
            return -epsilon;
        }
        else
        {
            return 0f;
        }
    }

    // Method to update separation distances for all molecule pairs
    private void UpdateSeparationDistances()
    {
        separationDistances.Clear(); // Clear previous distances

        for (int i = 0; i < molecules.Count; i++)
        {
            for (int j = i + 1; j < molecules.Count; j++)
            {
                GameObject moleculeA = molecules[i];
                GameObject moleculeB = molecules[j];

                float separationDistance = CalculateSeparationDistance(moleculeA, moleculeB);
                separationDistances[(moleculeA, moleculeB)] = separationDistance;
                separationDistances[(moleculeB, moleculeA)] = separationDistance; // Since it's symmetric
            }
        }
    }

    // Method to display separation distances and interaction energies in the console
    private void DisplaySeparationDistancesAndInteractionEnergies()
    {
        float totalInteractionEnergy = 0f;

        foreach (var pair in separationDistances)
        {
            GameObject moleculeA = pair.Key.Item1;
            GameObject moleculeB = pair.Key.Item2;
            float separationDistance = pair.Value;
            float interactionEnergy = CalculateInteractionEnergy(moleculeA, moleculeB, separationDistance);
            Debug.Log($"Separation distance between {moleculeA.name} and {moleculeB.name}: {separationDistance} meters");
            Debug.Log($"Interaction energy between {moleculeA.name} and {moleculeB.name}: {interactionEnergy}");

            totalInteractionEnergy += interactionEnergy;
        }

        Debug.Log($"Total Interaction Energy: {totalInteractionEnergy}");
    }

    // Method to update the displayed sigma value
    private void UpdateSigmaText(float value)
    {
        sigmaText.text = "Sigma: " + (value * 2.0f).ToString("F2"); // Display the input sigma value
    }

    // Method to find a safe position for duplication to avoid overlap
    private Vector3 FindSafePosition(Vector3 originalPosition, float offsetDistance)
    {
        Vector3[] directions = new Vector3[]
        {
            Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
        };

        foreach (Vector3 direction in directions)
        {
            Vector3 potentialPosition = originalPosition + direction * offsetDistance;
            if (!IsOverlapping(potentialPosition))
            {
                return potentialPosition;
            }
        }

        // If all directions are overlapping, return the original position with a small offset
        return originalPosition + Vector3.right * offsetDistance;
    }

    // Method to check if a position overlaps with existing molecules
    private bool IsOverlapping(Vector3 position)
    {
        foreach (GameObject molecule in molecules)
        {
            if (Vector3.Distance(position, molecule.transform.position) < sigmaValues[molecule] * 2)
            {
                return true;
            }
        }
        return false;
    }
}
