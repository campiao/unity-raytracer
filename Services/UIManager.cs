using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private SceneBuilder m_SceneBuilder;
    [SerializeField] private RawImage m_3DDisplayImage;
    [SerializeField] private RawImage m_RayTracedDisplayImage;
    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private GameObject m_LocalTab;
    [SerializeField] private GameObject m_GlobalTab;
    [SerializeField] private TMP_Text m_RayTracingDurationText;
    [SerializeField] private RayTracingManager m_RayTracingManager;

    [Header("UI Managers")]
    [SerializeField] private ImageControlsManager m_ImageControlsManager;
    [SerializeField] private CameraControlsManager m_CameraControlsManager;
    [SerializeField] private LightingControlsManager m_LightingControlsManager;
    [SerializeField] private RendererManager m_RendererManager;

    private string m_RelativeSaveRTImageDir = "Assets/SavedImages/RayTracingRenders/";
    private string m_RelativeSave3DRenderDir = "Assets/SavedImages/3DRenders/";
    private string m_RelativeLoad3DRenderPath = "Assets/SavedImages/3DRenders/TestScene.png";


    private string m_RTImagesDirPath = "SavedImages/RayTracingRenders/";
    private string m_3DRendersDirPath = "SavedImages/3DRenders";
    private RenderTexture m_3DRenderedTexture;
    private SceneService m_SceneService = new SceneService();

    private void Start()
    {
        m_3DRenderedTexture = new RenderTexture(1920, 1080, 24);
        m_3DRenderedTexture.antiAliasing = 4;
        m_3DRenderedTexture.filterMode = FilterMode.Bilinear;
    }

    public void UpdateManagersUI()
    {
        m_ImageControlsManager.UpdateUI();
        m_CameraControlsManager.UpdateUI();
    }

    public void ExitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnLoadDataClick()
    {
        m_SceneBuilder.LoadData();
        UpdateManagersUI();
    }

    public void On3DRenderStart()
    {
        m_MainCamera.enabled = true;
        m_3DRenderedTexture = new RenderTexture(1920, 1080, 24);
        m_3DRenderedTexture.antiAliasing = 4;
        m_3DRenderedTexture.filterMode = FilterMode.Bilinear;

        m_MainCamera.targetTexture = m_3DRenderedTexture;
        m_3DDisplayImage.texture = m_3DRenderedTexture;
    }

    public void On3DRenderStop()
    {
        m_MainCamera.enabled = false;
        m_3DDisplayImage.texture = null;
    }

    public void OnSaveImageClick()
    {
        
        if (!Directory.Exists(Application.dataPath + "/" + m_RTImagesDirPath))
        {
            Directory.CreateDirectory(Application.dataPath + "/" + m_RTImagesDirPath);
        }

        RenderTexture rayTracedImage = new RenderTexture(m_RayTracedDisplayImage.texture.width, m_RayTracedDisplayImage.texture.width, 24);
        Graphics.Blit(m_RayTracedDisplayImage.texture, rayTracedImage);

        RenderTexture.active = rayTracedImage;
        Texture2D screenshot = new Texture2D(rayTracedImage.width, rayTracedImage.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, rayTracedImage.width, rayTracedImage.height), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;
        string filename = $"screenshot_{System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.png";
#if UNITY_EDITOR
        string filePath = EditorUtility.SaveFilePanel("Save RayTraced Image as PNG", Application.dataPath + "/" + m_RTImagesDirPath, filename, "png");
        if (filePath.Length != 0)
        {
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"Image saved to: {filePath}");
        }
#else
        byte[] bytes = screenshot.EncodeToPNG();
        string filePath = Path.Combine(m_RelativeSaveRTImageDir, filename);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Image saved to: {filePath}");
#endif
    }

    public void OnLoadImageClick()
    {
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Load 3D Rendered Image", "Assets/SavedImages/", "png, jpeg");
        if (filePath.Length != 0)
        {
            Texture2D texture = new Texture2D(m_3DRenderedTexture.width, m_3DRenderedTexture.height);

            var fileContent = File.ReadAllBytes(filePath);
            texture.LoadImage(fileContent);
            Graphics.Blit(texture, m_3DRenderedTexture);
            m_3DDisplayImage.texture = m_3DRenderedTexture;
            m_MainCamera.enabled = false;
        }

#else
        string filePath = m_RelativeLoad3DRenderPath;
        Texture2D texture = new Texture2D(m_3DRenderedTexture.width, m_3DRenderedTexture.height);

        var fileContent = File.ReadAllBytes(filePath);
        texture.LoadImage(fileContent);
        Graphics.Blit(texture, m_3DRenderedTexture);
        m_3DDisplayImage.texture = m_3DRenderedTexture;
        m_MainCamera.enabled = false;
#endif
    }

    public void OnSaveSceneClick()
    {
        // TODO
    }

    public void OnTabSwitch()
    {
        m_LightingControlsManager.UpdateUI();
    }

    public void OnSave3DRenderClick()
    {
        if (!Directory.Exists(Application.dataPath + "/" + m_3DRendersDirPath))
        {
            Directory.CreateDirectory(Application.dataPath + "/" + m_3DRendersDirPath);
        }

        RenderTexture renderedImage = new RenderTexture(m_3DRenderedTexture.width, m_3DRenderedTexture.width, 24);
        Graphics.Blit(m_3DRenderedTexture, renderedImage);

        RenderTexture.active = renderedImage;
        Texture2D screenshot = new Texture2D(renderedImage.width, renderedImage.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, renderedImage.width, renderedImage.height), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;

        string filename = $"screenshot_{System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.png";
#if UNITY_EDITOR
        string filePath = EditorUtility.SaveFilePanel("Save 3D Rendered Image as PNG", Application.dataPath + "/" + m_3DRendersDirPath, filename, "png");
        if (filePath.Length != 0)
        {
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            Debug.Log($"Image saved to: {filePath}");
        }
#else
        string filePath = Path.Combine(m_RelativeSave3DRenderDir, filename);
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        Debug.Log($"Image saved to: {filePath}");
#endif
    }
}
