using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightData : ObjectData
{
    public Color m_Color = Color.white;

    public void ConsoleLog()
    {
        Debug.Log($"Light: {{ Color: {{{m_Color}}} }}");
    }
}
