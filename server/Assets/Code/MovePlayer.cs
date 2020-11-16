using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovePlayer : MonoBehaviour{
    Rigidbody2D rb;
    int tickExecuted = 0;

    public ServerMain serverMain;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void move(EnumDirection direction) {
        switch (direction) {
            case EnumDirection.Left:
                rb.velocity = new Vector2(-3f, rb.velocity.y);
                break;
            case EnumDirection.Right:
                rb.velocity = new Vector2(3f, rb.velocity.y);
                break;
            case EnumDirection.None:
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
        Physics2D.Simulate(Time.fixedDeltaTime);
        serverMain.sendStatusToClient(tickExecuted, rb.position);
        tickExecuted++;
    }
    

}