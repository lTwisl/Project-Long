using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.SDF;
using UnityEngine.VFX.Utility;

[ExecuteAlways]
public class SDFSceneBaker : MonoBehaviour
{
    public LayerMask collectLayers = -1;
    public bool bakeOnAwake = false;
    [Header("Box")]
    public Vector3 center;
    public Vector3 size = Vector3.one;
    public int maxResolution = 32;
    [Header("SDF Baker")]
    [Range(1, 16)]
    public int signPassesCount = 1;
    [Range(0f, 1f)]
    public float threshold = 0.5f;
    [Range(-1f, 1f)]
    public float offset = 0f;
    [Header("Debug")]
    [SerializeField] private VisualEffect debugVFX;
    [SerializeField] private ExposedProperty sdfTextureProperty = "sdf";
    [SerializeField] private ExposedProperty sdfPositionProperty = "sdfPosition";
    [SerializeField] private ExposedProperty sdfScaleProperty = "sdfScale";

    private readonly List<Mesh> meshes = new List<Mesh>();
    private readonly List<Matrix4x4> matrices = new List<Matrix4x4>();
    private MeshToSDFBaker sdfBaker;
    public Vector3 CenterWS => transform.TransformPoint(center);

    private void OnValidate()
    {
        size = new Vector3(Mathf.Max(0, size.x), Mathf.Max(0, size.y), Mathf.Max(0, size.z));
    }

    private void Start()
    {
        debugVFX = GetComponent<VisualEffect>();
        if (Application.isEditor || bakeOnAwake)
        {
            BakeSDF();
            StartCoroutine(Baker());
        }

    }

    private IEnumerator Baker()
    {
        while (true)
        {
            BakeSDF();
            yield return new WaitForSeconds(1f);
        }
    }


    private void OnDestroy()
    {
        sdfBaker?.Dispose();
        sdfBaker = null;
    }

    [ContextMenu("Bake SDF")]
    public void BakeSDF()
    {
        CollectMeshes(meshes, matrices);

        if (sdfBaker == null)
        {
            sdfBaker = new MeshToSDFBaker(size, CenterWS, maxResolution, meshes, matrices, signPassesCount, threshold, offset);
        }
        else
        {
            sdfBaker.Reinit(size, CenterWS, maxResolution, meshes, matrices, signPassesCount, threshold, offset);
        }
        sdfBaker.BakeSDF();

        if (debugVFX != null)
        {
            debugVFX.SetTexture(sdfTextureProperty, sdfBaker.SdfTexture);
            debugVFX.SetVector3(sdfScaleProperty, sdfBaker.GetActualBoxSize());
            debugVFX.SetVector3(sdfPositionProperty, CenterWS);
        }
    }

    private void CollectMeshes(List<Mesh> meshes, List<Matrix4x4> matrices)
    {
        // Find all mesh renderers on scene
        MeshRenderer[] meshRenderers = FindObjectsOfType<MeshRenderer>();

        // Prepare lists
        meshes.Clear();
        matrices.Clear();
        meshes.Capacity = Mathf.Max(meshes.Capacity, meshRenderers.Length);
        matrices.Capacity = Mathf.Max(matrices.Capacity, meshRenderers.Length);

        // Collect valid meshes matching the layer mask
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            MeshRenderer meshRenderer = meshRenderers[i];
            if (collectLayers == (collectLayers | (1 << meshRenderer.gameObject.layer)) && meshRenderer.TryGetComponent(out MeshFilter meshFilter))
            {
                meshes.Add(meshFilter.sharedMesh);
                matrices.Add(meshRenderers[i].localToWorldMatrix);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Baking box gizmo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(CenterWS, size);
    }
}

#if UNITY_EDITOR
// Inspector bake button
[UnityEditor.CustomEditor(typeof(SDFSceneBaker))]
public class SDFSceneBakerEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnityEditor.EditorGUILayout.Space();
        if (GUILayout.Button("Bake SDF"))
        {
            (target as SDFSceneBaker).BakeSDF();
        }
    }
}
#endif