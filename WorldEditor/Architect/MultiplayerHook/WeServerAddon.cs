using System.Linq;
using Hkmp.Api.Server;

namespace Architect.MultiplayerHook;

public class WeServerAddon : ServerAddon
{
    public override void Initialize(IServerApi serverApi)
    {
        Logger.Info("Initializing server-side Architect addon!");
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<ServerPacketId>(this, HkmpHook.InstantiatePacket);
        
        netReceiver.RegisterPacketHandler<ServerRefreshPacketData>(ServerPacketId.Refresh, (_, packet) =>
        {
            serverApi.NetServer.GetNetworkSender<ServerPacketId>(this)
                .SendSingleData(ServerPacketId.Refresh, packet, serverApi.ServerManager.Players.Select(player => player.Id).ToArray());
        });
        
        netReceiver.RegisterPacketHandler<ServerWinPacketData>(ServerPacketId.Win, (_, packet) =>
        {
            serverApi.NetServer.GetNetworkSender<ServerPacketId>(this)
                .SendSingleData(ServerPacketId.Win, packet, serverApi.ServerManager.Players.Select(player => player.Id).ToArray());
        });
    }

    protected override string Name => "Architect";
    protected override string Version => "0.1.0";
    public override bool NeedsNetwork => true;
}