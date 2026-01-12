using UnityEngine;

public class explotar : MonoBehaviour
{
    [Header("piezas")]
    public GameObject[] piezas; 

    [Header("Configuración")]
    public float fuerza = 1000f; 
    public float vertical = 2f;
    public float tiempoParaBorrar = 3f; 

    void Start()
    {
        foreach (GameObject pieza in piezas)
        {
            if (pieza != null)
            {
                if(pieza.GetComponent<MeshRenderer>()) pieza.GetComponent<MeshRenderer>().enabled = false;
                if(pieza.GetComponent<Collider>()) pieza.GetComponent<Collider>().enabled = false;
                if(pieza.GetComponent<Rigidbody>()) pieza.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }

    public void Bum()
    {
        // 1. Ocultamos el soldado completo
        if(GetComponent<MeshRenderer>()) GetComponent<MeshRenderer>().enabled = false;
        
        // 2. Activamos la explosión
        foreach (GameObject pieza in piezas)
        {
            if (pieza != null)
            {
                if(pieza.GetComponent<MeshRenderer>()) pieza.GetComponent<MeshRenderer>().enabled = true;
                if(pieza.GetComponent<Collider>()) pieza.GetComponent<Collider>().enabled = true;

                if(pieza.GetComponent<Rigidbody>())
                {
                    pieza.GetComponent<Rigidbody>().isKinematic = false;
                    pieza.GetComponent<Rigidbody>().AddExplosionForce(fuerza, transform.position, 5f, vertical, ForceMode.Impulse);
                }
            }
        }

        // 3. Borramos el soldado pasado un tiempo
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject, tiempoParaBorrar);
        }    
    }
}