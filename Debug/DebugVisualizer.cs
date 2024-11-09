using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Castrimaris.Monitoring
{
    [DefaultExecutionOrder(-1000)]
    public class DebugVisualizer : MonoBehaviour
    {
        public Camera mainCamera;
        public Material material;
        public GameObject prefabDebugPoint;
        public GameObject prefabDebugLine;
        // layerMask is Debug
        public LayerMask debugLayer; 
        private Quaternion lastCameraRotation;

        [SerializeField]
        private Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();
        private Queue<GameObject> recycledObjects = new Queue<GameObject>();

        public static DebugVisualizer Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }
        private void Start()
        {
            // auto assign camera
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                debugLayer = LayerMask.NameToLayer("Debug");
            }
        }
        private void Update()
        {
            if (mainCamera == null || mainCamera.transform.rotation == lastCameraRotation)
                return;

            lastCameraRotation = mainCamera.transform.rotation;
            foreach (var debugObject in objectPool)
            {
                if (debugObject.Value != null)
                {
                    debugObject.Value.transform.rotation = mainCamera.transform.rotation;
                }
            }
        }

        public void DrawPoint(Vector3 position, Color color, float size, string idLog)
        {
            if (!objectPool.TryGetValue(idLog, out GameObject point) || point == null)
            {
                point = Instantiate(prefabDebugPoint, position, Quaternion.identity);
                if(point.GetComponent<Collider>() != null)
                    Destroy(point.GetComponent<Collider>());
                point.name = idLog;
                point.gameObject.layer = debugLayer;
                objectPool[idLog] = point;
            }

            if(point != null)
            {
                point.gameObject.layer = debugLayer;
                point.transform.position = position;
                point.transform.localScale = Vector3.one * size;
                Renderer renderer = point.GetComponent<Renderer>();
                if(renderer != null)
                {
                    if (renderer.material == null)
                    {
                        renderer.material = CreateInstancedMaterial();
                    }
                    renderer.material.color = color;
                }
                else
                {
                    Debug.LogWarning("Renderer non trovato su " + idLog);
                }
            }
            else
            {
                Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
            }
        }
        public void DrawLine(Vector3 start, Vector3 end, Color color, string idLog)
        {
            if (!objectPool.TryGetValue(idLog, out GameObject lineObject) || lineObject == null)
            {
                lineObject = Instantiate(prefabDebugLine, Vector3.zero, Quaternion.identity);
                if(lineObject.GetComponent<Collider>() != null)
                    Destroy(lineObject.GetComponent<Collider>());
                lineObject.name = idLog;
                lineObject.layer = debugLayer;
                objectPool[idLog] = lineObject;
            }

            if (lineObject != null)
            {
                lineObject.transform.position = (start + end) / 2;
                lineObject.transform.up = (end - start).normalized;
                lineObject.transform.localScale = new Vector3(0.005f, (end - start).magnitude / 2, 0.005f);
                var renderer = lineObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (renderer.material == null)
                    {
                        renderer.material = CreateInstancedMaterial();
                    }
                    renderer.material.color = color;
                }
                else
                {
                    Debug.LogWarning("Renderer non trovato su " + idLog);
                }
            }
            else
            {
                Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
            }
        }

        public void DrawSphere(Vector3 center, Color color, float radius, string idLog)
        {
            if (!objectPool.TryGetValue(idLog, out GameObject sphereObject) || sphereObject == null)
            {
                sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                if(sphereObject.GetComponent<Collider>() != null)
                    Destroy(sphereObject.GetComponent<Collider>());
                sphereObject.name = idLog;
                sphereObject.layer = debugLayer;
                objectPool[idLog] = sphereObject;
            }

            if (sphereObject != null)
            {
                sphereObject.transform.position = center;
                sphereObject.transform.localScale = Vector3.one * radius * 2;
                var renderer = sphereObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (renderer.material == null)
                    {
                        renderer.material = CreateInstancedMaterial();
                    }

                    renderer.material.color = color;
                    renderer.material.renderQueue = 4000;
                }
                else
                {
                    Debug.LogWarning("Renderer non trovato su " + idLog);
                }
            }
            else
            {
                Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
            }
        }

        public void DrawText(Vector3 position, string text, Color color, int size, string idLog)
        {
            if (!objectPool.TryGetValue(idLog, out GameObject textObject) || textObject == null)
            {
                textObject = new GameObject("DebugText");
                if(textObject.GetComponent<Collider>() != null)
                    Destroy(textObject.GetComponent<Collider>());
                textObject.name = idLog;
                textObject.layer = debugLayer;
                objectPool[idLog] = textObject;
            }
            
            if (textObject != null)
            {
                if(textObject.GetComponent<Collider>() != null)
                    Destroy(textObject.GetComponent<Collider>());
                TextMesh textMesh = textObject.GetComponent<TextMesh>() ?? textObject.AddComponent<TextMesh>();
                textMesh.text = text;
                textMesh.color = color;
                textMesh.fontSize = size;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.transform.position = position;
                textMesh.transform.rotation = mainCamera.transform.rotation;
            }
            else
            {
                Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
            }
        }
        
        public void DrawBox(Vector3 center, Vector3 size, Color color, string idLog)
        {
           if (!objectPool.TryGetValue(idLog, out GameObject boxObject) || boxObject == null)
           {
               boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
               if(boxObject.GetComponent<Collider>() != null)
                   Destroy(boxObject.GetComponent<Collider>());
               boxObject.name = idLog;
               boxObject.layer = debugLayer;
               objectPool[idLog] = boxObject;
           }
           
           if (boxObject != null)
           {
               boxObject.transform.position = center;
               boxObject.transform.localScale = size;
               var renderer = boxObject.GetComponent<Renderer>();
               if (renderer != null)
               {
                   if (renderer.material == null)
                   {
                       renderer.material = CreateInstancedMaterial();
                   }
                   renderer.material.color = color;
                   renderer.material.renderQueue = 4000;
               }
               else
               {
                   Debug.LogWarning("Renderer non trovato su " + idLog);
               }
           }
           else
           {
               Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
           }
        }
        public void DrawCylinder(Vector3 position, float radius, float height, Color color, string idLog)
        {
            if (!objectPool.TryGetValue(idLog, out GameObject cylinderObject) || cylinderObject == null)
            {
                cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                if(cylinderObject.GetComponent<Collider>() != null)
                    Destroy(cylinderObject.GetComponent<Collider>());
                cylinderObject.name = idLog;
                cylinderObject.layer = debugLayer;
                objectPool[idLog] = cylinderObject;
            }
            
            if (cylinderObject != null)
            {
                cylinderObject.transform.position = position;
                cylinderObject.transform.localScale = new Vector3(radius * 2, height / 2, radius * 2);
                var renderer = cylinderObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (renderer.material == null)
                    {
                        renderer.material = CreateInstancedMaterial();
                    }
                    renderer.material.color = color;
                    renderer.material.renderQueue = 4000;
                }
                else
                {
                    Debug.LogWarning("Renderer non trovato su " + idLog);
                }
            }
            else
            {
                Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
            }
        }
        public void DrawArc(Vector3 center, float radius, float angleStart, float angleEnd, Color color, string idLog)
        {
           if (!objectPool.TryGetValue(idLog, out GameObject arcObject) || arcObject == null)
           {
               arcObject = new GameObject("DebugArc");
               arcObject.name = idLog;
               arcObject.layer = debugLayer;
               objectPool[idLog] = arcObject;
           }
           
           if (arcObject != null)
           {
               arcObject.transform.position = center;
               arcObject.transform.rotation = mainCamera.transform.rotation;
               arcObject.transform.localScale = Vector3.one * radius * 2;
               var renderer = arcObject.GetComponent<Renderer>();
               if (renderer != null)
               {
                   if (renderer.material == null)
                   {
                       renderer.material = CreateInstancedMaterial();
                   }
                   renderer.material.color = color;
                   renderer.material.renderQueue = 4000;
               }
               else
               {
                   Debug.LogWarning("Renderer non trovato su " + idLog);
               }
               var meshFilter = arcObject.GetComponent<MeshFilter>();
               if (meshFilter != null)
               {
                   meshFilter.mesh = CreateArcMesh(angleStart, angleEnd);
                   if(arcObject.GetComponent<Collider>() != null)
                       Destroy(arcObject.GetComponent<Collider>());
               }
               else
               {
                   Debug.LogWarning("MeshFilter non trovato su " + idLog);
               }
           }
           else
           {
               Debug.LogWarning("GameObject non trovato o distrutto per idLog: " + idLog);
           }
        }
        public void DestroyDrawObject(string idLog)
        {
            if (objectPool.TryGetValue(idLog, out GameObject obj))
            {
                DestroyImmediate(obj);
                objectPool.Remove(idLog);
            }
        }
        private Mesh CreateArcMesh(float angleStart, float angleEnd)
        {
            var mesh = new Mesh();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var angleStep = 5f;
            var angle = angleStart;
            var angleDelta = angleEnd - angleStart;
            var vertexIndex = 0;
            while (angle < angleEnd)
            {
                var x = Mathf.Cos(angle * Mathf.Deg2Rad);
                var y = Mathf.Sin(angle * Mathf.Deg2Rad);
                vertices.Add(new Vector3(x, y, 0));
                normals.Add(Vector3.forward);
                uvs.Add(new Vector2(x, y));
                if (angle > angleStart)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(0);
                }
                angle += angleStep;
                vertexIndex++;
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            return mesh;
        }
        
        private GameObject CreateOrReuseObject(string idLog)
        {
            GameObject obj;
            if (objectPool.ContainsKey(idLog))
            {
                obj = objectPool[idLog];
                if (obj == null)
                {
                    objectPool.Remove(idLog);
                }
                else
                {
                    return obj;
                }
            }

            if (recycledObjects.Count > 0)
            {
                obj = recycledObjects.Dequeue();
                obj.name = idLog;
            }
            else
            {
                obj = new GameObject(idLog);
            }

            obj.layer = debugLayer;
            objectPool[idLog] = obj;
            return obj;
        }

        private void DestroyObject(string idLog)
        {
            if (objectPool.ContainsKey(idLog))
            {
                var obj = objectPool[idLog];
                if (obj != null)
                {
                    recycledObjects.Enqueue(obj);
                    obj.SetActive(false);
                }
                objectPool.Remove(idLog);
            }
        }
        
        private Material CreateInstancedMaterial()
        {
            var mat = material;
            mat.enableInstancing = true;
            return mat;
        }
        private GameObject FindByIdLog(string idLog)
        {
            return objectPool.TryGetValue(idLog, out var value) ? value : null;
        }
    }
}