using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class MovePlayer : MonoBehaviour{
    Rigidbody2D rb;
    public int tickExecuted = -1;

    public ServerMain serverMain;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void move(EnumDirection direction, int tick) {
        
        //Comprobar si el tick en cuestion se ha ejecutado en el servidor
        //El envío redundante puede causarse debido a que el cliente cuando envia su mensaje de tick 
        //aun no haya recibido el acuse de recibo de un tick ejecutado en el servidor, o incluso que 
        //este mensaje se haya perdido en la comunicacion
        if (tick > tickExecuted) {
            tickExecuted = tick;
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
            Debug.Log("Procesado" + tick);
            serverMain.sendStatusToClient(tickExecuted, rb.position);
        }
    }
    

}