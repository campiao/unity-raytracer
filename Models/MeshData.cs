using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData : ObjectData
{
    public List<TriangleData> m_Triangles = new List<TriangleData>();

    public void ConsoleLog()
    {
        Debug.Log("MeshData: ");
        m_Transformations[0].ConsoleLog();
        foreach (var triangle in m_Triangles)
            triangle.ConsoleLog();
    }
}
