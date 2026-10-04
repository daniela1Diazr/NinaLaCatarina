using UnityEngine;

public class CamaraAutomatica : MonoBehaviour
{
    public MovimientoNina nina;     // de aqui lee la velocidad
    public float alturaFija = 0f;

    void Start()
    {
        if (nina == null)
        {
            Debug.LogError("CamaraAutomatica: falta arrastrar a Nina al campo Nina.", this);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        // Si Nina murio, la camara se detiene
        if (!nina.enabled) return;

        float nuevaX = transform.position.x + nina.velocidadCarrera * Time.deltaTime;
        transform.position = new Vector3(nuevaX, alturaFija, -10f);
    }
}