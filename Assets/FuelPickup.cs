using UnityEngine;
using UnityEngine.Events;

public class FuelPickup : MonoBehaviour
{
    [Header("Recarga")]
    [Min(0f)] public float amount = 25f;
    public bool destroyOnPickup = true;

    [Header("Feedback (opcional)")]
    public ParticleSystem pickupVFX;
    public AudioClip pickupSFX;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Tooltip("Evento cuando se recoge (útil para sumar puntaje, UI, etc.)")]
    public UnityEvent onPicked;

    private bool consumed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        // Buscamos el ShipMovement en el objeto que entró o en su padre
        var ship = other.GetComponentInParent<ShipMovement>();
        if (ship == null) return;

        // Recarga (ShipMovement.Refuel ya hace Clamp a maxFuel)
        float before = ship.fuel;
        ship.Refuel(amount);

        // Si realmente aportó algo de combustible, disparamos feedback
        if (ship.fuel > before - 0.001f)
        {
            consumed = true;
            onPicked?.Invoke();

            if (pickupVFX != null)
            {
                // Instancia VFX en el lugar del pickup
                var vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
                // Si el PS no tiene StopAction=Destroy, elimina manualmente:
                Destroy(vfx.gameObject, vfx.main.duration + vfx.main.startLifetime.constantMax + 0.1f);
            }

            if (pickupSFX != null)
            {
                // Reproduce en la posición del pickup (2D/3D según tu audio)
                AudioSource.PlayClipAtPoint(pickupSFX, transform.position, sfxVolume);
            }

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}
