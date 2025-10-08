using UnityEngine;
using UnityEngine.Events;

public class ShipMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float acceleration = 8f;
    public float maxSpeed = 12f;
    public float friction = 4f;
    public float rotationSpeed = 200f;

    [Header("Combustible")]
    public float maxFuel = 100f;
    public float fuel = 100f;
    public float drainPerSecond = 1.5f;   // consumo pasivo por tiempo
    public bool stopImmediatelyOnEmpty = true;
    public UnityEvent onFuelEmpty;        // evento al vaciar
    public UnityEvent onFuelHit;          // evento global cuando “te pegan” (opcional para SFX/UI)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool outOfFuel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // Si tu versión no tiene linearDamping/linearVelocity, usa rb.drag y rb.velocity.
        rb.linearDamping = 0; // controlamos fricción manual
    }

    void Update()
    {
        // Consumo por tiempo (no dispara onFuelHit para no spamear)
        if (!outOfFuel && fuel > 0f && drainPerSecond > 0f)
            ConsumeFuel(drainPerSecond * Time.deltaTime, invokeHitEvent:false);

        // Leer input sólo si hay combustible
        if (!outOfFuel)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;
        }
        else
        {
            moveInput = Vector2.zero;
        }
    }

    void FixedUpdate()
    {
        if (outOfFuel)
        {
            if (!stopImmediatelyOnEmpty)
            {
                currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, friction * Time.fixedDeltaTime);
                rb.linearVelocity = currentVelocity; // usa rb.velocity si tu API no tiene linearVelocity
            }
            return;
        }

        // Movimiento con inercia tipo Asteroids
        if (moveInput.sqrMagnitude > 0.01f)
        {
            currentVelocity += moveInput * acceleration * Time.fixedDeltaTime;
            currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);

            float angle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            rb.MoveRotation(Quaternion.RotateTowards(rb.transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, friction * Time.fixedDeltaTime);
        }

        rb.linearVelocity = currentVelocity; // usa rb.velocity si tu API no tiene linearVelocity
    }

    // =========================
    //  API pública de combustible
    // =========================
    public void ConsumeFuel(float amount, bool invokeHitEvent = true)
    {
        if (outOfFuel || amount <= 0f) return;

        fuel = Mathf.Max(0f, fuel - amount);

        if (invokeHitEvent) onFuelHit?.Invoke();

        if (fuel <= 0f)
        {
            fuel = 0f;
            outOfFuel = true;

            if (stopImmediatelyOnEmpty)
            {
                currentVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero; // usa rb.velocity si no existe linearVelocity
            }

            onFuelEmpty?.Invoke();
        }
    }

    public float FuelPercent => maxFuel <= 0f ? 0f : fuel / maxFuel;
    public Vector2 MoveInput => moveInput;
    public Vector2 Velocity => currentVelocity;
    public float Speed01 => Mathf.Clamp01(currentVelocity.magnitude / Mathf.Max(0.01f, maxSpeed));

    public void Refuel(float amount)
    {
        if (amount <= 0f) return;
        bool wasEmpty = (fuel <= 0f);
        fuel = Mathf.Clamp(fuel + amount, 0f, maxFuel);
        if (fuel > 0f && wasEmpty) outOfFuel = false;
    }
}
