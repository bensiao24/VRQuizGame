using UnityEngine;

public class Script : MonoBehaviour
{
    public float distance = 1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            transform.Translate(Vector3.forward * distance);
        }
    }
}
