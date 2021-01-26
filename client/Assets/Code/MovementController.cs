using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// Control del movimiento del personaje local
/// </summary>
public class MovementController : MonoBehaviour{
    
    //// REF PUB
    public ConnectionServer connectServer;
    public CorrectionsPhysic correctionsPhysic;
    // 🎲 Simulacion latencia
    [Header("Latency Simulate")]
    public Slider sliderLatency;
    public Text textLatency;
    // 🎲 Simulacion perdida de datagramas
    [Header("Lost Datagram Simulate")]
    public Slider sliderLostDatagram;
    public Text textLostDatagram;

    //// VAR PRI
    private Rigidbody2D rb;
    private EnumDisplacement displacement = EnumDisplacement.None;
    private bool jump = false;
    private float timer;
    private int tick = 0;
    private Queue<InputTick> tickQueue;

    //VAR STA
    private static readonly int limitTickQueueSize = 5;

    //---------------------------------------------------------------

    /// <summary>
    /// Inicializacion
    /// </summary>
    void Start(){
        Time.fixedDeltaTime = 0.02f; //Tickrate (1s / 0.02s = 50 ticks por segundo)
        rb = GetComponent<Rigidbody2D>();
        tickQueue = new Queue<InputTick>();
    }

    //---------------------------------------------------------------

    /// <summary>
    /// Ejecucion cada fotograma
    /// </summary>
    void Update() {

        //🎲 Simulacion. Actualizar datos de la interfaz gráfica
        textLostDatagram.text = "Perdida envío datagramas: " + (int)sliderLostDatagram.value + "%";
        textLatency.text = "Latencia de envío: " + (int)sliderLatency.value + "ms";

        //Ejecucion cada tick (tickrate)
        timer += Time.deltaTime;
        while (timer >= Time.fixedDeltaTime) {
            timer -= Time.fixedDeltaTime;
            if (connectServer.isConnected() ){

                //Crear datagrama para enviar al servidor
                byte[] data = new byte[508];
                BinaryWriter bw = new BinaryWriter(new MemoryStream(data));
                bw.Write((byte)2); //ActionCode 2.

                //Leer entradas y actuar
                switch (displacement) {
                    case EnumDisplacement.Left:
                        //Segun la varacion del personaje local con el servidor,
                        //aumentar o disminuir velocidad para realizar una correccion en esta variacion
                        if (correctionsPhysic.GetDiff().x > 0.02f) {
                            rb.velocity = new Vector2(-3.3f, rb.velocity.y);
                        } else if(correctionsPhysic.GetDiff().x < -0.02f) {
                            rb.velocity = new Vector2(-2.7f, rb.velocity.y);
                        } else {
                            rb.velocity = new Vector2(-3f, rb.velocity.y);
                        }
                        break;
                    case EnumDisplacement.Right:
                        //Segun la varacion del personaje local con el servidor,
                        //aumentar o disminuir velocidad para realizar una correccion en esta variacion
                        if (correctionsPhysic.GetDiff().x < -0.02f) {
                            rb.velocity = new Vector2(3.3f, rb.velocity.y);
                        } else if (correctionsPhysic.GetDiff().x > 0.02f) {
                            rb.velocity = new Vector2(2.7f, rb.velocity.y);
                        } else {
                            rb.velocity = new Vector2(3f, rb.velocity.y);
                        }
                        break;
                    case EnumDisplacement.None:
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        break;
                }

                if (jump) {
                    rb.velocity = new Vector2(rb.velocity.x, 5f);
                }

                //Almacenar tick en el cola
                InputTick inputTick;
                inputTick.tick = tick;
                inputTick.displacement = displacement;
                inputTick.jump = jump;
                AddInputTick(inputTick);
              
                //Recorrer los inputs almacenados y enviar al servidor
                //Envio de ticks duplicados para corregir posibles perdidas
                //producidas en la comunicación.
                bw.Write((short)tickQueue.Count);
                foreach (InputTick it in tickQueue) {
                    bw.Write(it.tick);
                    bw.Write((byte)it.displacement);
                    bw.Write(it.jump);
                }
                //Enviar tick al servidor
                SendToClientSimulate(data);

                //Ejecutar fisica
                Physics2D.Simulate(Time.fixedDeltaTime);

                //Almacenar la posicion obtenida en local asociada al tick
                correctionsPhysic.savePositionBuffer(tick, transform.position);

                //Incrementar el tick
                tick++;
            }
        }
    }


    //------------------------------------------------------------->

    /// <summary>
    /// Registrar o agregar un nuevo tick con informacion de input
    /// controlando un tamanno maximo para la cola
    /// </summary>
    /// <param name="inputTick"></param>
    public void AddInputTick(InputTick inputTick) {
        //Controlar tamanno maximo de la cola
        if (tickQueue.Count > limitTickQueueSize)
            tickQueue.Dequeue();

        tickQueue.Enqueue(inputTick);
    }

    //------------------------------------------------------------->

    /// <summary>
    /// 🎲 Simulación del envío de información en una conexión UDP entre nodos remotos.
    /// Posible perdida de datagramas
    /// </summary>
    /// <param name="data"></param>
    /// <param name="clientAddress"></param>
    public void SendToClientSimulate(byte[] data) {
        //🎲 Simulacion perdida datagramas: Probabilidad de que el datagrama se pierda
        if (StaticMethods.Percent((short)sliderLostDatagram.value)) {
            Debug.LogWarning("Paquete perdido");
        } else {
            //🎲 Simulacion de latencia
            StartCoroutine(sendToServerSimulateLatency(data));
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// 🎲 Simulación del envío de información en una conexión UDP entre nodos remotos.
    /// Posible latencia en el envío
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator sendToServerSimulateLatency(byte[] data) {
        yield return new WaitForSeconds(sliderLatency.value/1000);
        connectServer.sendDataToServer(data);
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public void setMovement(EnumDisplacement displacement) {
        this.displacement = displacement;
    }

    public void setJump(bool jump) {
        this.jump = jump;
    }
}