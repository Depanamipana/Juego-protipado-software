using UnityEngine;
using UnityEngine.Events;

public class ShipMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float acceleration = 8f;        // Qué tan rápido acelera
    public float maxSpeed = 12f;           // Velocidad máxima
    public float friction = 4f;            // Frenado al soltar
    public float rotationSpeed = 200f;     // Giro hacia la dirección

    [Header("Combustible")]
    public float maxFuel = 100f;
    public float fuel = 100f;              // Nivel actual
    public float drainPerSecond = 1.5f;    // Consumo por segundo (tiempo)
    public bool stopImmediatelyOnEmpty = true; // true: se detiene en seco al vaciar
    public UnityEvent onFuelEmpty;         // Evento al quedarse sin combustible

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool outOfFuel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0; // controlamos fricción manualmente
    }

    void Update()
    {
        // Drenaje de combustible por tiempo
        if (!outOfFuel && fuel > 0f)
        {
            fuel = Mathf.Max(0f, fuel - drainPerSecond * Time.deltaTime);
            if (fuel <= 0f)
            {
                fuel = 0f;
                outOfFuel = true;

                if (stopImmediatelyOnEmpty)
                {
                    currentVelocity = Vector2.zero;
                    rb.linearVelocity = Vector2.zero;
                }

                onFuelEmpty?.Invoke();
            }
        }

        // Leer input SOLO si hay combustible
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
            // Si no detienes en seco, deja que se frene suave por fricción
            if (!stopImmediatelyOnEmpty)
            {
                currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, friction * Time.fixedDeltaTime);
                rb.linearVelocity = currentVelocity;
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

        rb.linearVelocity = currentVelocity;
    }

    // Utilidades para HUD y pickups
    public float FuelPercent => maxFuel <= 0f ? 0f : fuel / maxFuel;

    public void Refuel(float amount)
    {
        if (amount <= 0f) return;
        bool wasEmpty = outOfFuel && fuel <= 0f;
        fuel = Mathf.Clamp(fuel + amount, 0f, maxFuel);
        if (fuel > 0f && wasEmpty)
            outOfFuel = false; // vuelve a permitir movimiento tras recargar
    }
}
