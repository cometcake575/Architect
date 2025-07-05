using System.Threading.Tasks;
using Architect.MultiplayerHook.Packets;
using Architect.Objects;
using Hkmp.Api.Client;
using Hkmp.Api.Server;
using Hkmp.Networking.Packet;
using UnityEngine;

namespace Architect.MultiplayerHook;

public static class HkmpHook
{
    private static WeClientAddon _clientAddon;
    
    public static void Initialize()
    {
        _clientAddon = new WeClientAddon();
        ClientAddon.RegisterAddon(_clientAddon);
        ServerAddon.RegisterAddon(new WeServerAddon());
    }

    public static void Refresh()
    {
        Task.Run(() => _clientAddon.Refresh());
    }

    public static void Update(string guid, string scene, Vector3 pos)
    {
        _clientAddon.Update(guid, scene, pos);
    }

    public static void Erase(string guid, string scene)
    {
        _clientAddon.Erase(guid, scene);
    }

    public static void Place(ObjectPlacement placement, string scene)
    {
        _clientAddon.Place(placement, scene);
    }

    public static void BroadcastWin()
    {
        _clientAddon.BroadcastWin();
    }
}