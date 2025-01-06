using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraControlsManager : MonoBehaviour
{
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private RayTracingManager m_RayTracingManager;

    [Header("Camera Controls")]
    [SerializeField] private TMP_InputField m_RotationXInput;
    [SerializeField] private TMP_InputField m_RotationYInput;
    [SerializeField] private TMP_InputField m_RotationZInput;
    [SerializeField] private Slider m_RotationXSlider;
    [SerializeField] private Slider m_RotationYSlider;
    [SerializeField] private Slider m_RotationZSlider;
    [SerializeField] private TMP_InputField m_PositionXInput;
    [SerializeField] private TMP_InputField m_PositionYInput;
    [SerializeField] private TMP_InputField m_PositionZInput;
    [SerializeField] private TMP_InputField m_FOVInput;
    
    private bool m_IsUpdating = false;
    private Vector3 m_CameraRotation = new();

    private void Start()
    {
        m_CameraRotation = m_MainCamera.transform.rotation.eulerAngles;
        Initialize();
    }

    public void UpdateUI()
    {
        m_IsUpdating = true;

        Vector3 rot = m_MainCamera.transform.rotation.eulerAngles;
        Vector3 currentRotation = new Vector3(rot.x, rot.y, rot.z);

        m_RotationXInput.text = Mathf.RoundToInt(currentRotation.x).ToString();
        m_RotationYInput.text = Mathf.RoundToInt(currentRotation.y).ToString();
        m_RotationZInput.text = Mathf.RoundToInt(currentRotation.z).ToString();

        m_RotationXSlider.value = currentRotation.x;
        m_RotationYSlider.value = currentRotation.y;
        m_RotationZSlider.value = currentRotation.z;

        Vector3 currentPosition = m_MainCamera.transform.position;
        m_PositionXInput.text = Mathf.RoundToInt(currentPosition.x).ToString();
        m_PositionYInput.text = Mathf.RoundToInt(currentPosition.y).ToString();
        m_PositionZInput.text = Mathf.RoundToInt(currentPosition.z).ToString();

        m_FOVInput.text = Mathf.RoundToInt(m_MainCamera.fieldOfView).ToString();

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
        foreach (Slider slider in new[] { m_RotationXSlider, m_RotationYSlider, m_RotationZSlider })
        {
            slider.minValue = -180.0f;
            slider.maxValue = 180.0f;
            slider.wholeNumbers = true;
        }
    }

    private void SetUpInputFields()
    {
        m_FOVInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        m_FOVInput.characterLimit = 3;
        m_FOVInput.characterValidation = TMP_InputField.CharacterValidation.Integer;

        foreach (TMP_InputField input in new[] { 
            m_RotationXInput, m_RotationXInput, m_RotationXInput,
            m_PositionXInput, m_PositionYInput, m_PositionZInput})
        {
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.characterLimit = 4;
            input.characterValidation = TMP_InputField.CharacterValidation.Integer;
        }
    }


    public void OnCameraFOVUpdate(string value)
    {
        m_MainCamera.fieldOfView = int.Parse(value);
    }

    public void OnRotationSliderUpdate()
    {
        if (m_IsUpdating) return;
        float rx = Mathf.RoundToInt(m_RotationXSlider.value);
        float ry = Mathf.RoundToInt(m_RotationYSlider.value);
        float rz = Mathf.RoundToInt(m_RotationZSlider.value);

        m_CameraRotation.Set(rx, ry, rz);

        m_RotationXInput.text = rx.ToString();
        m_RotationYInput.text = ry.ToString();
        m_RotationZInput.text = rz.ToString();
    }

    public void OnRotationInputUpdate()
    {
        float rx = int.Parse(m_RotationXInput.text);
        float ry = int.Parse(m_RotationYInput.text);
        float rz = int.Parse(m_RotationZInput.text);

        m_CameraRotation.Set(rx, ry, rz);

        m_IsUpdating = true;
        m_RotationXSlider.value = rx;
        m_RotationYSlider.value = ry;
        m_RotationZSlider.value = rz;
        m_IsUpdating = false;
    }

    public void OnApplyRotation()
    {
        m_MainCamera.transform.eulerAngles = new Vector3(0, 0, 0);
        m_MainCamera.transform.Rotate(m_CameraRotation);
    }

    public void OnCameraPositionUpdate()
    {
        float x = int.Parse(m_PositionXInput.text);
        float y = int.Parse(m_PositionYInput.text);
        float z = int.Parse(m_PositionZInput.text);
        m_MainCamera.transform.position = new Vector3(x, y, z);
    }
}
