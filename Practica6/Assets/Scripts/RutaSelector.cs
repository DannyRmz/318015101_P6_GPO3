using UnityEngine;

public class RutaSelector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public WayCreator[] rutas;

    public WayCreator ObtenerRutaAleatoria()
    {
        int index = Random.Range(0, rutas.Length);
        return rutas[index];
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
