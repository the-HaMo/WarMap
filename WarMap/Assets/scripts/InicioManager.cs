using UnityEngine;
using UnityEngine.SceneManagement;

public class InicioManager : MonoBehaviour
{
    public void IrAlMenu()
    {
        SceneManager.LoadScene("Menu"); 
    }

    public void IrAAyuda()
    {
        SceneManager.LoadScene("Help");
    }

     public void IrAInicio()
    {
        SceneManager.LoadScene("Inicio");
    }

    public void Exit()
    {
        Application.Quit();
    }
}