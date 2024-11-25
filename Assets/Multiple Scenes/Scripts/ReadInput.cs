using UnityEngine;

public class ReadInput : MonoBehaviour
{
    public static string input;

    public void ReadStringInput(string s)
    {
        input = s;
        Debug.Log(input);

        // Store input value in PlayerPrefs
        PlayerPrefs.SetString("InputValue", input);
    }
}
