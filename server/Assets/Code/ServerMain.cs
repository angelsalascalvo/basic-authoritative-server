using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
/**
* FUNCIONALIDAD DEL SCRIPT
*/
public class ServerMain : MonoBehaviour{


    public string ipServer;
    public int portServer;
    public MovePlayer movePlayer;
    


    private Socket serverSocket;
    private IPEndPoint serverAddress, clientAddress;
    private Thread thListenClients;

    /**
    * INICIALIZACION
    */
    void Start() {

        //1. Crear Socket
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        serverAddress = new IPEndPoint(IPAddress.Parse(ipServer), portServer);
        serverSocket.Bind(serverAddress);

        //2. Iniciar hilo de escucha
        thListenClients = new Thread(new ThreadStart(listen));
        thListenClients.Start();
    }

    //---------------------------------------------------------------

    /**
     * METODO PARA ESCUCHAR CONEXIONES DE CLIENTES
     */
    private void listen() {
        Debug.Log("Servidor escuchando...");
        EndPoint clientAddAux = serverAddress; //Inicializar el objeto para evitar errores
        byte[] dataRec, dataSend;

        while (true) {
            dataRec = new byte[508];
            try {
                serverSocket.ReceiveFrom(dataRec, ref clientAddAux);
            } catch (SocketException e) {
                Debug.LogError(e.ErrorCode);

            }
            clientAddress = clientAddAux as IPEndPoint;
            BinaryReader br = new BinaryReader(new MemoryStream(dataRec));

            //Leer ActionCode.
            switch (br.ReadByte()) {
                //Solicitud entrada a partida
                case 1:

                    dataSend = new byte[508];
                    BinaryWriter bw = new BinaryWriter(new MemoryStream(dataSend));
                    bw.Write((byte)2); //ActionCode 2. 
                    bw.Write(true); //Marca de admitido

                    serverSocket.SendTo(dataSend, clientAddress);
                    break;

                //Movimiento del personaje
                case 2:
                    int lengthTicks = br.ReadInt32(); //Cantidad de ticks enviados por el datagrama
                    //Recorrer cada tick recibido
                    for (int i = 0; i < lengthTicks; i++) {
                           
                        //Leer datos del datagrama
                        int tick = br.ReadInt32();
                        //Parsear byte to enum
                        EnumDirection dir = (EnumDirection)Enum.ToObject(typeof(EnumDirection), br.ReadByte());
                                   
                        UnityMainThreadDispatcher.Instance().Enqueue(() => movePlayer.move(dir, tick));
                            
                    }

                    break;
            }
        }
    }

    //------------------------------------------------------------------------

    /**
     * METODO ENVIAR MENSAJE DE ESTADO AL CLIENTE
     */
    public void sendStatusToClient(int tick, Vector2 position) {
        byte[] dataSend = new byte[508];
        BinaryWriter bw = new BinaryWriter(new MemoryStream(dataSend));
        bw.Write((byte)3); //ActionCode 3
        //Datos
        bw.Write(tick);
        bw.Write(position.x);
        bw.Write(position.y);

        //Enviar datos
        serverSocket.SendTo(dataSend, clientAddress); //Sin simular latencia
    }

    
}

