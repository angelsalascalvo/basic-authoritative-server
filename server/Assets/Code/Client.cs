using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client {
    private int id;
    private IPEndPoint address;
    private GameObject gameObject;
    private int lastTick;
    private Queue<InputTick> tickQueue;

    public Client(int id, IPEndPoint address) {
        this.id = id;
        this.address = address;
        tickQueue = new Queue<InputTick>();
        lastTick = -1;
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

    public int GetLastTick() { 
        return lastTick;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
    }

    //--------------------------------------------------

    public string Print() {
        return "ID: " + id + " | Address:" + address + " | GameObject: " + gameObject;
    }

    public void AddInputTick(InputTick inputTick) {
        tickQueue.Enqueue(inputTick);
        lastTick = inputTick.tick;
    }

   

}