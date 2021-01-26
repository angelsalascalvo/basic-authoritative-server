using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Funcionalidad principal de la comunicación por socket con el servidor.
/// Envio y recepcion de datagramas.
/// </summary>
public class ConnectionServer : MonoBehaviour{

    //// VAR PUB
    public string serverIp = "127.0.0.1";
    public int serverPort = 1999;

    //// REF PUB
    public CorrectionsPhysic correctionsPhysic;
    public Button ConnecttionToServerButton;

    //// VAR PRIV
    private Socket socket;
    private Thread thReceiveServer;
    private IPEndPoint serverAddress;
    private bool connect = false;
    private int myID=-1;

    //------------------------------------------------------------->

    /// <summary>
    /// Inicializacion
    /// </summary>
    void Start() {

        //1. Crear socket comunicacion con servidor
        serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect(serverAddress);

        //2. Iniciar hilo de escucha de mensaje del servidor
        thReceiveServer = new Thread(new ThreadStart(receiveFromServer));
        thReceiveServer.Start();
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Enviar solicitud acceso al servidor
    /// </summary>
    public void ConnectToServer() {
        byte[] data = new byte[508];
        data[0] = 1;//ActionCode 1. 
        sendDataToServer(data);
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Recibir datagramas del servidor
    /// </summary>
    private void receiveFromServer() {

        byte[] dataRec;
        EndPoint server = serverAddress;

        while (true) {
            dataRec = new byte[508];
            socket.ReceiveFrom(dataRec, ref server);
            BinaryReader br = new BinaryReader(new MemoryStream(dataRec));
            //StaticMethods.debugDatagram(dataRec);

            //Leer ActionCode.
            switch (br.ReadByte()) {
                //Respuesta servidor a entrar en partida
                case 2:
                    if (br.ReadBoolean()) {
                        Debug.Log("Admitido en partida");
                        connect = true;
                        myID = br.ReadInt32();
                        //Desactivar el boton de conectar con servidor
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            ConnecttionToServerButton.gameObject.SetActive(false)
                        );
                       
                    } else {
                        Debug.LogWarning("No admitido en partida");
                    }
                    break;

                //Mensaje de posiciones
                case 3:
                    byte length = br.ReadByte();
                    //Recorrer la informacion de todos los clientes
                    for (int i = 0; i < length; i++) {

                        int id = br.ReadInt32();
                        int tick = br.ReadInt32();
                        float posX = br.ReadSingle();
                        float posY = br.ReadSingle();
                      
                        //Es informacion de un jugador rival
                        if (id != myID) {
                            correctionsPhysic.ProcessStatusRival(id, new Vector2(posX, posY));
                        //Es información de mi jugador
                        } else {
                            correctionsPhysic.CorrectPlayer(tick, new Vector2(posX, posY));
                        }
                    }                                  
                    break;
            }
        }
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Enviar un datagrama al servidor
    /// </summary>
    /// <param name="data"></param>
    public void sendDataToServer(byte[] data) {
        //Enviar datos haciendo uso del socket
        socket.SendTo(data, serverAddress);
    }

    //------------------------------------------------------------->

    /// <summary>
    /// Al finalizar la ejecucion
    /// </summary>
    private void OnDestroy() {
        //Finalizar hilo
        thReceiveServer.Abort();
    }


    //-------------------------------------------------------------//
    //                        SETs + GETs                          //
    //-------------------------------------------------------------//

    public bool isConnected() {
        return connect;
    }
}