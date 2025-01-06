using System.Collections.Generic;
using System.IO;
using System.Globalization;
using UnityEngine;

// Service responsible for loading and interpreting data from a scene configuration file
// Handles the creation of ObjectData to be loaded into Unity GameObjects
public class SceneService
{
    // Method to load scene objects from a given configuration file path
    // Returns the list of objects to be loaded, which may include a camera, lights, meshes and spheres
    public List<ObjectData> LoadSceneObjects(string filePath)
    {
        List<Transformation> transforms = new List<Transformation>();
        List<RayTracingMaterial> materialProperties = new List<RayTracingMaterial>();

        List<ObjectData> sceneObjects = new List<ObjectData>();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found at {filePath}");
            return sceneObjects;
        }

        var source = new StreamReader(filePath);
        var fileContents = source.ReadToEnd();
        source.Close();

        string[] lines = fileContents.Split("\n"[0]);
        for (int i = 0; i < lines.Length; i++)
        {
            string objectTypeLine = lines[i].Trim();
            ++i;
            ++i;

            string[] values;

            // Image Settings
            // Width, Height and Background Color values are set to the RayTacingManager Object
            if (objectTypeLine == "Image")
            {
                values = lines[i].Trim().Split(" ");
                GameObject rayTracingManagerObj = GameObject.Find("RayTracingManager");
                RayTracingManager rayTracingManager = rayTracingManagerObj.GetComponent<RayTracingManager>();
                
                rayTracingManager.m_ImageSettings.m_Width = int.Parse(values[0]);
                rayTracingManager.m_ImageSettings.m_Height = int.Parse(values[1]);

                ++i;

                values = lines[i].Trim().Split(" ");
                float r = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                float g = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                float b = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                rayTracingManager.m_ImageSettings.m_BackgroundColor = new Color(r, g, b);

                ++i;
                ++i;
            }

            // Transformation Data
            // Creates and stores each transformation for later access when loading camera/light/mesh/sphere/box data
            else if (objectTypeLine == "Transformation")
            {
                values = lines[i].Trim().Split(" ");
                Transformation transformation = new Transformation();
                
                string firstLetter = values[0];
                while (!(firstLetter=="}"))
                {
                    if (firstLetter=="T")
                    {
                        float x = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat);
                        transformation.m_Translation.Set(x, y, z);
                    }
                    else if (firstLetter=="S")
                    {
                        float x = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat);
                        transformation.m_Scale.Set(x, y, z);
                    }
                    else if (firstLetter=="Rx")
                    {
                        int x = int.Parse(values[1]);
                        transformation.m_Rotation.x = x;
                    }
                    else if (firstLetter == "Ry")
                    {
                        int y = int.Parse(values[1]);
                        transformation.m_Rotation.y = y;
                    }
                    else if (firstLetter == "Rz")
                    {
                        int z = int.Parse(values[1]);
                        transformation.m_Rotation.z = z;
                    }

                    ++i;
                    values = lines[i].Trim().Split(' ');
                    firstLetter = values[0];
                }
                transforms.Add(transformation);

                ++i;
            }

            // Materials Data
            // Creates and stores each material for later acess when loading mesh/sphere/box data
            else if (objectTypeLine == "Material")
            {
                values = lines[i].Trim().Split(" ");
                RayTracingMaterial material = new RayTracingMaterial();

                float r = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                float g = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                float b = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                material.m_Color = new Color(r, g, b);
                ++i;

                values = lines[i].Trim().Split(' ');
                material.m_AmbientFactor = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                material.m_DiffuseFactor = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                material.m_SpecularFactor = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);
                material.m_ReflexionFactor = float.Parse(values[3], CultureInfo.InvariantCulture.NumberFormat);
                material.m_IOR = float.Parse(values[4], CultureInfo.InvariantCulture.NumberFormat);

                materialProperties.Add(material);

                ++i;
                ++i;
            }

            // Camera Data
            // Stores the scene transformation into the camera object, FOV and distance value
            // Camera will be created at position (0, 0, distance)
            else if (objectTypeLine == "Camera")
            {
                values = lines[i].Trim().Split(" ");
                CameraData cameraData = new CameraData();

                int transformIndex = int.Parse(values[0]);
                cameraData.m_Transformations.Add(transforms[transformIndex]);
                ++i;

                values = lines[i].Trim().Split(' ');
                float fov = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                cameraData.m_Fov = fov;
                ++i;

                values = lines[i].Trim().Split(' ');
                cameraData.m_Distance = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);

                sceneObjects.Add(cameraData);

                ++i;
                ++i;
            }

            // Light Data
            // Stores transformation and light color data
            else if (objectTypeLine == "Light")
            {
                values = lines[i].Trim().Split(" ");
                LightData lightData = new LightData();

                int transformIndex = int.Parse(values[0]);
                lightData.m_Transformations.Add(transforms[transformIndex]);
                //lightData.m_Transformations.Add(m_SceneTransform);
                ++i;

                values = lines[i].Trim().Split(" ");
                float r = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                float g = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                float b = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                lightData.m_Color = new Color(r, g, b);

                sceneObjects.Add(lightData);
                ++i;
                ++i;
            }

            // Mesh Data
            // Stores Triangle data (vertex positions and material data) and its transformation
            else if (objectTypeLine == "Triangles")
            {
                values = lines[i].Trim().Split(" ");
                MeshData meshData = new MeshData();

                int transformIndex = int.Parse (values[0]);
                meshData.m_Transformations.Add (transforms[transformIndex]);
                //meshData.m_Transformations.Add(m_SceneTransform);
                ++i;

                values = lines[i].Trim().Split(' ');
                while (values[0] != "}")
                {
                    TriangleData triangle = new TriangleData();
                    int materialIndex = int.Parse(values[0]);
                    triangle.m_MaterialProperties = materialProperties[materialIndex];
                    ++i;

                    values = lines[i].Trim().Split(' ');
                    for (int j = 0; j < 3; j++)
                    {
                        float x = float.Parse(values[0], CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(values[1], CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(values[2], CultureInfo.InvariantCulture.NumberFormat);

                        triangle.m_Points[j] = new Vector3 (x, y, z);

                        ++i;
                        values = lines[i].Trim().Split(' ');
                    }
                    meshData.m_Triangles.Add(triangle);
                }
                sceneObjects.Add(meshData);

                ++i;
            }

            // Sphere Data
            // Stores transformation and material
            // Primitive Type is used for creating corresponding Unity Primitive
            else if (objectTypeLine == "Sphere")
            {
                values = lines[i].Trim().Split(" ");
                PrimitiveData spherePrimitive = new PrimitiveData();
                spherePrimitive.m_Type = "Sphere";

                int transformIndex = int.Parse(values[0]);
                spherePrimitive.m_Transformations.Add(transforms[transformIndex]);
                //spherePrimitive.m_Transformations.Add(m_SceneTransform);
                ++i;

                values = lines[i].Trim().Split(" ");
                int materialIndex = int.Parse(values[0]);
                spherePrimitive.m_Material = materialProperties[materialIndex];

                sceneObjects.Add(spherePrimitive);

                ++i;
                ++i;
            }

            // Box Data
            // Stores transformation and material
            // Primitive Type is used for creating corresponding Unity Primitive
            else if (objectTypeLine == "Box")
            {
                values = lines[i].Trim().Split(" ");
                PrimitiveData cubePrimitive = new PrimitiveData();
                cubePrimitive.m_Type = "Box";

                int transformIndex = int.Parse(values[0]);
                cubePrimitive.m_Transformations.Add(transforms[transformIndex]);
                //cubePrimitive.m_Transformations.Add(m_SceneTransform);
                ++i;

                values = lines[i].Trim().Split(" ");
                int materialIndex = int.Parse(values[0]);
                cubePrimitive.m_Material = materialProperties[materialIndex];

                sceneObjects.Add(cubePrimitive);

                ++i;
                ++i;
            }
        }

        return sceneObjects;
    }
}
