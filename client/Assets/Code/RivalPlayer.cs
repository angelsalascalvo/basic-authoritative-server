using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class RivalPlayer {
    private int id;
    private GameObject gameObject;
    private Rigidbody2D rb;
    private Queue<Vector2> positionsQueue;
    private Vector2 startPosition, targetPosition;
    private float time;


    public RivalPlayer(int id) {
        this.id = id;
        gameObject = null;
        positionsQueue = new Queue<Vector2>();
        time = -1;
    }

    public void SetId(int id) {
        this.id = id;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
        this.rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public int GetId() {
        return id;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }

    public Rigidbody2D GetRigidbody2D() {
        return rb;
    }

    public Queue<Vector2> GetPositionsQueue() {
        return positionsQueue;
    }

    public void SetStartPosition(Vector2 startPosition) {
        this.startPosition = startPosition;
    }

    public void SetTargetPosition(Vector2 targetPosition) {
        this.targetPosition = targetPosition;
    }

    public Vector2 GetStartPosition() {
        return startPosition;
    }
    
    public Vector2 GetTargetPosition() {
        return targetPosition;
    }

    public void SetTime(float time) {
        this.time = time;
    }

    public float GetTime() {
        return time;
    }
}