using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// CinematicPlayer
/// ----------------
/// Responsabilidad única: vivir dentro de "CinematicScene" y actuar como
/// punto de entrada/salida del flujo de reproducción de una cinemática.
///
/// Su trabajo, en esta etapa, es:
///   1) Leer el CinematicId guardado en CinematicManager (informativo por ahora).
///   2) Reproducir el video mediante el componente VideoPlayer de la escena.
///   3) Suscribirse a VideoPlayer.loopPointReached para detectar el fin del video.
///   4) Exponer FinishCinematic(), el método que se invoca cuando la cinemática
///      termina (ya sea porque el video terminó, o porque otro sistema como
///      SkipController lo solicitó).
///
/// No es Singleton: vive y muere junto con "CinematicScene".
/// No conoce UI ni lógica de Skip: SkipController le habla a este script
/// desde afuera, sin que este necesite saber que existe.
/// </summary>
public class CinematicPlayer : MonoBehaviour
{
    [Header("Referencias")]

    [Tooltip("VideoPlayer que reproducirá la cinemática. " +
             "Si se deja vacío, se buscará automáticamente en Awake.")]
    [SerializeField] private VideoPlayer videoPlayer;

    private void Awake()
    {
        // Si no fue asignado desde el Inspector, se busca automáticamente
        // en la escena, igual que hace SkipController con CinematicPlayer.
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            Debug.LogError("[CinematicPlayer] No se encontró un VideoPlayer en la escena. " +
                            "No será posible reproducir la cinemática.");
            return;
        }

        // Nos suscribimos al evento que Unity dispara cuando el video termina.
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        // Buena práctica: desuscribirse para evitar referencias colgantes
        // si este objeto es destruido antes que el VideoPlayer.
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void Start()
    {
        PlayCinematic();
    }

    /// <summary>
    /// Punto de inicio del flujo dentro de esta escena.
    /// Obtiene el identificador desde el CinematicManager (por ahora solo
    /// informativo, todavía no se usa para elegir un video distinto) y
    /// dispara la reproducción del VideoPlayer.
    ///
    /// En el futuro, este método será el lugar donde se resuelva el
    /// CinematicId contra un sistema real (por ejemplo, un CinematicDatabase
    /// que devuelva el VideoClip correspondiente antes de reproducir).
    /// </summary>
    private void PlayCinematic()
    {
        if (CinematicManager.Instance == null)
        {
            Debug.LogError("[CinematicPlayer] No se encontró una instancia de CinematicManager. " +
                            "No es posible determinar qué cinemática reproducir.");
            return;
        }

        string cinematicId = CinematicManager.Instance.CinematicId;

        if (string.IsNullOrEmpty(cinematicId))
        {
            Debug.LogWarning("[CinematicPlayer] CinematicId está vacío. " +
                              "¿Se llegó a esta escena sin pasar por CinematicManager.PlayCinematic()?");
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogError("[CinematicPlayer] No hay VideoPlayer asignado. No se puede reproducir.");
            return;
        }

        Debug.Log($"[CinematicPlayer] Reproduciendo cinemática: \"{cinematicId}\".");
        videoPlayer.Play();
    }

    /// <summary>
    /// Callback suscrito a VideoPlayer.loopPointReached.
    /// Se dispara automáticamente cuando el video llega a su fin.
    /// Simplemente delega en FinishCinematic(), el mismo punto de salida
    /// que usaría SkipController u otro sistema.
    /// </summary>
    /// <param name="source">VideoPlayer que disparó el evento.</param>
    private void OnVideoFinished(VideoPlayer source)
    {
        FinishCinematic();
    }

    /// <summary>
    /// Punto de salida único del flujo de esta cinemática.
    ///
    /// Debe ser llamado por el sistema real de reproducción una vez
    /// que la cinemática haya terminado (ej: al finalizar un Timeline,
    /// al terminar un VideoPlayer, o al completarse una animación).
    ///
    /// CinematicPlayer no necesita saber CÓMO terminó la cinemática,
    /// solo reacciona al hecho de que terminó, cargando la escena
    /// que fue guardada previamente en CinematicManager.
    /// </summary>
    public void FinishCinematic()
    {
        if (CinematicManager.Instance == null)
        {
            Debug.LogError("[CinematicPlayer] No se encontró una instancia de CinematicManager. " +
                            "No es posible determinar la siguiente escena.");
            return;
        }

        string nextScene = CinematicManager.Instance.NextScene;

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[CinematicPlayer] NextScene está vacío. " +
                            "No se puede continuar el flujo.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }
}