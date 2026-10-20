using System.Collections;
using UnityEngine;

// Muerte estilo Mario: Nina se desactiva como jugadora, atraviesa el suelo,
// da un salto hacia arriba y cae fuera de la pantalla. Al terminar avisa al GestorJuego.
public class MuerteNina : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Cuanto puede salirse por la izquierda de la camara antes de morir.")]
    public float margenIzquierdo = 0.5f;
    [Tooltip("Cuanto puede bajar de la camara antes de morir.")]
    public float margenInferior = 1f;

    [Header("Animacion estilo Mario")]
    [Tooltip("Pausa con la pose de muerte antes del salto (solo si muere dentro de la pantalla).")]
    public float pausaAntesDelSalto = 0.5f;
    [Tooltip("Altura del salto de muerte, en unidades de Unity.")]
    public float alturaDelSalto = 3f;
    [Tooltip("Que tan fuerte tira hacia abajo durante la caida final.")]
    public float gravedad = 25f;
    [Tooltip("Si cae al vacio, reaparece por el borde inferior y hace el salto desde ahi.")]
    public bool salirDesdeDondeCayo = true;

    [Header("Fin del juego")]
    [Tooltip("Espera despues de que Nina sale de la pantalla, antes de mostrar el panel.")]
    public float esperaAntesDelPanel = 0.4f;

    private MovimientoNina movimiento;
    private AnimacionNina animacion;
    private Rigidbody2D rb;
    private SpriteRenderer[] sprites;
    private Camera camara;
    private bool muerta = false;

    void Start()
    {
        camara = Camera.main;
        movimiento = GetComponent<MovimientoNina>();
        animacion = GetComponent<AnimacionNina>();
        rb = GetComponent<Rigidbody2D>();
        sprites = GetComponentsInChildren<SpriteRenderer>();

        if (camara == null)
        {
            Debug.LogError("MuerteNina: no encuentro la camara principal.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (muerta) return;

        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;
        Vector3 cam = camara.transform.position;

        bool salioPorIzquierda = transform.position.x < cam.x - mitadAncho - margenIzquierdo;
        bool cayoAlVacio = transform.position.y < cam.y - mitadAlto - margenInferior;

        if (salioPorIzquierda || cayoAlVacio)
            StartCoroutine(Morir(cayoAlVacio));
    }

    IEnumerator Morir(bool cayoAlVacio)
    {
        muerta = true;

        // Avisa al marcador para que congele la distancia
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.AlMorir();

        // 1) Apagar el control y la fisica (la camara se detiene porque MovimientoNina queda desactivado)
        if (movimiento != null) movimiento.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 2) Sin colisiones: atraviesa el suelo como Mario
        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        // 3) Dibujarla por delante de todo
        foreach (SpriteRenderer s in sprites)
            s.sortingOrder = 100;

        // 4) Pose de muerte
        if (animacion != null) animacion.Morir();

        float mitadAlto = camara.orthographicSize;
        float bordeInferior = camara.transform.position.y - mitadAlto;
        float altoSprite = sprites.Length > 0 ? sprites[0].bounds.size.y : 1f;
        float yFueraAbajo = bordeInferior - altoSprite - 0.5f;

        if (cayoAlVacio && salirDesdeDondeCayo)
        {
            // Reaparece justo debajo del borde inferior, en la misma X donde cayo
            transform.position = new Vector3(transform.position.x, yFueraAbajo, transform.position.z);
        }
        else if (!cayoAlVacio)
        {
            // Murio dentro de la pantalla: se queda quieta un instante, como Mario
            yield return new WaitForSeconds(pausaAntesDelSalto);
        }

        // 5) Salto hacia arriba y caida hasta salir de la pantalla
        float velocidad = Mathf.Sqrt(2f * gravedad * alturaDelSalto);

        while (transform.position.y > yFueraAbajo - 1f)
        {
            velocidad -= gravedad * Time.deltaTime;
            transform.position += new Vector3(0f, velocidad * Time.deltaTime, 0f);
            yield return null;
        }

        // 6) Mostrar la pantalla de fin de juego
        yield return new WaitForSeconds(esperaAntesDelPanel);
        if (GestorJuego.Instancia != null) GestorJuego.Instancia.MostrarFinDeJuego();
    }
}