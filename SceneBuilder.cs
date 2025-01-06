using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

// Unity Object responsible for constructing and rendering the scene based on loaded data
public class SceneBuilder : MonoBehaviour
{
    public RawImage m_DisplayImage;
    public int m_RenderTextureWidth = 1920;
    public int m_RenderTextureHeight = 1080;
    public Material m_BaseMaterial;

    [SerializeField] private Camera m_MainCamera;
    [SerializeField] private UIManager m_UIManager;


    private string m_DefaultPath = "Assets/Resources/Config/TestScene.txt";
    private RenderTexture m_RenderTexture;
    private SceneService m_SceneService = new SceneService();
    private List<ObjectData> m_SceneObjects = new List<ObjectData>();
    private Transformation m_SceneTransformation = new Transformation();
    
    // Loads the data from disk and builds the corresponding Unity Scene
    public void LoadData()
    {
        ClearScene();
#if UNITY_EDITOR
        string filePath = EditorUtility.OpenFilePanel("Load Config File", "Assets/Resources/Config", "txt");
        if (filePath.Length != 0)
        {
            m_SceneObjects = m_SceneService.LoadSceneObjects(filePath);
            BuildScene();
        }
#else
        //m_DefaultPath = Application.dataPath + m_DefaultPath;
        m_SceneObjects = m_SceneService.LoadSceneObjects(m_DefaultPath);
        BuildScene();
#endif
    }

    private void ClearScene()
    {
        GameObject[] unityObjectsParent = GameObject.FindGameObjectsWithTag("UnityObjectsParent");
        if (unityObjectsParent.Length > 0)
        {
            GameObject gameObject = unityObjectsParent[0];
            Destroy(gameObject);
        }
    }

    // Creates all Unity Objects loaded from the configuration file
    private void BuildScene()
    {
        // Empty object located at position (0, 0, 0) to facilitate objects rotation
        GameObject unityObjectsParent = new GameObject("UnityObjectsParent");
        unityObjectsParent.tag = "UnityObjectsParent";

        List<Transformation> sceneTransformationList = new();

        int numObjects = 0;
        foreach (var objData in m_SceneObjects)
        {
            if (objData is CameraData cameraData)
            {
                GameObject objCamera = GameObject.Find("MainCamera");
                Camera camera = objCamera.GetComponent<Camera>();
                camera.fieldOfView = cameraData.m_Fov;
                m_SceneTransformation = cameraData.m_Transformations[0];
                sceneTransformationList.Add(m_SceneTransformation);
                objCamera.transform.position = new Vector3(0,0,cameraData.m_Distance);
                ConfigureCamera();
            }

            else if (objData is LightData lightData)
            {
                GameObject objLight = new GameObject("Light" + numObjects.ToString());
                objLight.transform.SetParent(unityObjectsParent.transform, true);
                Light light = objLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = lightData.m_Color;
                light.intensity = 0.45f;
                light.shadows = LightShadows.Hard;

                ApplyTransformations(objLight, lightData.m_Transformations);
                objLight.transform.LookAt(Vector3.zero);
            }

            // Creating objects from MeshData
            // Each MeshData instance creates a new Unity Object from a complex mesh
            // The triangles are split into submeshes based on their RayTracingMaterial (RTM)
            // E.g. -> a mesh with 1 RTM has 1 submesh, a mesh with 2 RTMs has 2 submeshes, etc.
            // Each submesh has its list of triangles and its own Unity Material 
            //
            // In Unity, each triangle in a mesh is defined by 3 vertex indices (int)
            // This function stores all triangle vertices and maps the triangles correspondingly
            else if (objData is MeshData meshData)
            {
                GameObject obj = new GameObject("RayTracingModel" + numObjects.ToString());
                obj.transform.SetParent(unityObjectsParent.transform);

                MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

                Mesh mesh = new Mesh();

                List<Vector3> vertices = new List<Vector3>();
                // Dictionary that maps RayTracingMaterial data to list of vertices indices (triangle information)
                Dictionary<RayTracingMaterial, List<int>> submeshTriangles = new Dictionary<RayTracingMaterial, List<int>>();
                // List of materials for the mesh
                List<Material> materials = new List<Material>();

                foreach (var triangle in meshData.m_Triangles)
                {
                    int baseIndex = vertices.Count;
                    vertices.Add(triangle.m_Points[0]);
                    vertices.Add(triangle.m_Points[1]);
                    vertices.Add(triangle.m_Points[2]);

                    if (!submeshTriangles.ContainsKey(triangle.m_MaterialProperties))
                    {
                        submeshTriangles[triangle.m_MaterialProperties] = new List<int>();
                        Material newMaterial = CreateMaterial(triangle.m_MaterialProperties);
                        materials.Add(newMaterial);
                    }
                    submeshTriangles[triangle.m_MaterialProperties].Add(baseIndex);
                    submeshTriangles[triangle.m_MaterialProperties].Add(baseIndex + 1);
                    submeshTriangles[triangle.m_MaterialProperties].Add(baseIndex + 2);
                }
                mesh.vertices = vertices.ToArray();

                mesh.subMeshCount = submeshTriangles.Count;
                int submeshIndex = 0;
                foreach (var entry in submeshTriangles)
                {
                    // Assign the triangles to the submesh
                    mesh.SetTriangles(entry.Value.ToArray(), submeshIndex);
                    submeshIndex++;
                }

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                meshFilter.mesh = mesh;
                meshRenderer.materials = materials.ToArray();

                // Store RayTracingMaterial data for later use when performing RayTracing
                RayTracingModel rayTracingModel = obj.AddComponent<RayTracingModel>();
                rayTracingModel.m_Materials.AddRange(submeshTriangles.Keys.ToArray());

                ApplyTransformations(obj, meshData.m_Transformations);
            }

            // Creates the corresponding Unity Primitive based on primitive type
            else if (objData is PrimitiveData primData)
            {
                if (primData.m_Type == "Sphere")
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.SetParent(unityObjectsParent.transform);
                    RayTracingSphere rayTracingSphere = sphere.AddComponent<RayTracingSphere>();
                    rayTracingSphere.m_Material = primData.m_Material;

                    ApplyTransformations(sphere, primData.m_Transformations);
                    SphereCollider collider = sphere.GetComponent<SphereCollider>();

                    rayTracingSphere.m_Center = sphere.transform.TransformPoint(collider.center);
                    rayTracingSphere.m_Radius = sphere.transform.lossyScale.x * 0.5f;

                    ApplyMaterial(sphere, primData.m_Material);
                }

                else if (primData.m_Type == "Box")
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(unityObjectsParent.transform);
                    RayTracingModel rayTracingModel = cube.AddComponent<RayTracingModel>();
                    rayTracingModel.m_Materials.Add(primData.m_Material);
                    ApplyTransformations(cube, primData.m_Transformations);
                    ApplyMaterial(cube, primData.m_Material);
                }
            }

            ++numObjects;
        }

        // Apply the scene's transformation to all objects in scene (except the camera)
        ApplyTransformations(unityObjectsParent,  sceneTransformationList);
    }

    // Configures the camera so that it is facing the negative z-axis and has a solid-color background
    private void ConfigureCamera()
    {
        m_MainCamera.transform.eulerAngles = new Vector3(0, 180, 0);
        m_MainCamera.orthographic = false;
        m_MainCamera.clearFlags = CameraClearFlags.SolidColor;
        RayTracingManager rayTracingManager = FindFirstObjectByType<RayTracingManager>();
        m_MainCamera.backgroundColor = rayTracingManager.m_ImageSettings.m_BackgroundColor;
    }

    // Helper function for applying transformation data to the object
    private void ApplyTransformations(GameObject obj, List<Transformation> transformation)
    {
        int sphereAdjuster = 1;
        foreach (var transform in transformation)
        {
            if (transform.m_Scale != Vector3.one)
                // Adjust sphere scales as in the original file it is meant to scale the radius!
                if (obj.GetComponent<RayTracingSphere>() != null)
                    sphereAdjuster = 2;
                obj.transform.localScale = transform.m_Scale * sphereAdjuster;
            obj.transform.Rotate(transform.m_Rotation);
            obj.transform.Translate(transform.m_Translation, Space.World);
            sphereAdjuster = 1;
        }
    }

    // Helper function for applying the material data to the object
    private void ApplyMaterial(GameObject obj,RayTracingMaterial matProperties)
    {
        Material newMaterial = CreateMaterial(matProperties);
        obj.GetComponent<Renderer>().material = newMaterial;
    }

    // Helper function that creates a new Unity Material based on matProperties
    private Material CreateMaterial(RayTracingMaterial matProperties)
    {
        Material newMaterial = new Material(m_BaseMaterial);
        newMaterial.color = matProperties.m_Color;

        return newMaterial;
    }
}


