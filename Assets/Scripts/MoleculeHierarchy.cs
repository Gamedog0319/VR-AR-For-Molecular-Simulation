using UnityEngine;

public class MoleculeHierarchy : MonoBehaviour
{
    public void MergeMolecules(GameObject molecule1, GameObject molecule2)
    {
        // Create a new parent GameObject to hold the merged molecules
        GameObject parentMolecule = new GameObject("ParentMolecule");

        // Make molecule1 and molecule2 children of the parent GameObject
        molecule1.transform.parent = parentMolecule.transform;
        molecule2.transform.parent = parentMolecule.transform;

        // Set the parent GameObject as the merged molecule
        // You may want to perform additional logic here if needed
    }
}

