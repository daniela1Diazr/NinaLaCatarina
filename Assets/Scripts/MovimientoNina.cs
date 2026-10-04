using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovimientoNina : MonoBehaviour
{
    [Header("Carrera")]
    public float velocidadCarrera = 3.5f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float multiplicadorCaida = 2.5f;
    public float multiplicadorSaltoCorto = 4f;

    [Header("Deteccion de suelo")]
    public Transform pies;
    public float radioDeteccion = 0.2f;
    public LayerMask capaSuelo;

    [Header("Ayudas al jugador")]
    public float tiempoCoyote = 0.1f;
    public float bufferSalto = 0.1f;

    [Header("Solo para ver (no tocar)")]
    [SerializeField] private bool enSuelo;
    public bool EnSuelo => enSuelo;

    private Rigidbody2D rb;
    private float contadorCoyote;
    private float contadorBuffer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (pies == null)
            Debug.LogError("MovimientoNina: falta arrastrar el objeto 'Pies' al campo Pies.", this);
        if (capaSuelo.value == 0)
            Debug.LogWarning("MovimientoNina: 'Capa Suelo' esta vacia, Nina nunca detectara el suelo.", this);
    }

    void Update()
    {
        if (pies == null) return;

        enSuelo = Physics2D.OverlapCircle(pies.position, radioDeteccion, capaSuelo);

        if (enSuelo) contadorCoyote = tiempoCoyote;
        else contadorCoyote -= Time.deltaTime;

        if (SaltoPresionado()) contadorBuffer = bufferSalto;
        else contadorBuffer -= Time.deltaTime;

        if (contadorBuffer > 0f && contadorCoyote > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            contadorBuffer = 0f;
            contadorCoyote = 0f;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(velocidadCarrera, rb.linearVelocity.y);

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                                 (multiplicadorCaida - 1f) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0f && !SaltoMantenido())
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y *
                                 (multiplicadorSaltoCorto - 1f) * Time.fixedDeltaTime;
        }
    }

    bool SaltoPresionado()
    {
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.UpArrow)
            || Input.GetKeyDown(KeyCode.W)
            || Input.GetMouseButtonDown(0);
    }

    bool SaltoMantenido()
    {
        return Input.GetKey(KeyCode.Space)
            || Input.GetKey(KeyCode.UpArrow)
            || Input.GetKey(KeyCode.W)
            || Input.GetMouseButton(0);
    }

    void OnDrawGizmosSelected()
    {
        if (pies == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pies.position, radioDeteccion);
    }
}