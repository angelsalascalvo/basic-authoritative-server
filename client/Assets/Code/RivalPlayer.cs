using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class RivalPlayer {
    private int id;
    private GameObject gameObject;

    public RivalPlayer(int id) {
        this.id = id;
        gameObject = null;
    }

    public void SetId(int id) {
        this.id = id;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
    }

    public int GetId() {
        return id;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }

}