using UnityEngine;

public class CinematicTestLauncher : MonoBehaviour
{
    [Header("Configuración de prueba")]
    [SerializeField] private string cinematicId = "VideoPrueba";
    [SerializeField] private string nextScene = "TestRoom";

    private void Start()
    {
        if (CinematicManager.Instance == null)
        {
            Debug.LogError("[CinematicTestLauncher] No se encontró CinematicManager.");
            return;
        }

        CinematicManager.Instance.PlayCinematic(cinematicId, nextScene);
    }
}