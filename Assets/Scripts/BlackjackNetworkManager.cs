using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class BlackjackNetworkManager : NetworkManager {

    void Awake()
        {
            if (isHeadless())
            {
                StartServer();
            }
        }

    bool isHeadless()
    {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }
}
