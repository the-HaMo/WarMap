using UnityEngine;
using TMPro; 
using System.Collections.Generic; // <--- 1. NUEVO: Necesario para usar Listas

public class Provincia : MonoBehaviour
{
    public enum Dueño { Nadie, Jugador, Enemigo }

    [Header("Configuración")]
    public Dueño quienManda;
    public int unidades = 1;

    // --- 2. NUEVO: AQUÍ ESTÁ LA MODIFICACIÓN DE VECINOS ---
    [Header("Fronteras")]
    public List<Provincia> vecinos; // Arrastra aquí las provincias vecinas en el Inspector
    // ------------------------------------------------------
    
    [Header("Visuales")]
    public GameObject prefabEtiqueta; 
    private TMP_Text textoCantidad;   

    private Renderer miRenderer;
    private Color colorOriginal;

    void Start()
    {
        miRenderer = GetComponent<Renderer>();
        
        // Regla de inicio
        if (quienManda == Dueño.Nadie) unidades = 0;
        
        ActualizarColor();
        CrearEtiqueta(); 
    }

    void CrearEtiqueta()
    {
        if (prefabEtiqueta != null)
        {
            Vector3 posicionBase;

            // 1. BUSCAMOS LA "CHINCHETA" MANUAL
            Transform puntoAnclaje = transform.Find("PuntoEtiqueta");

            if (puntoAnclaje != null)
            {
                // ¡Encontrado! Usamos su posición
                posicionBase = puntoAnclaje.position;
            }
            else
            {
                // No existe, usamos el centro matemático automático (Plan B)
                posicionBase = miRenderer.bounds.center;
            }
            
            // 2. APLICAMOS LA ALTURA
            // (Mantengo tu valor original, si no se ve, recuerda subirlo a 0.2f)
            Vector3 posicionFinal = new Vector3(posicionBase.x, posicionBase.y + 0.00024f, posicionBase.z);

            // 3. Creamos el objeto (Instantiate)
            GameObject etiqueta = Instantiate(prefabEtiqueta, posicionFinal, Quaternion.Euler(90, 0, 0));
            
            // 4. Lo hacemos hijo de la provincia
            etiqueta.transform.SetParent(this.transform);

            // 5. Buscamos el texto y actualizamos
            textoCantidad = etiqueta.GetComponentInChildren<TMP_Text>();
            ActualizarNumeroVisual();
        }
    }

    // Llamamos a esto cada vez que cambie el ejército
    public void ActualizarNumeroVisual()
    {
        if (textoCantidad != null)
        {
            textoCantidad.text = unidades.ToString();
        }
    }

    // --- CLICS ---
    private void OnMouseDown() => GameManager.instance.GestionarClicIzquierdo(this);

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) GameManager.instance.GestionarClicDerecho(this);
    }

    // --- MODIFICADORES ---
    public void ActualizarColor()
    {
        if (quienManda == Dueño.Jugador) miRenderer.material.color = Color.blue;
        else if (quienManda == Dueño.Enemigo) miRenderer.material.color = Color.red;
        else miRenderer.material.color = Color.white;
        colorOriginal = miRenderer.material.color;
    }

    public void Seleccionar() => miRenderer.material.color = Color.green;
    public void Deseleccionar() => miRenderer.material.color = colorOriginal;
}