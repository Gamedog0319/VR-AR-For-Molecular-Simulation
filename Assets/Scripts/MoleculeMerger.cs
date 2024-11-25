using UnityEngine;

public class MoleculeMerger : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Check if colliding with another molecule
        if (collision.gameObject.CompareTag("GeneratedMolecule"))
        {
            // Separate molecules to prevent overlap
            SeparateMolecules(collision.gameObject);
        }
    }

    void SeparateMolecules(GameObject otherMolecule)
    {
        // Calculate separation direction (away from the collision point)
        Vector3 separationDirection = (transform.position - otherMolecule.transform.position).normalized;

        // Move both molecules away from each other
        transform.position += separationDirection * 0.1f;
        otherMolecule.transform.position -= separationDirection * 0.1f;
    }
}

