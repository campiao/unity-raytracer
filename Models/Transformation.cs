using UnityEngine;

[System.Serializable]
public class Transformation
{
    public Vector3 m_Translation = Vector3.zero;
    public Vector3 m_Rotation = Vector3.zero;
    public Vector3 m_Scale = Vector3.one;

    public void ConsoleLog()
    {
        Debug.Log($"Transformation: {{ Translation: {m_Translation}, " +
            $"Rotation: {m_Rotation}, Scale: {m_Scale} }}");
    }
}
