using UnityEngine;
using TMPro; 
using System.Collections.Generic; 

public class Provincia : MonoBehaviour
{
    public enum Dueño { Nadie, Jugador, Enemigo }

    [Header("Configuración")]
    public Dueño quienManda;
    public int unidades = 1;

    [Header("Provincias Vecinas")]
    public List<Provincia> vecinos; 

    [Header("Visuales")]
    public GameObject prefabEtiqueta; 
    private TMP_Text textoCantidad;   

    private Renderer miRenderer;
    private Color colorOriginal;

    void Start()
    {
        miRenderer = GetComponent<Renderer>();
        if (quienManda == Dueño.Nadie) unidades = 0;
        
        ActualizarColor();
        CrearEtiqueta(); 
    }

    void CrearEtiqueta()
    {
        if (prefabEtiqueta != null)
        {
            Vector3 posicionBase;

            // empty para la posición del canvas
            Transform puntoAnclaje = transform.Find("PuntoEtiqueta");

            if (puntoAnclaje != null)
            {
                posicionBase = puntoAnclaje.position;
            }
            else
            {
                // Centro de la provincia
                posicionBase = miRenderer.bounds.center;
            }
            
            Vector3 posicionFinal = new Vector3(posicionBase.x, posicionBase.y + 0.00024f, posicionBase.z);
            // creacion del canvas
            GameObject etiqueta = Instantiate(prefabEtiqueta, posicionFinal, Quaternion.Euler(90, 0, 0));
            etiqueta.transform.SetParent(this.transform);
            textoCantidad = etiqueta.GetComponentInChildren<TMP_Text>();
            ActualizarNumeroVisual();
        }
    }

    public void ActualizarNumeroVisual()
    {
        if (textoCantidad != null)
        {
            textoCantidad.text = unidades.ToString();
        }
    }

    private void OnMouseDown() => GameManager.instance.GestionarClicIzquierdo(this);

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) GameManager.instance.GestionarClicDerecho(this);
    }

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