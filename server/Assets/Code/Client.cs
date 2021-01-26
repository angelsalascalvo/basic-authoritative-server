using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// Representacion del cliente o jugador conectado
/// </summary>
public class Client {
    //VAR STA
    private static readonly int limitTickQueueSize = 5;

    //PROP
    private int id;
    private IPEndPoint address;
    private GameObject gameObject;
    private InputTick lastTickExecuted;
    private int lastTickQueue;
    private Queue<InputTick> tickQueue;

    //------------------------------------------------------------->

    /// <summary>
    /// Constructor parametrizado
    /// </summary>
    /// <param name="id"></param>
    /// <param name="address"></param>
    public Client(int id, IPEndPoint address) {
        this.id = id;
        this.address = address;
        //Estado por defecto
        tickQueue = new Queue<InputTick>();
        lastTickQueue = -1;

        lastTickExecuted.tick = -1;
        lastTickExecuted.jump = false;
        lastTickExecuted.displacement = EnumDisplacement.None;
    }

    //------------------------------------------------------------->


    /// <summary>
    /// Registrar o agregar un nuevo tick con informacion de input 
    /// para el cliente controlando un tamanno maximo para la cola
    /// </summary>
    /// <param name="inputTick"></param>
    public void AddInputTick(InputTick inputTick) {
        if (tickQueue.Count > limitTickQueueSize)
            tickQueue.Dequeue();

        tickQueue.Enqueue(inputTick);
        lastTickQueue = inputTick.tick;
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public int GetId() {
        return id;
    }

    public IPEndPoint GetAddress() {
        return address;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }

    public Queue<InputTick> GetTickQueue() {
        return tickQueue;
    }

    public InputTick GetLastTickExecuted() {
        return lastTickExecuted;
    }
    public int GetLastTickQueue() { 
        return lastTickQueue;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
    }

    public void SetLastTickExecuted(InputTick lastTickExecuted) {
        this.lastTickExecuted = lastTickExecuted;
    }
}