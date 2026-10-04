using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimacionNina : MonoBehaviour
{
    private Animator animador;
    private MovimientoNina movimiento;

    void Awake()
    {
        animador = GetComponent<Animator>();
        movimiento = GetComponent<MovimientoNina>();
    }

    void Update()
    {
        animador.SetBool("enSuelo", movimiento.EnSuelo);
    }

    public void Morir()
    {
        animador.SetTrigger("morir");
    }
}