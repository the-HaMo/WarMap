using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuSeleccionManager : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Color colorSeleccionado = Color.green;

    [Header("Referencias a Botones")]
    public Button botonEspana;
    public Button boton2Jug;
    
    private bool mapaListo = false;
    private bool jugadoresListos = false;

    public void SeleccionarEspana()
    {
        mapaListo = true;        
        // Pintamos el botón de verde para que se vea marcado
        var colors = botonEspana.colors;
        colors.normalColor = colorSeleccionado;
        colors.selectedColor = colorSeleccionado;
        botonEspana.colors = colors;
    }

    public void Seleccionar2Jugadores()
    {
        jugadoresListos = true;

        // Pintamos el botón de verde
        var colors = boton2Jug.colors;
        colors.normalColor = colorSeleccionado;
        colors.selectedColor = colorSeleccionado;
        boton2Jug.colors = colors;
    }

    public void BotonStart()
    {
        if (mapaListo == false || jugadoresListos == false)
        {
            Debug.Log("¡Debes seleccionar España y 2 Jugadores primero!");
            return; 
        }
        SceneManager.LoadScene("spain");
    }

    public void BotonBack()
    {
        SceneManager.LoadScene("Inicio");
    }
}