using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public static class StaticMethods {

    /**
     * Devuelve true o false en funcion de un porcentaje
     * 0% = false; 100% = true;
     */
    public static bool percent(short numPercent) {
        return RandomGenerator.GetRandomNumber() <= (numPercent / 100f);
    }
}

//--------------------------------------------------------------------------------------------

/**
 * Clase estatica para poder llamar al metodo de percent desde un hilo
 */
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