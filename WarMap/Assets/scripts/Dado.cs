using UnityEngine;
using System.Collections;

public class Dado : MonoBehaviour
{
    [Header("Calibración de Caras")]
    public Vector3[] rotacionesCaras;

    private bool puedeSerLanzado = false;
    private bool estaRodando = false;
    
    private GameManager gameManager;

    void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        rotacionesCaras = new Vector3[7]; 
        rotacionesCaras[3] = new Vector3(0, 0, 0);     // 3
        rotacionesCaras[1] = new Vector3(90, 0, 0);    // 1
        rotacionesCaras[4] = new Vector3(180, 0, 0);   // 4
        rotacionesCaras[6] = new Vector3(270, 0, 0);   // 6
        rotacionesCaras[5] = new Vector3(0, 0, 90);    // 5
        rotacionesCaras[2] = new Vector3(0, 0, 270);   // 2
    }

    public void PrepararDado(bool lanzamientoAutomatico)
    {
        gameObject.SetActive(true); 
        estaRodando = false;        
        puedeSerLanzado = true;     

        if (lanzamientoAutomatico)
        {
            StartCoroutine(Lanzar());
        }
    }

    private void OnMouseDown()
    {
        if (puedeSerLanzado && !estaRodando)
        {
            StartCoroutine(Lanzar());
        }
    }

    IEnumerator EsperarParaActivar()
    {
        yield return new WaitForSeconds(0.5f); 
    }

    IEnumerator Lanzar()
    {
        puedeSerLanzado = false; 
        estaRodando = true;

        int resultado = Random.Range(1, 7);

        // animacion del dado 
        float tiempoTotal = 1.0f; // Tiempo que pasa rodando
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoTotal)
        {
            // Elegimos una cara cualquiera 
            int caraRandom = Random.Range(1, 7);
            Quaternion rotacionTumbo = Quaternion.Euler(rotacionesCaras[caraRandom]);
            float paso = 0f;
            Quaternion rotInicial = transform.rotation;
            // transiciones entre caras
            while (paso < 0.15f && tiempoTranscurrido < tiempoTotal)
            {
                transform.rotation = Quaternion.Lerp(rotInicial, rotacionTumbo, paso * 15f); 
                paso += Time.deltaTime;
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
        }

        // Resultado final
        transform.rotation = Quaternion.Euler(rotacionesCaras[resultado]);
        yield return new WaitForSeconds(0.3f);
        // Avisamos al juego
        gameManager.RecibirResultadoDado(resultado);
        
        estaRodando = false;
    }
}