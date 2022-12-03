using UnityEngine;

public class cube : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.04f, 0.02f, -0.001f);
        transform.position = new Vector3(0, Mathf.Sin(Time.frameCount / 1000f) + 2, 0);
    }
}
