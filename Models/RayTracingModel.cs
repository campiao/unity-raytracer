using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingModel : MonoBehaviour
{
    public List<RayTracingMaterial> m_Materials = new List<RayTracingMaterial>();
    public MeshFilter m_MeshFilter;
    public MeshRenderer m_MeshRender;

    private void Awake()
    {
        m_MeshFilter = gameObject.GetComponent<MeshFilter>();
        m_MeshRender = gameObject.GetComponent<MeshRenderer>();
    }
}
