using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ImageControlsManager : MonoBehaviour
{
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private RayTracingManager m_RayTracingManager;

    [Header("Image Controls")]
    [SerializeField] private TMP_InputField m_ResolutionXInput;
    [SerializeField] private TMP_InputField m_ResolutionYInput;
    [SerializeField] private Slider m_RedSlider;
    [SerializeField] private Slider m_GreenSlider;
    [SerializeField] private Slider m_BlueSlider;
    [SerializeField] private TMP_InputField m_RedInput;
    [SerializeField] private TMP_InputField m_GreenInput;
    [SerializeField] private TMP_InputField m_BlueInput;
    
    private bool m_IsUpdating = false;

    private void Start()
    {
        Initialize();
    }

    public void UpdateUI()
    {
        m_IsUpdating = true;

        m_ResolutionXInput.text = m_RayTracingManager.m_ImageSettings.m_Width.ToString();
        m_ResolutionYInput.text = m_RayTracingManager.m_ImageSettings.m_Height.ToString();

        Color bgColor = m_MainCamera.backgroundColor;

        m_RedSlider.value = Mathf.RoundToInt(bgColor.r * 255.0f);
        m_GreenSlider.value = Mathf.RoundToInt(bgColor.g * 255.0f);
        m_BlueSlider.value = Mathf.RoundToInt(bgColor.b * 255.0f);

        m_RedInput.text = Mathf.RoundToInt(bgColor.r * 255.0f).ToString();
        m_GreenInput.text = Mathf.RoundToInt(bgColor.g * 255.0f).ToString();
        m_BlueInput.text = Mathf.RoundToInt(bgColor.b * 255.0f).ToString();

        m_IsUpdating = false;
    }

    private void Initialize()
    {
        SetUpSliders();
        SetUpInputFields();
        UpdateUI();
    }

    private void SetUpSliders()
    {
        foreach (Slider slider in new[] { m_RedSlider, m_GreenSlider, m_BlueSlider })
        {
            slider.minValue = 0.0f;
            slider.maxValue = 255.0f;
            slider.wholeNumbers = true;
        }
    }

    private void SetUpInputFields()
    {
        m_ResolutionXInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_ResolutionXInput.characterLimit = 4;
        m_ResolutionXInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
        m_ResolutionYInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_ResolutionYInput.characterLimit = 4;
        m_ResolutionYInput.characterValidation = TMP_InputField.CharacterValidation.Integer;

        foreach (TMP_InputField input in new[] { m_RedInput, m_GreenInput, m_BlueInput })
        {
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.characterValidation = TMP_InputField.CharacterValidation.Integer;
            input.characterLimit = 3;
        }
    }

    public void OnImageSettingsUpdate()
    {
        int width = int.Parse(m_ResolutionXInput.text);
        int height = int.Parse(m_ResolutionYInput.text);
        m_RayTracingManager.m_ImageSettings.m_Width = width;
        m_RayTracingManager.m_ImageSettings.m_Height = height;
    }

    public void OnBgColorSliderUpdate()
    {
        Color newColor = new Color(
            m_RedSlider.value / 255.0f,
            m_GreenSlider.value / 255.0f,
            m_BlueSlider.value / 255.0f
        );
        m_MainCamera.backgroundColor = newColor;

        m_RedInput.text = Mathf.RoundToInt(newColor.r * 255.0f).ToString();
        m_GreenInput.text = Mathf.RoundToInt(newColor.g * 255.0f).ToString();
        m_BlueInput.text = Mathf.RoundToInt(newColor.b * 255.0f).ToString();
    }

    public void OnRGBInputsUpdate()
    {
        float r = int.Parse(m_RedInput.text) / 255.0f;
        float g = int.Parse(m_GreenInput.text) / 255.0f;
        float b = int.Parse(m_BlueInput.text) / 255.0f;
        m_MainCamera.backgroundColor = new Color(r, g, b);

        m_RedSlider.value = Mathf.RoundToInt(r * 255.0f);
        m_GreenSlider.value = Mathf.RoundToInt(g * 255.0f);
        m_BlueSlider.value = Mathf.RoundToInt(b * 255.0f);
    }
}
