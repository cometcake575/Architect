using System.Linq;
using Architect.MultiplayerHook.Packets;
using Hkmp.Api.Server;

namespace Architect.MultiplayerHook;

public class WeServerAddon : ServerAddon
{
    public override void Initialize(IServerApi serverApi)
    {
        Logger.Info("Initializing server-side Architect addon!");
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, HkmpHook.InstantiatePacket);
        
        netReceiver.RegisterPacketHandler<RefreshPacketData>(PacketId.Refresh, (id, packet) =>
        {
            serverApi.NetServer.GetNetworkSender<PacketId>(this)
                .SendSingleData(PacketId.Refresh, packet, serverApi.ServerManager.Players
                    .Where(player => player.Id != id)
                    .Select(player => player.Id)
                    .ToArray()
                );
        });
        
        netReceiver.RegisterPacketHandler<WinPacketData>(PacketId.Win, (_, packet) =>
        {
            serverApi.NetServer.GetNetworkSender<PacketId>(this)
                .SendSingleData(PacketId.Win, packet, serverApi.ServerManager.Players.Select(player => player.Id).ToArray());
        });
    }

    protected override string Name => "Architect";
    protected override string Version => "1.0.0";
    public override bool NeedsNetwork => true;
}