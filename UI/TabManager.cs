using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UI Helper for handling changing between local and global context for RayTracing
public class TabManager : MonoBehaviour
{
    public GameObject[] tabs;
    public Image[] tabButtons;
    public Sprite InactiveTab, ActiveTab;

    public void SwitchToTab(int tabID)
    {
        foreach (GameObject tab in tabs)
        {
            tab.SetActive(false);
        }
        tabs[tabID].SetActive(true);

        foreach (Image im in tabButtons)
        {
            im.sprite = InactiveTab;
        }
        tabButtons[tabID].sprite = ActiveTab;
    }
}