using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Mesh;

public class RayTracingManager : MonoBehaviour
{
    public RawImage m_RayTracedImage;
    public ImageSettings m_ImageSettings;
    public ComputeShader m_RayTracingShader;
    public Camera m_MainCamera;
    public UIManager m_UIManager;

    // Light Components
    public int m_AllowLocalAmbientComponent = 1;
    public int m_AllowLocalDiffuseComponent = 1;
    public int m_AllowLocalSpecularComponent = 1;
    public int m_AllowLocalRefractionComponent = 1;
    public int m_AllowGlobalAmbientComponent = 1;
    public int m_AllowGlobalDiffuseComponent = 1;
    public int m_AllowGlobalSpecularComponent = 1;
    public int m_AllowGlobalRefractionComponent = 1;

    // Ray Tracing Properties
    public int m_MaxRecursionDepth = 4;
    public int m_NumRaysPerPixel = 5;
    public bool m_UseUnityCameraPerspective = false;
    public int m_AllowAntiAliasing = 1;

    private RenderTexture m_RenderTexture;
    private Material m_Material;
    private bool m_StartedRayTracing = false;
    private MeshDataLists m_AllMeshData = new();
    private List<LightInfo> m_AllLightData = new();
    
    public struct MeshInfo
    {
        public int triangleStart;
        public int triangleCount;
        public RayTracingMaterial material; // 9 floats
        public Vector3 boundsMin; // 3 floats
        public Vector3 boundsMax; // 3 floats

    }

    public struct SphereInfo
    {
        public Vector3 center;
        public float radius;
        public RayTracingMaterial material;
    }

    public struct TriangleInfo
    {
        public Vector3 vertexA;
        public Vector3 vertexB;
        public Vector3 vertexC;

        public Vector3 normalA;
        public Vector3 normalB;
        public Vector3 normalC;
    }

    public struct LightInfo
    {
        public Vector3 position;
        public Color color;
    }

    public class MeshDataLists
    {
        public List<TriangleInfo> triangles = new();
        public List<MeshInfo> meshInfo = new();
        public List<SphereInfo> spheres = new();
    }

    private ComputeBuffer m_SphereBuffer;
    private ComputeBuffer m_MeshBuffer;
    private ComputeBuffer m_TriangleBuffer;
    private ComputeBuffer m_LightBuffer;

    private int numIterations = 0;

    private void Update()
    {
        if (m_StartedRayTracing)
        {
            SetShaderParams();
            StartRayTracing();
        }
    }

    private void OnDestroy()
    {
        ReleaseBuffers();
    }

    private void ReleaseBuffers()
    {
        if (m_SphereBuffer != null)
        {
            m_SphereBuffer.Release();
        }

        if (m_MeshBuffer != null)
        {
            m_MeshBuffer.Release();
        }

        if (m_TriangleBuffer != null)
        {
            m_TriangleBuffer.Release();
        }

        if (m_LightBuffer != null)
        {
            m_LightBuffer.Release();
        }
    }

    public void StartRayTracing()
    {
        int kernel = m_RayTracingShader.FindKernel("CSMain");
        m_RenderTexture = new RenderTexture(m_ImageSettings.m_Width, m_ImageSettings.m_Height, 24);
        m_RenderTexture.enableRandomWrite = true;
        m_RenderTexture.Create();

        m_RayTracingShader.SetTexture(kernel, "Result", m_RenderTexture);

        if (m_SphereBuffer != null)
        {
            m_RayTracingShader.SetBuffer(kernel, "SphereInfoBuffer", m_SphereBuffer);
            m_RayTracingShader.SetInt("_SphereInfoCount", m_AllMeshData.spheres.Count);
        }

        if (m_MeshBuffer != null)
        {
            m_RayTracingShader.SetBuffer(kernel, "MeshInfoBuffer", m_MeshBuffer);
            m_RayTracingShader.SetInt("_MeshInfoCount", m_AllMeshData.meshInfo.Count);
        }

        if (m_TriangleBuffer != null)
        {
            m_RayTracingShader.SetBuffer(kernel, "TriangleInfoBuffer", m_TriangleBuffer);
            m_RayTracingShader.SetInt("_TriangleInfoCount", m_AllMeshData.triangles.Count);
        }

        if (m_LightBuffer != null)
        {
            m_RayTracingShader.SetBuffer(kernel, "LightInfoBuffer", m_LightBuffer);
            m_RayTracingShader.SetInt("_LightInfoCount", m_AllLightData.Count);
        }

        uint x, y, z;
        m_RayTracingShader.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
        Vector3 threadGroupSizes = new Vector3Int((int)x, (int)y, (int)z);
        int numGroupsX = Mathf.CeilToInt(m_ImageSettings.m_Width / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(m_ImageSettings.m_Height / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(1 / (float)threadGroupSizes.z);

        m_RayTracingShader.Dispatch(kernel, numGroupsX, numGroupsY, numGroupsZ);
        m_RayTracedImage.texture = m_RenderTexture;
    }

    public void EnableRayTracing()
    {
        CreateRayTracingData();
        m_StartedRayTracing = true;
    }

    public void DisableRayTracing()
    {
        ReleaseBuffers();
        m_RayTracedImage.texture = null;
        m_StartedRayTracing = false;
    }

    public void RenderOneFrame()
    {
        CreateRayTracingData();
        SetShaderParams();
        StartRayTracing();
        ReleaseBuffers();
        m_StartedRayTracing = false;
    }

    private void CreateRayTracingData()
    {
        m_RenderTexture = new RenderTexture(m_ImageSettings.m_Width, m_ImageSettings.m_Height, 24);
        m_RenderTexture.enableRandomWrite = true;
        m_RenderTexture.Create();

        m_AllMeshData = new();
        m_AllMeshData = CreateMeshData();

        Light[] lights = FindObjectsOfType<Light>();

        m_AllLightData = new();
        foreach (Light light in lights)
        {
            LightInfo info = new();
            info.position = light.transform.position;
            info.color = light.color;
            m_AllLightData.Add(info);
        }
    }


    private void SetShaderParams()
    {
        //  Image Settings
        m_RayTracingShader.SetVector("backgroundColor", m_MainCamera.backgroundColor);

        // Camera Settings
        m_RayTracingShader.SetFloat("_CameraFov", m_MainCamera.fieldOfView);
        m_RayTracingShader.SetVector("_CameraWorldPosition", m_MainCamera.transform.position);
        m_RayTracingShader.SetMatrix("_CameraToWorldMatrix", m_MainCamera.cameraToWorldMatrix);
        m_RayTracingShader.SetMatrix("_CameraInverseProjection", m_MainCamera.projectionMatrix.inverse);

        // RayTracing Properties
        m_RayTracingShader.SetInt("_AllowLocalAmbientComponent", m_AllowLocalAmbientComponent);
        m_RayTracingShader.SetInt("_AllowLocalDiffuseComponent", m_AllowLocalDiffuseComponent);
        m_RayTracingShader.SetInt("_AllowLocalSpecularComponent", m_AllowLocalSpecularComponent);
        m_RayTracingShader.SetInt("_AllowLocalRefractiveComponent", m_AllowLocalRefractionComponent);
        m_RayTracingShader.SetInt("_AllowGlobalAmbientComponent", m_AllowGlobalAmbientComponent);
        m_RayTracingShader.SetInt("_AllowGlobalDiffuseComponent", m_AllowGlobalDiffuseComponent);
        m_RayTracingShader.SetInt("_AllowGlobalSpecularComponent", m_AllowGlobalSpecularComponent);
        m_RayTracingShader.SetInt("_AllowGlobalRefractionComponent", m_AllowGlobalRefractionComponent);

        m_RayTracingShader.SetInt("_NumRaysPerPixel", m_NumRaysPerPixel);
        m_RayTracingShader.SetInt("_MaxRecursionDepth", m_MaxRecursionDepth);
        m_RayTracingShader.SetInt("_AllowAntiAliasing", m_AllowAntiAliasing);
        m_RayTracingShader.SetBool("_UseUnityCameraPerspective", m_UseUnityCameraPerspective);

        // Compute Buffers
        if (m_SphereBuffer != null)
        {
            m_SphereBuffer.Release();
        }

        if (m_MeshBuffer != null)
        {
            m_MeshBuffer.Release();
        }

        if (m_TriangleBuffer != null)
        {
            m_TriangleBuffer.Release();
        }

        if  (m_LightBuffer != null)
        {
            m_LightBuffer.Release();
        }
        m_SphereBuffer = new ComputeBuffer(m_AllMeshData.spheres.Count, sizeof(float) * 13);
        m_SphereBuffer.SetData(m_AllMeshData.spheres);

        m_MeshBuffer = new ComputeBuffer(m_AllMeshData.meshInfo.Count, sizeof(int) * 2 + sizeof(float) * 15);
        m_MeshBuffer.SetData(m_AllMeshData.meshInfo);

        m_TriangleBuffer = new ComputeBuffer(m_AllMeshData.triangles.Count, sizeof(float) * 18);
        m_TriangleBuffer.SetData(m_AllMeshData.triangles);

        m_LightBuffer = new ComputeBuffer(m_AllLightData.Count, sizeof(float) * 7);
        m_LightBuffer.SetData(m_AllLightData);
    }

    private MeshDataLists CreateMeshData()
    {
        RayTracingModel[] models = GameObject.FindObjectsOfType<RayTracingModel>();
        RayTracingSphere[] spheres = GameObject.FindObjectsOfType<RayTracingSphere>();

        MeshDataLists meshData = new();


        foreach (RayTracingSphere sphere in spheres)
        {
            
            SphereInfo sphereInfo = new SphereInfo();
            sphereInfo.center = sphere.transform.parent.localToWorldMatrix.MultiplyPoint3x4(sphere.m_Center);
            sphereInfo.radius = sphere.m_Radius;
            sphereInfo.material = sphere.m_Material;

            meshData.spheres.Add(sphereInfo);
        }

        int numTriangles = 0;
        foreach (RayTracingModel model in models)
        {
            
            Mesh mesh = model.GetComponent<MeshFilter>().sharedMesh;

            Vector3[] vertices = mesh.vertices;
            
            int numSubMeshes = mesh.subMeshCount;
            for (int i = 0; i < numSubMeshes; i++)
            {
                MeshInfo meshInfo = new MeshInfo();
                meshInfo.triangleStart = numTriangles;
                meshInfo.material = model.m_Materials[i];

                int[] triangles = mesh.triangles;

                SubMeshDescriptor submesh = mesh.GetSubMesh(i);
                int trianglesStart = submesh.indexStart;
                int trianglesCount = submesh.indexCount;
                //model.transform.localToWorldMatrix.MultiplyPoint3x4(bounds.min);
                meshInfo.boundsMin = model.transform.TransformPoint(vertices[0]);
                meshInfo.boundsMax = model.transform.TransformPoint(vertices[0]);
                //Debug.Log($"Min: {meshInfo.boundsMin}, Max: {meshInfo.boundsMax}}}");

                int trianglesLenght = trianglesStart + trianglesCount;
                for (int j = trianglesStart; j < trianglesLenght; j += 3)
                {
                    TriangleInfo tri = new TriangleInfo();
                    tri.vertexA = vertices[triangles[j]];
                    tri.vertexB = vertices[triangles[j + 1]];
                    tri.vertexC = vertices[triangles[j + 2]];

                    tri.vertexA = model.transform.TransformPoint(tri.vertexA);
                    tri.vertexB = model.transform.TransformPoint(tri.vertexB);
                    tri.vertexC = model.transform.TransformPoint(tri.vertexC);

                    Vector3 edgeAB = tri.vertexB - tri.vertexA;
                    Vector3 edgeAC = tri.vertexC - tri.vertexA;
                    Vector3 normal = Vector3.Cross(edgeAB, edgeAC).normalized;

                    tri.normalA = normal;
                    tri.normalB = normal;
                    tri.normalC = normal;

                    //meshInfo.boundsMin = Vector3.Min(meshInfo.boundsMin, tri.vertexA);
                    //meshInfo.boundsMin = Vector3.Min(meshInfo.boundsMin, tri.vertexB);
                    //meshInfo.boundsMin = Vector3.Min(meshInfo.boundsMin, tri.vertexC);

                    //meshInfo.boundsMax = Vector3.Max(meshInfo.boundsMax, tri.vertexA);
                    //meshInfo.boundsMax = Vector3.Max(meshInfo.boundsMax, tri.vertexB);
                    //meshInfo.boundsMax = Vector3.Max(meshInfo.boundsMax, tri.vertexC);

                    meshData.triangles.Add(tri);
                    numTriangles++;
                }

                meshInfo.triangleCount = numTriangles;
                meshData.meshInfo.Add(meshInfo);
            }
        }
        

        return meshData;
    }

    public void SetAmbientComponent(bool value, bool isLocalTab)
    {
        if (isLocalTab)
            m_AllowLocalAmbientComponent = value ? 1 : 0;
        else
            m_AllowGlobalAmbientComponent = value ? 1 : 0;
    }

    public void SetDiffuseComponent(bool value, bool isLocalTab)
    {
        if (isLocalTab)
            m_AllowLocalDiffuseComponent = value ? 1 : 0;
        else
            m_AllowGlobalDiffuseComponent = value ? 1 : 0;
    }

    public void SetSpecularComponent(bool value, bool isLocalTab)
    {
        if (isLocalTab)
            m_AllowLocalSpecularComponent = value ? 1 : 0;
        else
            m_AllowGlobalSpecularComponent = value ? 1 : 0;
    }

    public void SetRefractionComponent(bool value, bool isLocalTab)
    {
        if (isLocalTab)
            m_AllowLocalRefractionComponent = value ? 1 : 0;
        else
            m_AllowGlobalRefractionComponent = value ? 1 : 0;
    }

    public void SetAntiAliasingValue(bool value)
    {
        m_AllowAntiAliasing = value ? 1: 0;
    }

    public void SetUseUnityCameraPerspectiveValue(bool value)
    {
        m_UseUnityCameraPerspective = value;
    }
}
