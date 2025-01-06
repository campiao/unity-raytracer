using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LightingControlsManager : MonoBehaviour
{
    [SerializeField] private GameObject m_LocalTab;
    [SerializeField] private RayTracingManager m_RayTracingManager;

    [Header("Lighting Controls")]
    [SerializeField] private Toggle m_AmbientToggle;
    [SerializeField] private Toggle m_DiffuseToggle;
    [SerializeField] private Toggle m_SpecularToggle;
    [SerializeField] private Toggle m_RefractionToggle;
    [SerializeField] private Slider m_LightIntensitySlider;
    [SerializeField] private TMP_InputField m_LightIntensityInput;

    private bool m_IsUpdating = false;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        SetUpSliders();
        SetUpInputFields();
        UpdateUI();
    }

    private void SetUpSliders()
    {
        m_LightIntensitySlider.minValue = 0.0f;
        m_LightIntensitySlider.maxValue = 10.0f;
        m_LightIntensitySlider.wholeNumbers = true;
    }

    private void SetUpInputFields()
    {
        m_LightIntensityInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_LightIntensityInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
        m_LightIntensityInput.characterLimit = 3;
    }

    public void UpdateUI()
    {
        m_IsUpdating = true;

        bool isLocalEnabled = m_LocalTab.activeInHierarchy;
        if (isLocalEnabled)
        {
            m_AmbientToggle.isOn = m_RayTracingManager.m_AllowLocalAmbientComponent == 1 ? true : false;
            m_DiffuseToggle.isOn = m_RayTracingManager.m_AllowLocalDiffuseComponent == 1 ? true : false;
            m_SpecularToggle.isOn = m_RayTracingManager.m_AllowLocalSpecularComponent == 1 ? true : false;
            m_RefractionToggle.isOn = m_RayTracingManager.m_AllowLocalRefractionComponent == 1 ? true : false;
        }
        else
        {
            m_AmbientToggle.isOn = m_RayTracingManager.m_AllowGlobalAmbientComponent == 1 ? true : false;
            m_DiffuseToggle.isOn = m_RayTracingManager.m_AllowGlobalDiffuseComponent == 1 ? true : false;
            m_SpecularToggle.isOn = m_RayTracingManager.m_AllowGlobalSpecularComponent == 1 ? true : false;
            m_RefractionToggle.isOn = m_RayTracingManager.m_AllowGlobalRefractionComponent == 1 ? true : false;
        }

        m_IsUpdating = false;
    }
    public void OnAmbientToggle(bool value)
    {
        bool isLocalEnabled = m_LocalTab.activeInHierarchy;
        m_RayTracingManager.SetAmbientComponent(value, isLocalEnabled);
    }

    public void OnDiffuseToggle(bool value)
    {
        bool isLocalEnabled = m_LocalTab.activeInHierarchy;
        m_RayTracingManager.SetDiffuseComponent(value, isLocalEnabled);
    }

    public void OnSpecularToggle(bool value)
    {
        bool isLocalEnabled = m_LocalTab.activeInHierarchy;
        m_RayTracingManager.SetSpecularComponent(value, isLocalEnabled);
    }

    public void OnRefractionToggle(bool value)
    {
        bool isLocalEnabled = m_LocalTab.activeInHierarchy;
        m_RayTracingManager.SetRefractionComponent(value, isLocalEnabled);
    }
}
