using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovementController : MonoBehaviour{

    Rigidbody2D rb;
    private EnumDirection direction=EnumDirection.None;

    public ConnectServer conectServer;
    public ValidatePhysic validatePhysic;

    private float timer;
    private int tick = 0;

    //---------------------------------------------------------------

    /**
     * INICIALIZACION
     */
    void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    //---------------------------------------------------------------

    void Update() {
        timer += Time.deltaTime;

        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;

            byte[] data = new byte[508];
            data[0] = 2; //ActionCode 2.

            switch (direction) {
                case EnumDirection.Left:
                    data[1] = 1;
                    rb.velocity = new Vector2(-3f, rb.velocity.y);
                    break;
                case EnumDirection.Right:
                    data[1] = 2;
                    rb.velocity = new Vector2(3f, rb.velocity.y);
                    break;
                case EnumDirection.None:
                    data[1] = 0;
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    break;
            }

            if (conectServer.isConnected()) {
                Physics2D.Simulate(Time.fixedDeltaTime);
                //Debug.Log("tick:"+tick);
                conectServer.sendDataToServer(data);
                validatePhysic.savePositionBuffer(tick, transform.position);
                tick++;
            }

            
        }

        //rb.position = Vector2.MoveTowards(rb.position, targetPosition, Time.fixedDeltaTime);
    }

    //---------------------------------------------------------------

    public void setMovement(EnumDirection direction) {
        this.direction = direction;
    }
}

