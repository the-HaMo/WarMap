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
        gameManager = FindObjectOfType<GameManager>();

        // TUS COORDENADAS EXACTAS
        rotacionesCaras = new Vector3[7]; 
        rotacionesCaras[3] = new Vector3(0, 0, 0);     // 3
        rotacionesCaras[1] = new Vector3(90, 0, 0);    // 1
        rotacionesCaras[4] = new Vector3(180, 0, 0);   // 4
        rotacionesCaras[6] = new Vector3(270, 0, 0);   // 6
        rotacionesCaras[5] = new Vector3(0, 0, 90);    // 5
        rotacionesCaras[2] = new Vector3(0, 0, 270);   // 2
    }

    public void PrepararDado(bool esParaIA)
    {
        puedeSerLanzado = true;
        estaRodando = false;
        // Si es la IA (o el jugador 2 en manual), espera un segundo antes de dejar clicar
        if (esParaIA) StartCoroutine(EsperarParaActivar());
    }

    private void OnMouseDown()
    {
        if (puedeSerLanzado && !estaRodando)
        {
            StartCoroutine(RutinaLanzar());
        }
    }

    IEnumerator EsperarParaActivar()
    {
        // Pequeña pausa al aparecer antes de que se pueda lanzar
        yield return new WaitForSeconds(0.5f); 
    }

    IEnumerator RutinaLanzar()
    {
        puedeSerLanzado = false; 
        estaRodando = true;

        // 1. Calculamos el resultado final
        int resultado = Random.Range(1, 7);

        // 2. ANIMACIÓN DE RODAR (Dando tumbos entre caras)
        float tiempoTotal = 1.0f; // Tiempo que pasa rodando
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoTotal)
        {
            // Elegimos una cara cualquiera para hacer el efecto de "tumbo"
            int caraRandom = Random.Range(1, 7);
            Quaternion rotacionTumbo = Quaternion.Euler(rotacionesCaras[caraRandom]);
            
            // Hacemos un movimiento rápido hacia esa cara
            float paso = 0f;
            Quaternion rotInicial = transform.rotation;
            
            // Este bucle interno hace la transición rápida entre un tumbo y otro
            while (paso < 0.15f && tiempoTranscurrido < tiempoTotal)
            {
                transform.rotation = Quaternion.Lerp(rotInicial, rotacionTumbo, paso * 15f); 
                paso += Time.deltaTime;
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
        }

        // ---------------------------------------------------------
        // 3. ATERRIZAJE FINAL (SECO) - ¡Aquí está el cambio!
        // ---------------------------------------------------------
        // Al terminar el tiempo, forzamos la rotación final inmediatamente.
        // Sin transiciones raras, se queda clavado.
        transform.rotation = Quaternion.Euler(rotacionesCaras[resultado]);

        // Añadimos una pequeñísima pausa (0.3s) para que el jugador vea el número
        // antes de que el dado desaparezca.
        yield return new WaitForSeconds(0.3f);

        // 4. Avisamos al juego
        Debug.Log("Ha salido un: " + resultado);
        gameManager.RecibirResultadoDado(resultado);
        
        estaRodando = false;
    }
}