using UnityEngine;

public class CamaraSeguidora : MonoBehaviour
{
    public Transform objetivo;
    public float desfaseX = 4f;      // espacio que se ve por delante de Nina
    public float suavizado = 0.15f;
    public float alturaFija = 0f;    // la camara no sube ni baja con los saltos

    private float velocidadRef;

    void LateUpdate()
    {
        if (objetivo == null) return;

        float destinoX = objetivo.position.x + desfaseX;
        float nuevaX = Mathf.SmoothDamp(transform.position.x, destinoX,
                                        ref velocidadRef, suavizado);

        transform.position = new Vector3(nuevaX, alturaFija, -10f);
    }
}