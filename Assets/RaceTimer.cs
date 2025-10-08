using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class RaceTimer : MonoBehaviour
{
    [Header("Tiempo")]
    [Min(0f)] public float startSeconds = 60f;   // Tiempo total de carrera
    public bool autoStart = true;
    public bool useUnscaledTime = false;         // Si quieres que no dependa de Time.timeScale

    [Header("UI (opcional)")]
    public TextMeshProUGUI timerLabel;           // Texto mm:ss
    public Color normalColor = Color.white;
    public Color lowColor = Color.red;
    [Range(0f, 1f)] public float lowThreshold = 0.15f; // 15% del tiempo = rojo

    [Header("Refs")]
    public ShipMovement ship;                    // La nave a la que vaciamos la gasolina

    [Header("Eventos")]
    public UnityEvent onTimeUp;                  // Se llama cuando llega a 0

    private float timeLeft;
    private bool running;
    private bool finished;

    void Awake()
    {
        ResetTimer();
        if (ship == null) ship = FindObjectOfType<ShipMovement>();
    }

    void OnEnable()
    {
        if (autoStart) StartTimer();
        UpdateLabel(); // dibuja estado inicial
    }

    void Update()
    {
        if (!running || finished) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timeLeft -= dt;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            finished = true;
            running = false;

            // Vaciar combustible de la nave (sin disparar "golpe")
            if (ship != null)
            {
                ship.ConsumeFuel(ship.fuel + 99999f, invokeHitEvent: false);
            }

            onTimeUp?.Invoke();
        }

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (timerLabel == null) return;

        int seconds = Mathf.CeilToInt(timeLeft);
        int mm = Mathf.Max(0, seconds / 60);
        int ss = Mathf.Max(0, seconds % 60);
        timerLabel.text = $"{mm:00}:{ss:00}";

        // Color segun umbral
        float pct = startSeconds <= 0f ? 0f : (timeLeft / startSeconds);
        timerLabel.color = (pct <= lowThreshold) ? lowColor : normalColor;
    }

    // -------- API pública --------
    public void StartTimer()  { running = true;  finished = false; }
    public void StopTimer()   { running = false; }
    public void ResetTimer()  { timeLeft = Mathf.Max(0f, startSeconds); running = false; finished = false; UpdateLabel(); }
    public void AddTime(float sec) { timeLeft = Mathf.Max(0f, timeLeft + sec); UpdateLabel(); }
    public float TimeLeft => Mathf.Max(0f, timeLeft);
    public bool IsRunning => running;
}
