using UnityEngine;

public class MuerteNina : MonoBehaviour
{
    [Header("Margenes (unidades fuera de la pantalla)")]
    public float margenIzquierdo = 1f;
    public float margenAbajo = 2f;

    private Camera camara;
    private MovimientoNina movimiento;
    private Rigidbody2D rb;
    private bool muerta;

    void Awake()
    {
        movimiento = GetComponent<MovimientoNina>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        camara = Camera.main;
    }

    void Update()
    {
        if (muerta) return;

        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;

        float bordeIzquierdo = camara.transform.position.x - mitadAncho;
        float bordeInferior = camara.transform.position.y - mitadAlto;

        if (transform.position.x < bordeIzquierdo - margenIzquierdo)
            Morir("quedo fuera de la camara");
        else if (transform.position.y < bordeInferior - margenAbajo)
            Morir("cayo al vacio");
    }

    void Morir(string motivo)
    {
        muerta = true;
        Debug.Log("Nina murio: " + motivo);
        AnimacionNina anim = GetComponent<AnimacionNina>();
        if (anim != null) anim.Morir();

        movimiento.enabled = false;        // deja de correr y de saltar
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;              // congela la fisica
    }
}
