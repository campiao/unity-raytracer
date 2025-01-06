using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraData : ObjectData
{
    public float m_Fov = 90.0f;
    public float m_Distance = 0.0f;

    public void ConsoleLog()
    {
        Debug.Log($"CameraData: {{ FOV: {m_Fov} }}");
        m_Transformations[0].ConsoleLog();
    }
}
