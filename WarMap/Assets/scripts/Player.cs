using UnityEngine;
using System.Collections.Generic;

public class Player
{
    public enum Tipo { Humano, IA }
    public enum Dificultad { Facil, Normal, Dificil }

    public int id; // 0, 1, 2, etc.
    public string nombre;
    public Color color;
    public Tipo tipo;
    public Dificultad dificultad;
    
    public List<Provincia> provinciasControladas = new List<Provincia>();

    public Player(int _id, string _nombre, Color _color, Tipo _tipo, Dificultad _dificultad = Dificultad.Normal)
    {
        id = _id;
        nombre = _nombre;
        color = _color;
        tipo = _tipo;
        dificultad = _dificultad;
    }

    public void AgregarProvincia(Provincia provincia)
    {
        if (!provinciasControladas.Contains(provincia))
            provinciasControladas.Add(provincia);
    }

    public void EliminarProvincia(Provincia provincia)
    {
        provinciasControladas.Remove(provincia);
    }

    public int GetTotalTropas()
    {
        int total = 0;
        foreach (Provincia p in provinciasControladas)
            total += p.unidades;
        return total;
    }

    public int GetProvinciasControladas()
    {
        return provinciasControladas.Count;
    }

    public bool EstaVivo()
    {
        return provinciasControladas.Count > 0;
    }
}
