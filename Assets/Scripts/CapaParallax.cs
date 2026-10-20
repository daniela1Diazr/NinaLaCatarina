using UnityEngine;

// Pon este script en cada capa de fondo (un GameObject con SpriteRenderer).
// Crea copias a los lados para que el fondo se repita sin fin y lo mueve
// mas lento que el mundo para dar sensacion de profundidad.
[RequireComponent(typeof(SpriteRenderer))]
public class CapaParallax : MonoBehaviour
{
    public enum Anclaje { BordeInferior, BordeSuperior }

    [Header("Parallax")]
    [Tooltip("0 = pegado a la camara (no se mueve). 1 = se mueve igual que el suelo.")]
    [Range(0f, 1f)] public float factor = 0.3f;

    [Header("Posicion vertical")]
    public Anclaje anclaje = Anclaje.BordeInferior;
    [Tooltip("Sube (+) o baja (-) la capa, en unidades de Unity.")]
    public float desplazamientoY = 0f;

    [Header("Pixel art")]
    [Tooltip("Debe coincidir con el Pixels Per Unit del fondo (tu terreno usa 32).")]
    public float pixelesPorUnidad = 32f;

    private Camera camara;
    private float anchoCapa;
    private float altoCapa;

    void Start()
    {
        camara = Camera.main;
        if (camara == null)
        {
            Debug.LogError("CapaParallax: no encuentro la camara principal (Main Camera).", this);
            enabled = false;
            return;
        }

        SpriteRenderer original = GetComponent<SpriteRenderer>();
        if (original.sprite == null)
        {
            Debug.LogError("CapaParallax: el SpriteRenderer no tiene sprite.", this);
            enabled = false;
            return;
        }

        anchoCapa = original.sprite.bounds.size.x * transform.lossyScale.x;
        altoCapa = original.sprite.bounds.size.y * transform.lossyScale.y;

        // Cuantas copias hacen falta a cada lado para cubrir la pantalla
        float anchoCamara = camara.orthographicSize * 2f * camara.aspect;
        int copiasPorLado = Mathf.CeilToInt(anchoCamara / anchoCapa) + 1;

        for (int i = -copiasPorLado; i <= copiasPorLado; i++)
        {
            if (i == 0) continue;

            GameObject copia = new GameObject(name + "_copia" + i);
            copia.transform.SetParent(transform, false);
            copia.transform.localPosition = new Vector3(i * original.sprite.bounds.size.x, 0f, 0f);

            SpriteRenderer sr = copia.AddComponent<SpriteRenderer>();
            sr.sprite = original.sprite;
            sr.color = original.color;
            sr.sharedMaterial = original.sharedMaterial;
            sr.sortingLayerID = original.sortingLayerID;
            sr.sortingOrder = original.sortingOrder;
        }
    }

    void LateUpdate()
    {
        float camX = camara.transform.position.x;

        // Cuanto se ha "corrido" el contenido de la capa respecto a la camara
        float corrimiento = Mathf.Repeat(camX * factor, anchoCapa);
        float x = camX - corrimiento;

        float mitadAlto = camara.orthographicSize;
        float y = (anclaje == Anclaje.BordeInferior)
            ? camara.transform.position.y - mitadAlto + altoCapa * 0.5f
            : camara.transform.position.y + mitadAlto - altoCapa * 0.5f;
        y += desplazamientoY;

        // Redondeo al pixel para que el pixel art no "tiemble"
        if (pixelesPorUnidad > 0f)
        {
            x = Mathf.Round(x * pixelesPorUnidad) / pixelesPorUnidad;
            y = Mathf.Round(y * pixelesPorUnidad) / pixelesPorUnidad;
        }

        transform.position = new Vector3(x, y, transform.position.z);
    }
}