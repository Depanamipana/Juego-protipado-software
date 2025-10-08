using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class FuelDamage : MonoBehaviour
{
    [Header("Daño de combustible")]
    [Min(0f)] public float fuelLoss = 15f;     // cuánto quita este obstáculo
    public float hitCooldown = 0.2f;           // evita restar varias veces seguidas al mismo tocar
    public bool destroyOnHit = false;          // destruir este objeto tras golpear

    [Header("Filtros (opcionales)")]
    public bool requireTagOnOther = false;
    public string requiredTag = "Player";      // si usas tag del jugador

    [Header("Eventos")]
    public UnityEvent onHit;                   // evento local del obstáculo (SFX/VFX/puntos)

    private float lastHitTime = -999f;

    // TRIGGERS
    private void OnTriggerEnter2D(Collider2D other)  { TryDamage(other, null); }
    private void OnTriggerStay2D(Collider2D other)   { TryDamage(other, null); }

    // COLISIONES
    private void OnCollisionEnter2D(Collision2D c)   { TryDamage(c.collider, c.rigidbody); }
    private void OnCollisionStay2D(Collision2D c)    { TryDamage(c.collider, c.rigidbody); }

    private void TryDamage(Collider2D other, Rigidbody2D otherRb)
    {
        if (Time.time - lastHitTime < hitCooldown) return;

        if (requireTagOnOther && !other.CompareTag(requiredTag)) return;

        // ¿Este collider (o su padre/hijo) tiene la nave?
        ShipMovement ship =
            other.GetComponent<ShipMovement>() ??
            other.GetComponentInParent<ShipMovement>() ??
            other.GetComponentInChildren<ShipMovement>();

        if (ship == null) return; // no es el jugador

        lastHitTime = Time.time;

        // Quita gasolina (esto internamente maneja 0 combustible y evento onFuelEmpty)
        ship.ConsumeFuel(fuelLoss, invokeHitEvent:true);

        onHit?.Invoke();

        if (destroyOnHit)
            Destroy(gameObject);
    }
}
