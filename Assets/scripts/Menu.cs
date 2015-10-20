using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public string IP = "142.232.154.95";
	public int port = 25001;
    private NetworkView nView;
    private Color color;

    public GameObject testSphere;

    void Start()
    {
        nView = GetComponent<NetworkView>();
        color = Color.red;
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
                bool useNat = !Network.HavePublicAddress();
                Network.InitializeServer(4, port, useNat);
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

                if (GUI.Button(new Rect(100, 150, 100, 25), "Change Color"))
                {
                    nView.RPC("AskChangeColor", RPCMode.Server);
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
        } // end else

    } // end OnGUI()

    [RPC]
    void AskChangeColor()
    {
        nView.RPC("ChangeColor", RPCMode.All);
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
