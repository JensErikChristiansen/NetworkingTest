using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))] // because we will use rpc calls
public class NetworkManager : MonoBehaviour {

	public string IP = "142.232.154.95";
	public int port = 25002;
    private NetworkView nView;
    private Color color;

    public GameObject testSphere;

    public GameObject testObject;

    void Start()
    {
        nView = GetComponent<NetworkView>();
        color = Color.red;
    }

    //--------------------------------------------------------
    /// <summary>
    /// Starts the server.
    /// </summary>
    private void StartServer()
    {
        bool useNat = !Network.HavePublicAddress();
        Network.InitializeServer(4, port, false);
        MasterServer.RegisterHost("Jens", "Jens Test", "NetworkingTest");
    }

    void OnGUI()
	{
		if (Network.peerType == NetworkPeerType.Disconnected)
		{
            if (GUI.Button(new Rect(100, 100, 100, 25), "Start Client"))
            {
                Network.Connect(IP, port);
            }

            if (GUI.Button(new Rect(100, 125, 100, 25), "Start Server"))
            {
                StartServer();
                //Network.InitializeServer(4, port, useNat);
            }
        } // end if disconnected
        else
        {
            if (Network.peerType == NetworkPeerType.Client)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Client");
                if (GUI.Button(new Rect(100, 125, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }

                if (GUI.Button(new Rect(100, 150, 100, 25), "Spawn Object"))
                {
                    nView.RPC("AskSpawnObject", RPCMode.Server);
                }
            }

            if (Network.peerType == NetworkPeerType.Server)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Server");
                GUI.Label(new Rect(100, 125, 100, 25), "Connections: " + Network.connections.Length);
                if (GUI.Button(new Rect(100, 150, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }
            }

            if (GUI.Button(new Rect(Screen.width / 2, 10f, 150f, 30f), "Spawn Object"))
            {
                GetComponent<NetworkView>().RPC("AskSpawnObject", RPCMode.All);
            }
        } // end else

    } // end OnGUI()

    /// <summary>
    /// Ask server to create numbers for a cube spawn.
    /// </summary>
    [RPC]
    void AskSpawnObject()
    {
        if (Network.isServer)
        {
            float randomX = Random.Range(-Screen.width, Screen.width) / 100;
            float randomY = Random.Range(-Screen.height, Screen.height) / 100;
            GetComponent<NetworkView>().RPC("SpawnObject", RPCMode.All, randomX, randomY);
        }
    }

    /// <summary>
    /// Spawns a cube at a random position.
    /// </summary>
    /// <param name="randomValueX">Random value for X.</param>
    /// <param name="randomValueY">Random value for Y.</param>
    [RPC]
    void SpawnObject(float randomValueX, float randomValueY)
    {

        if (testObject != true)
        {
            testObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        else if (testObject.GetComponent<Renderer>().enabled == false)
        {
            testObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }
        Instantiate(testObject, new Vector3(randomValueX, randomValueY, 0), Quaternion.identity);
    }

    [RPC]
    void AskChangeColor()
    {
        if (Network.isServer)
        {
            nView.RPC("ChangeColor", RPCMode.All);
        }
    }

    [RPC]
    void ChangeColor()
    {
        if (color == Color.red)
        {
            color = Color.blue;
        } else
        {
            color = Color.red;
        }

        testSphere.GetComponent<Renderer>().material.color = color;
    } // end ChangeColor()

}
