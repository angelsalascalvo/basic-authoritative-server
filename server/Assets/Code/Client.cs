using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client {
    //VAR STA
    private static int limitTickQueueSize = 5;

    //PROP
    private int id;
    private IPEndPoint address;
    private GameObject gameObject;
    private InputTick lastTickExecuted;
    private int lastTickQueue;
    private Queue<InputTick> tickQueue;
   

    public Client(int id, IPEndPoint address) {
        this.id = id;
        this.address = address;
        tickQueue = new Queue<InputTick>();
        lastTickQueue = -1;

        //Por defecto
        lastTickExecuted.tick = -1;
        lastTickExecuted.jump = false;
        lastTickExecuted.displacement = EnumDisplacement.None;
    }

    //---------------------------------------------------

    //// GET + SET ////

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
    //--------------------------------------------------

    public string Print() {
        return "ID: " + id + " | Address:" + address + " | GameObject: " + gameObject;
    }

    public void AddInputTick(InputTick inputTick) {
        //Controlar tamanno maximo de la cola
        if (tickQueue.Count > limitTickQueueSize)
            tickQueue.Dequeue();

        tickQueue.Enqueue(inputTick);
        lastTickQueue = inputTick.tick;
    }

   

}