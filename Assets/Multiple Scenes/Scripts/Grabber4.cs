using UnityEngine;

public class Grabber4 : MonoBehaviour
{
    public Transform moleculesParent; // Reference to the parent transform containing all molecules
    public GameObject[] moleculePrefabs; // Array to hold references to different molecule prefabs

    private void Start()
    {
        LoadMolecule();
    }

    // Function to load saved molecule data
    private void LoadMolecule()
    {
        string moleculeName = PlayerPrefs.GetString("SelectedMolecule");

        if (!string.IsNullOrEmpty(moleculeName))
        {
            // Load the molecule prefab from the Resources folder using the stored name
            GameObject moleculePrefab = Resources.Load<GameObject>(moleculeName);

            if (moleculePrefab != null)
            {
                // Instantiate the molecule prefab
                GameObject moleculeObject = Instantiate(moleculePrefab);

                // Set the parent to moleculesParent
                moleculeObject.transform.SetParent(moleculesParent);
            }
        }
    }


    // Function to get the index of the prefab in moleculePrefabs array based on molecule data
    // Function to get the index of the prefab in moleculePrefabs array based on molecule data
    private int GetPrefabIndexForMoleculeData(MoleculeData moleculeData)
    {
        // Implement your logic to match molecule data with prefabs
        // You can compare positions, rotations, or scales to determine the appropriate prefab index
        // For simplicity, you can return a default index or -1 if not found

        // Example logic:
        for (int i = 0; i < moleculePrefabs.Length; i++)
        {
            // Compare molecule data with prefabs using some criteria
            // For example, comparing positions
            if (Vector3.Distance(moleculeData.position, moleculePrefabs[i].transform.position) < 0.01f)
            {
                // Return the index if a match is found
                return i;
            }
        }

        // If no match is found, return -1
        return -1;
    }
}