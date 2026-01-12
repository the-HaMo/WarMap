using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    [Header("Cámaras")]
    public Camera mainCamera;   
    public Camera battleCamera; 

    [Header("Soldado y Materiales")]
    public GameObject soldadoUnicoPrefab; 
    public Material materialAzul; 
    public Material materialRojo;

    [Header("Ajustes")]
    public Vector3 offsetCamara = new Vector3(0, 0.394f, 0.792f); 
    public float duracionViajeCamara = 1.5f; 

    void Awake() { instance = this; }

    public void EmpezarBatalla(Provincia atacante, Provincia defensor, bool ganaAtacante)
    {
        StartCoroutine(SecuenciaDeBatalla(atacante, defensor, ganaAtacante));
    }

    IEnumerator SecuenciaDeBatalla(Provincia p1, Provincia p2, bool ganaAtacante)
    {
        Vector3 sitioAtacante = p1.GetComponent<Renderer>().bounds.center; 
        Vector3 sitioDefensor = p2.GetComponent<Renderer>().bounds.center; 
        Vector3 centroCombate = (sitioAtacante + sitioDefensor) / 2;   

        Vector3 posicionFinalCamara = centroCombate + offsetCamara;
        Quaternion rotacionFinalCamara = Quaternion.LookRotation(centroCombate - posicionFinalCamara);

        battleCamera.transform.position = mainCamera.transform.position;
        battleCamera.transform.rotation = mainCamera.transform.rotation;
        
        mainCamera.gameObject.SetActive(false); 
        battleCamera.gameObject.SetActive(true); 
        
        // Creamos soldados
        GameObject soldadoA = Instantiate(soldadoUnicoPrefab, sitioAtacante, Quaternion.identity);
        AplicarColor(soldadoA, p1.quienManda);

        GameObject soldadoB = Instantiate(soldadoUnicoPrefab, sitioDefensor, Quaternion.identity);
        AplicarColor(soldadoB, p2.quienManda);

        // Orientamos
        Vector3 dirHaciaDefensor = sitioDefensor - sitioAtacante;
        Vector3 dirHaciaAtacante = sitioAtacante - sitioDefensor;
        dirHaciaDefensor.y = 0; dirHaciaAtacante.y = 0;

        if (dirHaciaDefensor != Vector3.zero) 
            soldadoA.transform.rotation = Quaternion.LookRotation(-dirHaciaDefensor);

        if (dirHaciaAtacante != Vector3.zero) 
            soldadoB.transform.rotation = Quaternion.LookRotation(-dirHaciaAtacante);

        // Transición suave de cámara
        float tiempo = 0;
        Vector3 posOrigen = battleCamera.transform.position;
        Quaternion rotOrigen = battleCamera.transform.rotation;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / duracionViajeCamara;
            float tSuave = Mathf.SmoothStep(0, 1, tiempo); 
            
            battleCamera.transform.position = Vector3.Lerp(posOrigen, posicionFinalCamara, tSuave);
            battleCamera.transform.rotation = Quaternion.Slerp(rotOrigen, rotacionFinalCamara, tSuave);
            yield return null;
        }

        // Esperamos un poquito ya con la cámara cerca de los soldados
        yield return new WaitForSeconds(0.2f);

        GameObject perdedor = ganaAtacante ? soldadoB : soldadoA;
        GameObject ganador = ganaAtacante ? soldadoA : soldadoB;
        explotar scriptExplosion = perdedor.GetComponentInChildren<explotar>();
        
        if (scriptExplosion != null)
        {
            scriptExplosion.Bum(); 
        }
        else
        {
            Destroy(perdedor); // Si se destruye no funciona la explosión
        }

        yield return new WaitForSeconds(1.5f);
        
        Destroy(ganador);
        if (perdedor != null) Destroy(perdedor);

        // transición de vuelta de cámara
        tiempo = 0;
        Vector3 posAbajo = battleCamera.transform.position;
        Quaternion rotAbajo = battleCamera.transform.rotation;
        
        Vector3 posArriba = mainCamera.transform.position;
        Quaternion rotArriba = mainCamera.transform.rotation;

        while (tiempo < 1f)
        {
            tiempo += Time.deltaTime / (duracionViajeCamara * 0.5f); // Subida rápida
            float tSuave = Mathf.SmoothStep(0, 1, tiempo); 

            battleCamera.transform.position = Vector3.Lerp(posAbajo, posArriba, tSuave);
            battleCamera.transform.rotation = Quaternion.Slerp(rotAbajo, rotArriba, tSuave);
            yield return null;
        }

        // 7. FINALIZAR
        battleCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
        GameManager.instance.FinalizarCombateVisual(); 
    }

    void AplicarColor(GameObject soldado, Provincia.Dueño dueño)
    {
        Material materialAUsar = (dueño == Provincia.Dueño.Jugador) ? materialAzul : materialRojo;
        Renderer[] todosLosRenderers = soldado.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in todosLosRenderers)
        {
            r.material = materialAUsar;
        }
    }
}