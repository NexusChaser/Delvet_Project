using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// CinematicManager
/// -----------------
/// Responsabilidad única: coordinar el FLUJO de una cinemática, no reproducirla.
///
/// Este manager NO sabe qué es una cinemática (video, Timeline, animación, etc.).
/// Solo conoce dos datos:
///   1) Qué cinemática se debe reproducir (un identificador genérico).
///   2) A qué escena ir una vez que la cinemática termine.
///
/// El flujo esperado es:
///   1. Desde cualquier escena, se llama a CinematicManager.Instance.PlayCinematic(id, nextScene).
///   2. Esto guarda los datos y carga la escena "CinematicScene".
///   3. Dentro de "CinematicScene", otro sistema (ej. un CinematicPlayer o CinematicResolver)
///      lee CinematicManager.Instance.CinematicId y decide QUÉ reproducir y CÓMO.
///   4. Cuando ese sistema termina, es su responsabilidad cargar la escena guardada
///      en CinematicManager.Instance.NextScene.
///
/// De esta forma, el Manager queda completamente desacoplado de la implementación
/// concreta de cada cinemática, cumpliendo SRP y quedando abierto a extensión (OCP).
/// </summary>
public class CinematicManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // SINGLETON
    // ---------------------------------------------------------------
    private static CinematicManager _instance;

    /// <summary>
    /// Acceso global y seguro al Singleton. Si no existe, no se crea aquí:
    /// se asume que el manager vive en una escena inicial/bootstrap.
    /// Esto evita instancias "fantasma" creadas por accidente.
    /// </summary>
    public static CinematicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogWarning("[CinematicManager] No existe una instancia en la escena. " +
                                 "Asegúrate de colocarlo en una escena de arranque (bootstrap).");
            }
            return _instance;
        }
    }

    // ---------------------------------------------------------------
    // DATOS ALMACENADOS (estado del flujo, no de la cinemática en sí)
    // ---------------------------------------------------------------

    /// <summary>
    /// Identificador/nombre de la cinemática a reproducir.
    /// Es un dato genérico (string) a propósito: el Manager no conoce
    /// las cinemáticas concretas que existen en el proyecto.
    /// </summary>
    public string CinematicId { get; private set; }

    /// <summary>
    /// Nombre de la escena a la que se debe volver/avanzar
    /// una vez finalizada la cinemática.
    /// </summary>
    public string NextScene { get; private set; }

    // Nombre fijo de la escena intermedia donde se resuelven las cinemáticas.
    private const string CinematicSceneName = "CinematicScene";

    // ---------------------------------------------------------------
    // CICLO DE VIDA
    // ---------------------------------------------------------------

    private void Awake()
    {
        // Patrón Singleton persistente clásico.
        if (_instance != null && _instance != this)
        {
            // Ya existe una instancia (por ejemplo, venimos de otra escena
            // donde este mismo prefab/objeto también estaba presente).
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------------------------------------------------------
    // API PÚBLICA
    // ---------------------------------------------------------------

    /// <summary>
    /// Punto de entrada único para iniciar el flujo de una cinemática.
    /// Únicamente:
    ///   1) Guarda los datos necesarios para que otro sistema los use después.
    ///   2) Carga la escena "CinematicScene".
    ///
    /// No reproduce nada, no muestra UI, no maneja skip.
    /// Toda esa lógica pertenece a otros componentes que se construirán
    /// como piezas separadas y desacopladas de este manager.
    /// </summary>
    /// <param name="cinematicId">Identificador de la cinemática a reproducir.</param>
    /// <param name="nextScene">Escena a cargar cuando la cinemática finalice.</param>
    public void PlayCinematic(string cinematicId, string nextScene)
    {
        if (string.IsNullOrEmpty(cinematicId))
        {
            Debug.LogError("[CinematicManager] cinematicId no puede ser nulo o vacío.");
            return;
        }

        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("[CinematicManager] nextScene no puede ser nulo o vacío.");
            return;
        }

        // 1. Guardar la información del flujo.
        CinematicId = cinematicId;
        NextScene = nextScene;

        // 2. Cargar la escena donde se resolverá qué cinemática reproducir.
        SceneManager.LoadScene(CinematicSceneName);
    }
}