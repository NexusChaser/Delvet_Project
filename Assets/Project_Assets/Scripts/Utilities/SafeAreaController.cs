using UnityEngine;

[ExecuteAlways]
public class SafeAreaController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform area;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2 lastScreenSize = new Vector2(0, 0);

    void Awake()
    {
        CheckIfAreaExist();
        
        Refresh();
    }

    void LateUpdate()
    {
        if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            Refresh();
        }
    }

    private void CheckIfAreaExist() 
    {
        if (area == null)
        {
            area = GetComponent<RectTransform>();
        }

        if (area == null)
        {
            return;
        }
    }

    void Refresh()
    {
        CheckIfAreaExist();

        Rect safeArea = Screen.safeArea;

        // Guardamos los estados actuales para la siguiente comparación
        lastSafeArea = safeArea;
        lastScreenSize.x = Screen.width;
        lastScreenSize.y = Screen.height;

        // Convertir de pixeles a coordenadas normalizadas (0 a 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        // Evitar división por cero en el editor si la ventana de Game es muy pequeña o no ha cargado
        if (Screen.width > 0 && Screen.height > 0)
        {
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Aplicar los anchors
            area.anchorMin = anchorMin;
            area.anchorMax = anchorMax;
        }
    }
}
