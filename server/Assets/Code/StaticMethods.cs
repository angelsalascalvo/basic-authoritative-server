using System;
using UnityEngine;

/// <summary>
/// Bateria de metodos estaticos para el debug y simulaciones de comportamientos de red reales 🎲
/// </summary>
public static class StaticMethods {

    /// <summary>
    /// Imprimer en consola un datagrama (array de bytes)
    /// </summary>
    /// <param name="rData"></param>
    public static void DebugDatagram(byte[] rData) {
        string t = "";
        foreach (byte a in rData) {
            t += a + " - ";
        }
        Debug.Log(t);
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Devuelve true o false en funcion de un porcentaje
    /// 0% = false; 100% = true;
    /// </summary>
    /// <param name="numPercent"></param>
    /// <returns></returns>
    public static bool Percent(short numPercent) {
        return RandomGenerator.GetRandomNumber() <= (numPercent / 100f);
    }
}


/////////////////////////////////////////////////////////////////////////////////


/// <summary>
/// Clase estatica para poder llamar al metodo de percent desde un hilo
/// </summary>
public static class RandomGenerator {
    private static object locker = new object();
    private static System.Random seedGenerator = new System.Random(Environment.TickCount);

    public static double GetRandomNumber() {
        int seed;

        lock (locker) {
            seed = seedGenerator.Next(int.MinValue, int.MaxValue);
        }

        var random = new System.Random(seed);

        return random.NextDouble();
    }
}