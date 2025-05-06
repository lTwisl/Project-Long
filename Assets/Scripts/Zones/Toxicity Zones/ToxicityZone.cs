using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using Zenject;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(Collider))]
public class ToxicityZone : MonoBehaviour
{
    [Inject] private World _world;

    [field: Header("Параметры обьекта:")]
    [field: SerializeField, Min(0)] public float Toxicity { get; private set; }
    public enum ZoneType
    {
        Rate,
        Single
    }
    [SerializeField] private ZoneType _currentType;
    [SerializeField] private string _zoneID = "Toxicity Zone 1";
    [SerializeField, DisableEdit] private Collider _collider;

    public string ZoneID => _zoneID;
    public ZoneType CurrentType => _currentType;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnEnterToxicityZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _world.InvokeOnExitToxicityZone(this);
    }

#if UNITY_EDITOR
    public bool ShowInfo;

    private void OnValidate()
    {
        ChangeNaming();
        CacheCollider();
    }

    private void ChangeNaming()
    {
        gameObject.name = $"[{_currentType}] {_zoneID}";
    }

    private void CacheCollider()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();

        if (_collider != null && _collider.isTrigger != true)
            _collider.isTrigger = true;
    }

    private void OnDrawGizmos()
    {
        if (!ShowInfo || _collider == null) return;

        // 1. Цвета для разных типов зон
        Color textColor = _currentType == ZoneType.Rate ?
            new Color(0.75f, 0.75f, 0.62f, 0.85f) :  // Аэрозоль
            new Color(0.75f, 0.46f, 0.75f, 0.85f);   // Хлад-9

        Color markersColor = _currentType == ZoneType.Rate ?
            new Color(0.65f, 0.65f, 0.52f, 0.85f) :
            new Color(0.65f, 0.36f, 0.65f, 0.85f);

        // 2. Отображение зоны влияние (зоны коллайдера)
        Handles.color = markersColor;

        if (_collider is BoxCollider boxCollider)
        {
            VisualizeBoxCollider(boxCollider);
        }
        else if (_collider is SphereCollider sphereCollider)
        {
            VisualizeSphereCollider(sphereCollider);
        }
        else if (_collider is CapsuleCollider capsuleCollider)
        {
            VisualizeCapsuleCollider(capsuleCollider);
        }
        else if (_collider is MeshCollider meshCollider)
        {
            VisualizeMeshCollider(meshCollider);
        }

        // 3. Текстовая информация
        GUIStyle textStyle = new GUIStyle
        {
            normal = {textColor = textColor },
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 18,
            richText = true
        };
        Handles.Label(transform.position + Vector3.up * 1f, $"<b>☣ {_zoneID}</b>\n" + $"<size=14>{_currentType} | Зараженность: {Toxicity:F1} ед.</size>", textStyle);
    }


    private void VisualizeBoxCollider(BoxCollider box)
    {
        Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
        Vector3 halfSize = size * 0.5f;
        Vector3 centre = box.transform.position + box.center;

        // 1. Получаем углы бокса
        Vector3[] corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            corners[i] = centre + new Vector3(
                (i & 1) == 0 ? -halfSize.x : halfSize.x,
                (i & 2) == 0 ? -halfSize.y : halfSize.y,
                (i & 4) == 0 ? -halfSize.z : halfSize.z);
        }

        // 2. Рисуем нижнюю грань и сферы
        Handles.DrawLine(corners[0], corners[1]);            //Handles.DrawLine(corners[3], corners[2]);
        Handles.DrawLine(corners[1], corners[5]);            //Handles.DrawLine(corners[2], corners[6]);
        Handles.DrawLine(corners[5], corners[4]);            //Handles.DrawLine(corners[6], corners[7]);
        Handles.DrawLine(corners[4], corners[0]);            //Handles.DrawLine(corners[7], corners[3]);

        Handles.SphereHandleCap(0, corners[3], Quaternion.identity, 0.25f, EventType.Repaint);
        Handles.SphereHandleCap(0, corners[2], Quaternion.identity, 0.25f, EventType.Repaint);
        Handles.SphereHandleCap(0, corners[6], Quaternion.identity, 0.25f, EventType.Repaint);
        Handles.SphereHandleCap(0, corners[7], Quaternion.identity, 0.25f, EventType.Repaint);


        // 3. Рисуем вертикальные линии по углам
        foreach (var corner in corners)
        {
            Vector3 groundPos = new Vector3(corner.x, 0, corner.z);
            Handles.DrawLine(groundPos, corner);
        }

        // 4. Рисуем линии размерности
        float sizeX = _collider.bounds.extents.x;
        float sizeZ = _collider.bounds.extents.z;
        Handles.DrawLine(centre + Vector3.forward * sizeZ - new Vector3(0, halfSize.y, 0), centre - Vector3.forward * sizeZ - new Vector3(0, halfSize.y, 0));
        Handles.DrawLine(centre + Vector3.right * sizeX - new Vector3(0, halfSize.y, 0), centre - Vector3.right * sizeX - new Vector3(0, halfSize.y, 0));
    }

    private void VisualizeSphereCollider(SphereCollider sphere)
    {
        float radius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        int markers = 8;
        float heightMarkers = 1f;
        Vector3 centre = sphere.transform.position + sphere.center;

        // 1. Рисуем линии и маркеры
        for (int i = 0; i < markers; i++)
        {
            float angle = i * Mathf.PI * 2 / markers;
            Vector3 edgePoint = centre + new Vector3(Mathf.Cos(angle) * radius, heightMarkers, Mathf.Sin(angle) * radius);

            Vector3 groundPos = new(edgePoint.x, centre.y, edgePoint.z);
            Handles.DrawLine(groundPos, edgePoint);
            Handles.SphereHandleCap(0, edgePoint, Quaternion.identity, 0.25f, EventType.Repaint);
        }

        // 3. Рисуем линии размерности
        float sizeX = _collider.bounds.extents.x;
        float sizeZ = _collider.bounds.extents.z;
        Handles.DrawLine(centre + Vector3.forward * sizeZ, centre - Vector3.forward * sizeZ);
        Handles.DrawLine(centre + Vector3.right * sizeX, centre - Vector3.right * sizeX);

        // 4. Рисуем верхнюю и нижнюю точку
        Handles.SphereHandleCap(0, centre + Vector3.up * radius, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.SphereHandleCap(0, centre - Vector3.up * radius, Quaternion.identity, 0.5f, EventType.Repaint);
    }

    private void VisualizeCapsuleCollider(CapsuleCollider capsule)
    {
        float radius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float height = capsule.height * transform.lossyScale.y;
        Vector3 centre = capsule.transform.position + capsule.center;

        // 1. Рассчитываем верхнюю и нижнюю точки капсулы
        Vector3 top = centre + Vector3.up * (height * 0.5f - radius);
        Vector3 bottom = centre - Vector3.up * (height * 0.5f - radius);

        // 2. Рисуем вертикальные линии и маркеры
        int markers = 8;
        for (int i = 0; i < markers; i++)
        {
            float angle = i * Mathf.PI * 2 / markers;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

            // Верхнее кольцо
            Vector3 topEdge = top + dir * radius;
            Vector3 topGround = new Vector3(topEdge.x, 0, topEdge.z);
            Handles.DrawLine(topGround, topEdge);
            Handles.SphereHandleCap(0, topEdge, Quaternion.identity, 0.25f, EventType.Repaint);

            // Нижнее кольцо
            Vector3 bottomEdge = bottom + dir * radius;
            Vector3 bottomGround = new Vector3(bottomEdge.x, 0, bottomEdge.z);
            Handles.DrawLine(bottomGround, bottomEdge);
            Handles.SphereHandleCap(0, bottomEdge, Quaternion.identity, 0.25f, EventType.Repaint);

            // Вертикальные линии между кольцами
            Handles.DrawLine(topEdge, bottomEdge);
        }

        // 3. Рисуем линии размерности
        Handles.DrawLine(centre + Vector3.forward * radius, centre - Vector3.forward * radius);
        Handles.DrawLine(centre + Vector3.right * radius, centre - Vector3.right * radius);

        // 4. Рисуем верхнюю и нижнюю полусферы
        Handles.SphereHandleCap(0, top + Vector3.up * radius, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.SphereHandleCap(0, bottom - Vector3.up * radius, Quaternion.identity, 0.5f, EventType.Repaint);
    }

    private void VisualizeMeshCollider(MeshCollider mesh)
    {
        if (mesh.sharedMesh == null) return;

        Vector3 centre = mesh.transform.position;
        Vector3[] vertices = mesh.sharedMesh.vertices;
        int[] triangles = mesh.sharedMesh.triangles;

        // 1. Рисуем основные грани
        for (int i = 0; i < triangles.Length; i += 3)
        {
            if (i % 10 != 0) continue; // Рисуем каждую 10-ю грань

            Vector3 a = mesh.transform.TransformPoint(vertices[triangles[i]]);
            Vector3 b = mesh.transform.TransformPoint(vertices[triangles[i + 1]]);
            Vector3 c = mesh.transform.TransformPoint(vertices[triangles[i + 2]]);

            Handles.DrawLine(a, b);
            Handles.DrawLine(b, c);
            Handles.DrawLine(c, a);
        }

        // 2. Рисуем линии размерности
        Bounds bounds = mesh.bounds;
        Handles.DrawLine(centre + Vector3.forward * bounds.extents.z, centre - Vector3.forward * bounds.extents.z);
        Handles.DrawLine(centre + Vector3.right * bounds.extents.x, centre - Vector3.right * bounds.extents.x);
    }
#endif
}