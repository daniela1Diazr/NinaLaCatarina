using System.Collections.Generic;
using UnityEngine;

public class GeneradorDeChunks : MonoBehaviour
{
    [Header("Configuracion")]
    public GameObject[] prefabsDeChunk;
    public int chunksActivos = 4;

    [Header("Inicio seguro")]
    [Tooltip("Arrastra aqui el Chunk_A (el plano).")]
    public GameObject chunkInicial;
    [Tooltip("Cuantos chunks planos seguidos salen al empezar.")]
    public int chunksPlanosAlInicio = 2;

    [Header("Depuracion")]
    public bool mostrarAnchos = true;   // imprime el ancho medido en la consola

    private List<GameObject> chunks = new List<GameObject>();

    // Guarda el ancho de cada chunk creado, para no volver a medirlo
    private Dictionary<GameObject, float> anchos = new Dictionary<GameObject, float>();

    private Camera camara;
    private float siguienteX;
    private int ultimoIndice = -1;   // evita repetir el mismo chunk dos veces seguidas
    private int chunksCreados = 0;   // cuenta cuantos chunks se han generado en total

    void Start()
    {
        camara = Camera.main;

        if (prefabsDeChunk == null || prefabsDeChunk.Length == 0)
        {
            Debug.LogError("GeneradorDeChunks: la lista 'Prefabs De Chunk' esta vacia.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < prefabsDeChunk.Length; i++)
        {
            if (prefabsDeChunk[i] == null)
            {
                Debug.LogError($"GeneradorDeChunks: el Element {i} esta vacio (None).", this);
                enabled = false;
                return;
            }
        }

        siguienteX = camara.transform.position.x - MitadAnchoCamara();

        for (int i = 0; i < chunksActivos; i++)
            CrearChunk();
    }

    void Update()
    {
        if (chunks.Count == 0) return;

        GameObject masViejo = chunks[0];
        float anchoDeEste = anchos[masViejo];

        float bordeDerechoDelChunk = masViejo.transform.position.x + anchoDeEste;
        float bordeIzquierdoCamara = camara.transform.position.x - MitadAnchoCamara();

        if (bordeDerechoDelChunk < bordeIzquierdoCamara)
        {
            // Se elimina el chunk viejo y se crea uno NUEVO elegido al azar
            chunks.RemoveAt(0);
            anchos.Remove(masViejo);
            Destroy(masViejo);

            CrearChunk();
        }
    }

    void CrearChunk()
    {
        GameObject prefab = ElegirPrefab();

        GameObject nuevo = Instantiate(
            prefab,
            new Vector3(siguienteX, 0f, 0f),
            Quaternion.identity,
            transform
        );

        float ancho = MedirAncho(nuevo);
        anchos[nuevo] = ancho;

        if (mostrarAnchos)
            Debug.Log($"Chunk '{prefab.name}' mide {ancho} unidades de ancho.");

        siguienteX += ancho;
        chunks.Add(nuevo);
        chunksCreados++;
    }

    // Los primeros chunks son planos; despues, al azar sin repetir el anterior
    GameObject ElegirPrefab()
    {
        if (chunkInicial != null && chunksCreados < chunksPlanosAlInicio)
            return chunkInicial;

        if (prefabsDeChunk.Length == 1)
            return prefabsDeChunk[0];

        int indice;
        do { indice = Random.Range(0, prefabsDeChunk.Length); }
        while (indice == ultimoIndice);

        ultimoIndice = indice;
        return prefabsDeChunk[indice];
    }

    float MedirAncho(GameObject chunk)
    {
        Renderer[] partes = chunk.GetComponentsInChildren<Renderer>();

        if (partes.Length == 0)
        {
            Debug.LogError("GeneradorDeChunks: el prefab no tiene ningun sprite adentro.", this);
            return 1f;
        }

        Bounds caja = partes[0].bounds;
        foreach (Renderer parte in partes)
            caja.Encapsulate(parte.bounds);

        return caja.size.x;
    }

    float MitadAnchoCamara()
    {
        return camara.orthographicSize * camara.aspect;
    }
}