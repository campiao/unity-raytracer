using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float m_DeltaTime = 0.0f;
    private float m_Timer = 0.0f;       // Timer to control update frequency
    private float m_Fps = 0.0f;    
    private float m_Msec = 0.0f; 

    void Update()
    {
        m_DeltaTime += (Time.deltaTime - m_DeltaTime) * 0.1f;

        m_Timer += Time.deltaTime;
        if (m_Timer >= 0.1f)
        {
            m_Timer = 0.0f;

            m_Fps = 1.0f / m_DeltaTime;
            m_Msec = m_DeltaTime * 1000.0f;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 25, w - 20, h * 4 / 100);
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = h * 4 / 100;
        style.normal.textColor = Color.black;

        string text = string.Format("Rendering stats: {0:0.0} ms ({1:0.} fps)", m_Msec, m_Fps);
        GUI.Label(rect, text, style);
    }
}
