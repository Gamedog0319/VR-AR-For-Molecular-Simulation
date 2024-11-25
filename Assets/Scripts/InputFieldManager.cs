using UnityEngine;
using UnityEngine.UI;

public class InputFieldManager : MonoBehaviour
{
    public InputField sigmaInput;
    public InputField epsilonInput;
    public InputField lambdaInput;
    public GameObject redMoleculeUI;
    public GameObject blueMoleculeUI;

    private GameObject selectedMolecule;

    public void SetSelectedMolecule(GameObject molecule)
    {
        selectedMolecule = molecule;

        // Hide all UI elements initially
        sigmaInput.gameObject.SetActive(false);
        epsilonInput.gameObject.SetActive(false);
        lambdaInput.gameObject.SetActive(false);

        // Show UI elements based on the selected molecule
        if (selectedMolecule.CompareTag("RedMolecule"))
        {
            sigmaInput.gameObject.SetActive(true);
        }
        else if (selectedMolecule.CompareTag("BlueMolecule"))
        {
            sigmaInput.gameObject.SetActive(true);
            epsilonInput.gameObject.SetActive(true);
            lambdaInput.gameObject.SetActive(true);
        }
    }

    public void OnSubmit()
    {
        if (selectedMolecule == null)
        {
            Debug.LogError("No molecule selected.");
            return;
        }

        float sigma, epsilon = 0f, lambda = 0f;
        bool isValid = true;

        if (!float.TryParse(sigmaInput.text, out sigma))
        {
            Debug.LogError("Invalid input for Sigma");
            isValid = false;
        }

        if (selectedMolecule.CompareTag("BlueMolecule"))
        {
            if (!float.TryParse(epsilonInput.text, out epsilon))
            {
                Debug.LogError("Invalid input for Epsilon");
                isValid = false;
            }

            if (!float.TryParse(lambdaInput.text, out lambda))
            {
                Debug.LogError("Invalid input for Lambda");
                isValid = false;
            }
        }

        if (isValid)
        {
            Debug.Log("Submitted values: Sigma = " + sigma + ", Epsilon = " + epsilon + ", Lambda = " + lambda);
        }
    }
}
