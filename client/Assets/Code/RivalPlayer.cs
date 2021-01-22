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

    public RivalPlayer(int id) {
        this.id = id;
        gameObject = null;
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

}