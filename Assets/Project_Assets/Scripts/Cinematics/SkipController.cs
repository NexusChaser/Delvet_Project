using UnityEngine;

/// <summary>
/// SkipController
/// ----------------
/// Responsabilidad única: detectar la intención del jugador de saltar
/// la cinemática actual y solicitar su finalización.
///
/// No decide qué significa "terminar" la cinemática (eso es responsabilidad
/// de CinematicPlayer), no conoce Timeline ni VideoPlayer, y no tiene
/// ningún vínculo con CinematicManager. Su única dependencia es
/// CinematicPlayer.FinishCinematic().
/// </summary>
public class SkipController : MonoBehaviour
{
    [Header("Configuración de Skip")]

    [Tooltip("Tecla que dispara el skip de la cinemática.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Escape;

    [Tooltip("Habilita o deshabilita la posibilidad de hacer skip.")]
    public bool allowSkip = true;

    [Header("Referencias")]

    [Tooltip("Referencia al CinematicPlayer de la escena. " +
             "Si se deja vacío, se buscará automáticamente en Awake.")]
    [SerializeField] private CinematicPlayer cinematicPlayer;

    private void Awake()
    {
        // Si no fue asignado desde el Inspector, se busca automáticamente
        // en la escena. Esto mantiene el componente fácil de usar sin
        // configuración manual obligatoria.
        if (cinematicPlayer == null)
        {
            cinematicPlayer = FindObjectOfType<CinematicPlayer>();
        }

        if (cinematicPlayer == null)
        {
            Debug.LogError("[SkipController] No se encontró un CinematicPlayer en la escena. " +
                            "El skip no funcionará hasta que exista uno.");
        }
    }

    private void Update()
    {
        if (!allowSkip)
        {
            return;
        }

        if (Input.GetKeyDown(skipKey))
        {
            RequestSkip();
        }
    }

    /// <summary>
    /// Solicita la finalización de la cinemática actual.
    /// No carga escenas ni maneja ningún otro efecto: delega
    /// completamente la responsabilidad a CinematicPlayer.
    /// </summary>
    private void RequestSkip()
    {
        if (cinematicPlayer == null)
        {
            Debug.LogError("[SkipController] No es posible hacer skip: no hay referencia a CinematicPlayer.");
            return;
        }

        cinematicPlayer.FinishCinematic();
    }
}