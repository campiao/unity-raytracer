using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleData
{
    public Vector3[] m_Points = new Vector3[3] { Vector3.zero, Vector3.zero, Vector3.zero };
    public RayTracingMaterial m_MaterialProperties = new RayTracingMaterial();

    public void ConsoleLog()
    {
        Debug.Log($"Triangle: {{ PointA: {m_Points[0]}, PointB: {m_Points[1]}, PointC: {m_Points[2]}}}");
    }
}
