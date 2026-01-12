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
    
    // Variables para saber si el jugador ha hecho clic
    private bool mapaListo = false;
    private bool jugadoresListos = false;

    // --- 1. FUNCIÓN PARA EL BOTÓN ESPAÑA ---
    public void SeleccionarEspana()
    {
        mapaListo = true;
        
        // Pintamos el botón de verde para que se vea marcado
        var colors = botonEspana.colors;
        colors.normalColor = colorSeleccionado;
        colors.selectedColor = colorSeleccionado;
        botonEspana.colors = colors;
    }

    // --- 2. FUNCIÓN PARA EL BOTÓN 2 JUGADORES ---
    public void Seleccionar2Jugadores()
    {
        jugadoresListos = true;

        // Pintamos el botón de verde
        var colors = boton2Jug.colors;
        colors.normalColor = colorSeleccionado;
        colors.selectedColor = colorSeleccionado;
        boton2Jug.colors = colors;
    }

    // --- 3. FUNCIÓN PARA EL BOTÓN START ---
    public void BotonStart()
    {
        // Si no ha marcado los dos, no le dejamos pasar
        if (mapaListo == false || jugadoresListos == false)
        {
            Debug.Log("¡Debes seleccionar España y 2 Jugadores primero!");
            return; 
        }

        // Si todo está marcado, guardamos y arrancamos
        PlayerPrefs.SetString("MapaSeleccionado", "Espana");
        PlayerPrefs.SetInt("NumeroJugadores", 2);
        PlayerPrefs.Save();

        SceneManager.LoadScene("spain");
    }

    public void BotonBack()
    {
        SceneManager.LoadScene("Inicio");
    }
}