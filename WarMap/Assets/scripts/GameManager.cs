using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public TMP_Text textoInformativo;

    [Header("Menú Fin de Partida")]
    public GameObject menuFinPartida;

    [Header("Dados")]
    public Dado dadoAzul; 
    public Dado dadoRojo; 

    public enum EstadoJuego { Preparacion, Jugando, GameOver }
    private EstadoJuego estadoActual = EstadoJuego.Preparacion;

    public enum FaseTurno { Refuerzo, Ataque, Defensa }
    private FaseTurno faseTurnoActual;

    private Provincia.Dueño turnoDe = Provincia.Dueño.Jugador; 
    
    // Configuración
    private int turnoPrepActual = 1;
    private int limiteTurnosPrep = 10;

    // Juego
    private int tropasDisponibles = 0;
    private Provincia provinciaSeleccionada;
    
    // --- BLOQUEOS Y MEMORIA DE BATALLA ---
    private bool esperandoDado = false;
    private bool viendoBatalla = false; 
    
    private Provincia provinciaAtacanteCache;
    private Provincia provinciaDefensoraCache;
    private bool ultimoAtaqueGano;
    
    private int tropasFinalesAtacante; 
    private int tropasFinalesDefensor; 

    void Awake() { instance = this; }

    void Start() 
    { 
        if (menuFinPartida != null) menuFinPartida.SetActive(false);

        ActualizarUI(); 
        if(dadoAzul != null) dadoAzul.gameObject.SetActive(false);
        if(dadoRojo != null) dadoRojo.gameObject.SetActive(false);
    }

    void Update()
    {
        if (estadoActual == EstadoJuego.GameOver) return;

        if (estadoActual == EstadoJuego.Jugando && !esperandoDado && !viendoBatalla)
        {
            if (Input.GetKeyDown(KeyCode.Space)) SiguienteFase();
        }
    }

    // Click IZQUIERDO selección de provincia
    public void GestionarClicIzquierdo(Provincia provincia)
    {
        if (estadoActual == EstadoJuego.GameOver) return;
        if (esperandoDado || viendoBatalla) return; 

        if (estadoActual == EstadoJuego.Preparacion)
        {
            prepararProvincia(provincia);
            return;
        }

        if (faseTurnoActual == FaseTurno.Refuerzo)
        {
            if (provincia.quienManda == turnoDe)
            {
                if (tropasDisponibles > 0)
                {
                    provincia.unidades += tropasDisponibles;
                    provincia.ActualizarNumeroVisual();
                    ActualizarUI($"{tropasDisponibles} tropas más a {LimpiarNombre(provincia.name)}");
                    tropasDisponibles = 0;
                    
                    if(dadoAzul != null) dadoAzul.gameObject.SetActive(false);
                    if(dadoRojo != null) dadoRojo.gameObject.SetActive(false);

                    faseTurnoActual = FaseTurno.Ataque;
                    ActualizarUI();
                }
            }
            else
            {
                ActualizarUI("Solo puedes reforzar tus propias provincias!");
            }
        }
        else 
        {
            if (provincia.quienManda == turnoDe) SeleccionarProvincia(provincia);
        }
    }

   // Click DERECHO ataque o movimiento
    public void GestionarClicDerecho(Provincia objetivo)
    {
        if (estadoActual == EstadoJuego.GameOver) return;
        if (esperandoDado || viendoBatalla) return;
        if (estadoActual == EstadoJuego.Preparacion) return;
        if (provinciaSeleccionada == null) return;
        if (objetivo == provinciaSeleccionada) return;

        if (!provinciaSeleccionada.vecinos.Contains(objetivo))
        {
            ActualizarUI($"Error! No son vecinos.");
            return; 
        }

        if (faseTurnoActual == FaseTurno.Ataque)
        {
            if (objetivo.quienManda != turnoDe) 
            {
                ResolverAtaque(provinciaSeleccionada, objetivo);
            }
        }
        else if (faseTurnoActual == FaseTurno.Defensa)
        {
            if (objetivo.quienManda == turnoDe) MoverTropas(objetivo);
        }
    }

    // Fase inicial de preparación +2 por turno
    void prepararProvincia(Provincia provincia)
    {
        if (provincia.quienManda != Provincia.Dueño.Nadie && provincia.quienManda != turnoDe) return;
        provincia.quienManda = turnoDe;
        provincia.unidades += 2;
        provincia.ActualizarColor();
        provincia.ActualizarNumeroVisual(); 
        AvanzarTurnoPreparacion();
    }

    void AvanzarTurnoPreparacion()
    {
        turnoDe = (turnoDe == Provincia.Dueño.Jugador) ? Provincia.Dueño.Enemigo : Provincia.Dueño.Jugador;
        turnoPrepActual++;

        if (turnoPrepActual > limiteTurnosPrep)
        {
            estadoActual = EstadoJuego.Jugando;
            IniciarTurno();
        }
        else ActualizarUI();
    }

    void IniciarTurno()
    {
        faseTurnoActual = FaseTurno.Refuerzo;
        if(provinciaSeleccionada != null) { provinciaSeleccionada.Deseleccionar(); provinciaSeleccionada = null; }

        esperandoDado = true; 

        if (turnoDe == Provincia.Dueño.Jugador)
        {
            ActualizarUI("Haz CLIC al dado AZUL.");
            if(dadoAzul != null) { dadoAzul.gameObject.SetActive(true); dadoAzul.PrepararDado(false); }
        }
        else
        {
            ActualizarUI("Haz CLIC al dado ROJO.");
            if(dadoRojo != null) { dadoRojo.gameObject.SetActive(true); dadoRojo.PrepararDado(false); }
        }
    }

    // Recibe el resultado del dado lanzado
    public void RecibirResultadoDado(int resultado)
    {
        // 1. Calculamos solo el número del bonus
        int bonus = CalcularBonusRegional(turnoDe);

        // 2. Sumamos Dado + Bonus
        tropasDisponibles = resultado + bonus;
        
        esperandoDado = false; 
        
        // 3. Informamos al jugador 
        if (bonus > 0)
        {
            ActualizarUI($"¡{resultado} del Dado!\nBonus: {bonus}\nTropas totales: {tropasDisponibles}\nHaz clic en una provincia para recibirlos.");
        }
        else
        {
            ActualizarUI($"¡{tropasDisponibles} tropas!\nHaz clic en una provincia para recibirlos.");
        }
    }
    
    void SiguienteFase()
    {
        if (faseTurnoActual == FaseTurno.Ataque)
        {
            faseTurnoActual = FaseTurno.Defensa;
            if(provinciaSeleccionada != null) provinciaSeleccionada.Deseleccionar();
        }
        else if (faseTurnoActual == FaseTurno.Defensa)
        {
            turnoDe = (turnoDe == Provincia.Dueño.Jugador) ? Provincia.Dueño.Enemigo : Provincia.Dueño.Jugador;
            IniciarTurno();
            return;
        }
        ActualizarUI();
    }

    // Ataque entre provincias
    public void ResolverAtaque(Provincia atacante, Provincia defensor)
    {
        if (atacante.unidades < 2) return; // Necesita al menos 2 para atacar

        int fuerzaAtacante = atacante.unidades;
        int fuerzaDefensor = defensor.unidades;
        bool ganaAtacante = fuerzaAtacante > fuerzaDefensor;

        if (ganaAtacante)
        {
            int supervivientes = fuerzaAtacante - fuerzaDefensor;
            int tropasParaMover = supervivientes - 1;
            if (tropasParaMover < 1) tropasParaMover = 1; 

            tropasFinalesDefensor = tropasParaMover; 
            tropasFinalesAtacante = 1; 
        }
        else
        {
            int supervivientes = fuerzaDefensor - fuerzaAtacante;
            if (supervivientes < 1) supervivientes = 1; 

            tropasFinalesDefensor = supervivientes;
            tropasFinalesAtacante = 1; 
        }

        if (defensor.quienManda == Provincia.Dueño.Nadie)
        {
            AplicarResultadoCalculado(atacante, defensor, ganaAtacante);
            ActualizarUI($"{LimpiarNombre(defensor.name)} ocupado.");
            if (provinciaSeleccionada != null) provinciaSeleccionada.Deseleccionar();
            provinciaSeleccionada = null;
        }
        else
        {
            provinciaAtacanteCache = atacante;
            provinciaDefensoraCache = defensor;
            ultimoAtaqueGano = ganaAtacante;

            viendoBatalla = true;
            ActualizarUI(); 

            if (BattleManager.instance != null)
            {
                BattleManager.instance.EmpezarBatalla(atacante, defensor, ganaAtacante);
            }
            else
            {
                FinalizarCombateVisual();
            }
        }
    }

    void AplicarResultadoCalculado(Provincia atacante, Provincia defensor, bool ganaAtacante)
    {
        if (ganaAtacante)
        {
            defensor.quienManda = atacante.quienManda;
            defensor.unidades = tropasFinalesDefensor; 
            atacante.unidades = tropasFinalesAtacante; 
        }
        else
        {
            defensor.unidades = tropasFinalesDefensor;
            atacante.unidades = tropasFinalesAtacante;
        }

        atacante.ActualizarColor();
        atacante.ActualizarNumeroVisual();
        defensor.ActualizarColor();
        defensor.ActualizarNumeroVisual();

        ComprobarVictoriaDerrota();
    }

    void ComprobarVictoriaDerrota()
    {
        Provincia[] todasLasProvincias = FindObjectsByType<Provincia>(FindObjectsSortMode.None);

        int contadorJugador = 0;
        int contadorEnemigo = 0;

        foreach (Provincia p in todasLasProvincias)
        {
            if (p.quienManda == Provincia.Dueño.Jugador) contadorJugador++;
            if (p.quienManda == Provincia.Dueño.Enemigo) contadorEnemigo++;
        }

        if (contadorJugador == 0 || contadorEnemigo == 0)
        {
            estadoActual = EstadoJuego.GameOver;
            ActualizarUI(); 
        }
    }

    public void FinalizarCombateVisual()
    {
        if (estadoActual == EstadoJuego.GameOver) return;

        AplicarResultadoCalculado(provinciaAtacanteCache, provinciaDefensoraCache, ultimoAtaqueGano);

        if (estadoActual == EstadoJuego.GameOver) return;

        if (ultimoAtaqueGano) 
            ActualizarUI($"¡VICTORIA {LimpiarNombre(provinciaDefensoraCache.name)} conquistada.");
        else 
            ActualizarUI($"¡DEFENSA! El enemigo resistió.");

        viendoBatalla = false;
        
        if (provinciaSeleccionada != null) provinciaSeleccionada.Deseleccionar();
        provinciaSeleccionada = null;
    }

    void MoverTropas(Provincia destino)
    {
        if (provinciaSeleccionada.unidades < 2) return;
        provinciaSeleccionada.unidades--;
        destino.unidades++;
        provinciaSeleccionada.ActualizarNumeroVisual();
        destino.ActualizarNumeroVisual();
    }

    void SeleccionarProvincia(Provincia p)
    {
        if (provinciaSeleccionada != null) provinciaSeleccionada.Deseleccionar();
        provinciaSeleccionada = p;
        provinciaSeleccionada.Seleccionar();
        
        ActualizarUI($"Seleccionada: {LimpiarNombre(p.name)}");
    }

    // Interfaz de usuario y mensajes
    void ActualizarUI(string mensajeExtra = "")
    {
        if (textoInformativo == null) return;

        // -- GAME OVER --
        if (estadoActual == EstadoJuego.GameOver)
        {
            if (menuFinPartida != null) menuFinPartida.SetActive(true);

            Provincia[] todas = FindObjectsByType<Provincia>(FindObjectsSortMode.None);
            bool quedaAlguienAzul = false;
            foreach(var p in todas) if(p.quienManda == Provincia.Dueño.Jugador) { quedaAlguienAzul = true; break; }

            if (quedaAlguienAzul)
                textoInformativo.text = "<size=150%><color=blue>¡VICTORIA DEL JUGADOR AZUL!</color></size>\nEl ejército rojo ha sido eliminado.";
            else
                textoInformativo.text = "<size=150%><color=red>¡VICTORIA DEL JUGADOR ROJO!</color></size>\nEl ejército azul ha sido eliminado.";
            return;
        }
        
        string nombreJugador = (turnoDe == Provincia.Dueño.Jugador) ? "<color=blue>AZUL</color>" : "<color=red>ROJO</color>";

        // -- PREPARACIÓN --
        if (estadoActual == EstadoJuego.Preparacion)
        {
            textoInformativo.text = $"TURNO: {nombreJugador} \nFASE: INICIO ({turnoPrepActual}/{limiteTurnosPrep})\nEscoge tus provincias iniciales.";
            return;
        }

        // -- JUEGO NORMAL --
        string nombreFase = "";
        switch (faseTurnoActual)
        {
            case FaseTurno.Refuerzo: nombreFase = "REFUERZO"; break;
            case FaseTurno.Ataque:    nombreFase = "ATAQUE"; break;
            case FaseTurno.Defensa:   nombreFase = "DEFENSA"; break;
        }

        string cabecera = $"TURNO: {nombreJugador}\nFASE: {nombreFase}";
        
        string instrucciones = "";

        if (esperandoDado)
        {
            instrucciones = "Lanza el dado para obtener tropas.";
        }
        else if (viendoBatalla)
        {
            instrucciones = $"Combate:\n{LimpiarNombre(provinciaAtacanteCache.name)} vs {LimpiarNombre(provinciaDefensoraCache.name)}";
        }
        else
        {
            if (string.IsNullOrEmpty(mensajeExtra))
            {
                if(faseTurnoActual == FaseTurno.Refuerzo) instrucciones = "Selecciona provincia para reforzar.";
                else if(faseTurnoActual == FaseTurno.Ataque) instrucciones = "Clic Derecho en enemigos para atacar.";
                else instrucciones = "Mueve tropas entre tus territorios.";
            }
            else
            {
                instrucciones = mensajeExtra;
            }
        }

        textoInformativo.text = $"{cabecera}\n{instrucciones}";
    }

    string LimpiarNombre(string nombreOriginal)
    {
        if (string.IsNullOrEmpty(nombreOriginal)) return "";
        string[] partes = nombreOriginal.Split('-');
        if (partes.Length > 1) return partes[1].Trim();
        return nombreOriginal;
    }

    public void BotonReiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void BotonBack()
    {
        SceneManager.LoadScene("Menu");
    }

    // Cálculo de bonus regional
    int CalcularBonusRegional(Provincia.Dueño jugador)
    {
        int bonusTotal = 0;
        
        Provincia[] todas = FindObjectsByType<Provincia>(FindObjectsSortMode.None);
        var regiones = todas.GroupBy(p => p.transform.parent.name);

        foreach (var region in regiones)
        {
            if (region.Count() < 2) continue;

            bool tieneTodaLaRegion = region.All(p => p.quienManda == jugador);

            if (tieneTodaLaRegion)
            {
                int puntos = 0;
                string nombreRegion = region.Key; 

                switch (nombreRegion)
                {
                    case "Extremadura":
                        puntos = 1; break;
                    case "Comunidad-Valenciana":
                    case "Aragon":
                    case "Pais-Vasco":
                        puntos = 2; break;
                    case "Galicia":
                    case "Cataluña": 
                    case "Cataluna":
                        puntos = 3; break;
                    case "Castilla-LaMancha":
                        puntos = 4; break;
                    case "Andalucia":
                        puntos = 5; break;
                    case "Castilla-Leon":
                        puntos = 6; break;
                }

                if (puntos > 0)
                {
                    bonusTotal += puntos;
                }
            }
        }
        
        return bonusTotal;
    }
}