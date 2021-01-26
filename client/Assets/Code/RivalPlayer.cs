using UnityEngine;

/// <summary>
/// Representacion de un jugador rival
/// </summary>
public class RivalPlayer {
    //PROP
    private int id;
    private GameObject gameObject;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private float time;

    //------------------------------------------------------------->

    /// <summary>
    /// Constructor parametrizado
    /// </summary>
    /// <param name="id"></param>
    public RivalPlayer(int id) {
        this.id = id;
        gameObject = null;
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public void SetId(int id) {
        this.id = id;
    }

    public void SetGameObject(GameObject gameObject) {
        this.gameObject = gameObject;
        this.rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void SetTargetPosition(Vector2 targetPosition) {
        this.targetPosition = targetPosition;
    }

    public void SetTime(float time) {
        this.time = time;
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


    public Vector2 GetTargetPosition() {
        return targetPosition;
    }

    public float GetTime() {
        return time;
    }
}