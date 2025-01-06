using UnityEngine;

[System.Serializable]
public class ImageSettings
{
    public int m_Width = 1920;
    public int m_Height = 1080;
    public Color m_BackgroundColor = Color.white;

    public void ConsoleLog()
    {
        Debug.Log($"ImageSettings {{ Width: {m_Width}, Height: {m_Height}, " +
            $"BackgroundColor: {m_BackgroundColor} }}");
    }
}
