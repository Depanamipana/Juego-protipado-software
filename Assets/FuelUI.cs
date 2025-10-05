using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ShipMovement ship;   // tu script de la nave
    [SerializeField] private Slider fuelSlider;   // Slider [0..1]
    [SerializeField] private TextMeshProUGUI fuelText; // Texto opcional
    [SerializeField] private Image fillImage;     // La Image del Fill del slider

    [Header("Estética")]
    [SerializeField] private Color fullColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color midColor  = new Color(1f,   0.85f, 0.2f);
    [SerializeField] private Color lowColor  = new Color(1f,   0.25f, 0.2f);
    [SerializeField, Range(0f, 1f)] private float lowThreshold = 0.2f;
    [SerializeField] private bool blinkWhenLow = true; // parpadeo al estar en reserva

    private void Reset()
    {
        if (ship == null) ship = FindObjectOfType<ShipMovement>();
        if (fuelSlider == null) fuelSlider = GetComponentInChildren<Slider>(true);
        if (fuelSlider != null && fillImage == null && fuelSlider.fillRect != null)
            fillImage = fuelSlider.fillRect.GetComponent<Image>();
        if (fuelText == null) fuelText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void OnEnable()
    {
        if (ship == null) ship = FindObjectOfType<ShipMovement>();
        if (ship != null) ship.onFuelEmpty.AddListener(OnFuelEmpty);
    }

    private void OnDisable()
    {
        if (ship != null) ship.onFuelEmpty.RemoveListener(OnFuelEmpty);
    }

    private void Update()
    {
        if (ship == null || fuelSlider == null) return;

        float p = Mathf.Clamp01(ship.FuelPercent);
        fuelSlider.value = p;

        if (fuelText != null)
        {
            // Muestra porcentaje o unidades. Elige el formato que te guste.
            // fuelText.text = $"FUEL {Mathf.RoundToInt(p * 100f)}%";
            fuelText.text = $"FUEL {Mathf.CeilToInt(ship.fuel)}/{Mathf.CeilToInt(ship.maxFuel)}";
        }

        if (fillImage != null)
        {
            // Verde → Amarillo → Rojo + parpadeo si está bajo el umbral
            Color target = (p < lowThreshold) ? lowColor : (p < 0.6f ? midColor : fullColor);

            if (blinkWhenLow && p < lowThreshold)
            {
                float blink = Mathf.PingPong(Time.unscaledTime * 4f, 1f); // parpadeo ~4Hz
                target = Color.Lerp(target * 0.4f, target, blink);
            }

            fillImage.color = target;
        }
    }

    private void OnFuelEmpty()
    {
        // Mensaje rápido al vaciar (opcional)
        if (fuelText != null) fuelText.text += " (VACÍO)";
    }
}
