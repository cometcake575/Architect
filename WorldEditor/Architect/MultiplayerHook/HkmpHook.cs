using Hkmp.Api.Client;
using Hkmp.Api.Server;
using Hkmp.Networking.Packet;

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
        _clientAddon.Refresh();
    }

    public static void BroadcastWin()
    {
        _clientAddon.BroadcastWin();
    }
    
    public static IPacketData InstantiatePacket(ServerPacketId packetId)
    {
        return packetId switch
        {
            ServerPacketId.Refresh => new ServerRefreshPacketData(),
            ServerPacketId.Win => new ServerWinPacketData(),
            _ => null
        };
    }
}