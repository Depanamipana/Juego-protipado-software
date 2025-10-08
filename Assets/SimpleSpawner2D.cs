using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SimpleSpawner2D : MonoBehaviour
{
    [Header("Qué spawnear")]
    public GameObject prefab;                   // Objeto a instanciar (FuelPickup u Obstáculo con FuelDamage)
    public int maxAlive = 10;                   // Máximo simultáneo
    public int initialSpawn = 5;                // Sembrado inicial al iniciar
    public float spawnInterval = 2f;            // Intervalo entre intentos de spawn
    public bool randomRotation = false;         // Rotación aleatoria Z

    [Header("Reglas/colocación")]
    public Transform player;                    // Para no aparecer encima del jugador
    public float minDistanceFromPlayer = 3f;    // Distancia mínima al jugador
    public float avoidOverlapRadius = 0.5f;     // Chequeo de superposición
    public LayerMask overlapMask;               // Capas a evitar (obstáculos, bordes, player, etc.)
    public int maxPlacementAttempts = 20;       // Intentos por spawn para encontrar hueco

    [Header("Debug")]
    public bool drawAreaGizmo = true;

    private BoxCollider2D area;                 // Área de spawn (puede estar rotada)
    private readonly List<GameObject> alive = new List<GameObject>();
    private float timer;

    void Awake()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true; // solo usamos el volumen; no colisiona
        if (player == null && Camera.main != null)
        {
            // Si no asignas el player, lo intentamos encontrar
            var ship = FindObjectOfType<ShipMovement>();
            if (ship != null) player = ship.transform;
        }
    }

    void OnEnable()
    {
        // Sembrado inicial
        for (int i = 0; i < initialSpawn; i++)
            TrySpawnOne();
        timer = spawnInterval;
    }

    void Update()
    {
        // Limpia referencias nulas en la lista (objetos recogidos/destruidos)
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            if (alive[i] == null)
                alive.RemoveAt(i);
            else if (!alive[i].activeInHierarchy)
                alive.RemoveAt(i);
        }

        // Controla el máximo y el intervalo
        if (alive.Count >= maxAlive || prefab == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (TrySpawnOne())
                timer = spawnInterval;
            else
                // si no encontramos hueco, reintenta pronto
                timer = Mathf.Min(spawnInterval, 0.25f);
        }
    }

    bool TrySpawnOne()
    {
        if (prefab == null) return false;

        for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
        {
            Vector2 point = RandomPointInBox(area);

            // Evita aparición encima del jugador
            if (player != null && Vector2.Distance(point, player.position) < minDistanceFromPlayer)
                continue;

            // Evita solapar con otros colliders
            if (avoidOverlapRadius > 0f && overlapMask.value != 0)
            {
                var hit = Physics2D.OverlapCircle(point, avoidOverlapRadius, overlapMask);
                if (hit != null) continue;
            }

            Quaternion rot = randomRotation ? Quaternion.Euler(0, 0, Random.Range(0f, 360f)) : Quaternion.identity;
            GameObject go = Instantiate(prefab, point, rot);
            alive.Add(go);
            return true;
        }
        return false; // no encontró lugar
    }

    Vector2 RandomPointInBox(BoxCollider2D box)
    {
        // calcula punto aleatorio respetando offset, escala y rotación del collider
        Vector2 size = Vector2.Scale(box.size, box.transform.lossyScale);
        Vector2 local = new Vector2(Random.Range(-size.x * 0.5f, size.x * 0.5f),
                                    Random.Range(-size.y * 0.5f, size.y * 0.5f));
        // aplica offset local del collider
        local += box.offset;
        return box.transform.TransformPoint(local);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawAreaGizmo) return;
        var box = GetComponent<BoxCollider2D>();
        if (!box) return;

        Gizmos.color = new Color(0f, 1f, 0.6f, 0.2f);
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(box.transform.TransformPoint(box.offset), box.transform.rotation, box.transform.lossyScale);
        Gizmos.DrawCube(Vector3.zero, new Vector3(box.size.x, box.size.y, 0.05f));
        Gizmos.matrix = prev;
    }
}

