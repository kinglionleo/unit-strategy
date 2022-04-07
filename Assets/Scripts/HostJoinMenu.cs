using System;
using UnityEngine;
using Photon.Bolt;
using Photon.Bolt.Matchmaking;
using UdpKit;


public class HostJoinMenu : GlobalEventListener
{
    public void StartServer()
    {
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()
    {
        BoltMatchmaking.CreateSession(sessionID: "test", sceneToLoad: "OnlineTesting");
    }

    public void StartClient()
    {
        BoltLauncher.StartClient();

    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {

        // This will join the first available session
        foreach (var session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;

            if(photonSession.Source == UdpSessionSource.Photon)
            {
                BoltMatchmaking.JoinSession(photonSession);
            }
        }
    }
}
