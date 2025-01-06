using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RendererManager : MonoBehaviour
{
    [SerializeField] private RayTracingManager m_RayTracingManager;

    [Header("Renderer Controls")]
    [SerializeField] private TMP_InputField m_RaysPerPixelInput;
    [SerializeField] private TMP_InputField m_MaxRecursionInput;
    [SerializeField] private Toggle m_AntiAliasingToggle;
    [SerializeField] private Toggle m_UseUnityCPToggle;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        SetUpInputFields();
        UpdateUI();
    }

    private void SetUpInputFields()
    {
        m_RaysPerPixelInput.characterLimit = 3;
        m_RaysPerPixelInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_RaysPerPixelInput.characterValidation = TMP_InputField.CharacterValidation.Integer;

        m_MaxRecursionInput.characterLimit = 3;
        m_MaxRecursionInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_MaxRecursionInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
    }

    private void UpdateUI()
    {
        m_RaysPerPixelInput.text = m_RayTracingManager.m_NumRaysPerPixel.ToString();
        m_MaxRecursionInput.text = m_RayTracingManager.m_MaxRecursionDepth.ToString();
        m_AntiAliasingToggle.isOn = m_RayTracingManager.m_AllowAntiAliasing == 1 ? true : false;
        m_UseUnityCPToggle.isOn = m_RayTracingManager.m_UseUnityCameraPerspective;
    }

    public void OnAntiAliasingToggle(bool value)
    {
        m_RayTracingManager.SetAntiAliasingValue(value);
    }

    public void OnRecursionDepthUpdate(string value)
    {
        int recursionDepth = int.Parse(value);
        m_RayTracingManager.m_MaxRecursionDepth = recursionDepth;
    }

    public void OnRaysPerPixelUpdate(string value)
    {
        int raysPerPixel = int.Parse(value);
        m_RayTracingManager.m_NumRaysPerPixel = raysPerPixel;
    }

    public void OnUseUnityCameraPerspectiveToggle(bool value)
    {
        m_RayTracingManager.SetUseUnityCameraPerspectiveValue(value);
    }
}
