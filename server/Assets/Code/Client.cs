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
    private int lastTickExecuted;
    private int lastTickQueue;
    private Queue<InputTick> tickQueue;
   

    public Client(int id, IPEndPoint address) {
        this.id = id;
        this.address = address;
        tickQueue = new Queue<InputTick>();
        lastTickQueue = -1;
        lastTickExecuted = -1;
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

    public int GetLastTickExecuted() {
        return lastTickExecuted;
    }
    public int GetLastTickQueue() { 
        return lastTickQueue;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
    }

    public void SetLastTickExecuted(int lastTickExecuted) {
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