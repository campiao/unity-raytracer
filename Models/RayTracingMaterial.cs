using UnityEngine;

[System.Serializable]
public struct RayTracingMaterial
{
    public Color m_Color;
    public float m_AmbientFactor;
    public float m_DiffuseFactor;      
    public float m_SpecularFactor;
    public float m_ReflexionFactor;
    public float m_IOR;

}
