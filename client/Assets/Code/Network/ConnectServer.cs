using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/**
 * FUNCIONALIDAD DEL SCRIPT
 */
public class ConnectServer : MonoBehaviour{

    //// VAR PUB
    public short quantityDatagramLost = 10;//%

    //// REF PUB
    public string serverIp = "127.0.0.1";
    public int serverPort = 1999;
    public ValidatePhysic validatePhysic;

    //// VAR PRIV
    private Socket socket;
    private Thread thReceiveServer;
    private IPEndPoint serverAddress;
    private bool connect = false;
    public int lastTickServer = 0;
    private int myID=-1;

    ///////////////////////////////////////////////////////////////////////////////////////
    //                                      METODOS                                      //
    ///////////////////////////////////////////////////////////////////////////////////////

    /**
     * INICIALIZACION
     */
    void Start() {
        Time.fixedDeltaTime = 0.02f;
        /// Crear socket comunicacion con servidor
        serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(serverAddress);

        /// Escuchar respuestas del sevidor en hilo 
        thReceiveServer = new Thread(new ThreadStart(receiveFromServer));
        thReceiveServer.Start();

        /// Solicitar acceso a partida
        byte[] data = new byte[508];
        data[0] = 1;//ActionCode 1. 
        sendDataToServer(data);
    }

    //---------------------------------------------------------------

    /**
     * HILO RECIBIR MENSAJE DEL SERVIDOR
     */
    private void receiveFromServer() {

        byte[] dataRec;
        EndPoint server = serverAddress;

        while (true) {
            dataRec = new byte[508];
            socket.ReceiveFrom(dataRec, ref server);
            BinaryReader br = new BinaryReader(new MemoryStream(dataRec));

            //StaticMethods.debugDatagram(dataRec);

            switch (br.ReadByte()) {
                //Respuesta servidor a entrar en partida
                case 2:
                    if (br.ReadBoolean()) {
                        Debug.Log("Admitido en partida");
                        connect = true;
                        myID = br.ReadInt32();
                    }
                    break;
                //Mensaje de posiciones
                case 3:

                    byte length = br.ReadByte();
                    for (int i = 0; i < length; i++) {

                        int id = br.ReadInt32();
                        float posX = br.ReadSingle();
                        float posY = br.ReadSingle();
                        float velX = br.ReadSingle();
                        float velY = br.ReadSingle();

                        if (id != myID) {
                            //Jugador Rival
                            validatePhysic.ProcessStatusRival(id, new Vector2(posX, posY), new Vector2(velX, velY));
                        }
                    }                                  
                    break;
            }
        }
    }

    //---------------------------------------------------------------

    /**
     * METODO ENVIAR MENSAJE AL SERVIDOR
     */
    public void sendDataToServer(byte[] data) {
        //Enviar datos haciendo uso del socket
        socket.SendTo(data, serverAddress);
    }

    //---------------------------------------------------------------

    /**
     * EJECUCION AL FINALIZAR LA EJECUCION
     */
    private void OnDestroy() {
        //Matar hilos
        thReceiveServer.Abort();
        Debug.Log("Final");
    }


    ///////////////////////////////////////////////////////////////////////////////////////
    //                                     GET + SET                                     //
    ///////////////////////////////////////////////////////////////////////////////////////


    /**
     * ¿ESTA CONECTADO AL SERVIDOR?
     */
    public bool isConnected() {
        return connect;
    }

    public int getLastTickServer() {
        return lastTickServer;
    }
}