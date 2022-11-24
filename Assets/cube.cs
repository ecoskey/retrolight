using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public class cube : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.04f, 0.02f, -0.001f);
        transform.position = new Vector3(Mathf.Cos(Time.frameCount / 100f) * 2, Mathf.Sin(Time.frameCount / 100f) * 2);
    }
}
