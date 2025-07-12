using System.Linq;
using Architect.MultiplayerHook.Packets;
using Hkmp.Api.Server;
using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook;

public class WeServerAddon : ServerAddon
{
    public IPacketData InstantiatePacket(PacketId packetId)
    {
        return packetId switch
        {
            PacketId.Refresh => new RefreshPacketData(),
            PacketId.Win => new WinPacketData(),
            PacketId.Edit => new EditPacketData(),
            PacketId.Erase => new ErasePacketData(),
            PacketId.Update => new UpdatePacketData(),
            _ => null
        };
    }
    
    public override void Initialize(IServerApi serverApi)
    {
        Logger.Info("Initializing server-side Architect addon!");
        
        var netReceiver = serverApi.NetServer.GetNetworkReceiver<PacketId>(this, InstantiatePacket);
        var sender = serverApi.NetServer.GetNetworkSender<PacketId>(this);
        
        netReceiver.RegisterPacketHandler<RefreshPacketData>(PacketId.Refresh, (id, packet) =>
        {
            Logger.Info("Receiving Refresh Packet [SERVER]");
            sender.SendSingleData(PacketId.Refresh, packet, serverApi.ServerManager.Players
                    .Where(player => player.Id != id)
                    .Select(player => player.Id)
                    .ToArray()
                );
        });
        
        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, (id, packet) =>
        {
            Logger.Info("Receiving Erase Packet [SERVER]");
            sender.SendSingleData(PacketId.Erase, packet, serverApi.ServerManager.Players
                    .Where(player => player.Id != id)
                    .Select(player => player.Id)
                    .ToArray()
                );
        });
        
        netReceiver.RegisterPacketHandler<UpdatePacketData>(PacketId.Update, (id, packet) =>
        {
            Logger.Info("Receiving Update Packet [SERVER]");
            sender.SendSingleData(PacketId.Update, packet, serverApi.ServerManager.Players
                    .Where(player => player.Id != id)
                    .Select(player => player.Id)
                    .ToArray()
                );
        });
        
        netReceiver.RegisterPacketHandler<EditPacketData>(PacketId.Edit, (id, packet) =>
        {
            Logger.Info("Receiving Edit Packet [SERVER]");
            sender.SendSingleData(PacketId.Edit, packet, serverApi.ServerManager.Players
                    .Where(player => player.Id != id)
                    .Select(player => player.Id)
                    .ToArray()
                );
        });
        
        netReceiver.RegisterPacketHandler<WinPacketData>(PacketId.Win, (_, packet) =>
        {
            Logger.Info("Receiving Win Packet [SERVER]");
            sender.SendSingleData(PacketId.Win, packet, serverApi.ServerManager.Players.Select(player => player.Id).ToArray());
        });
    }

    protected override string Name => "Architect";
    protected override string Version => "1.8.6.0";
    public override bool NeedsNetwork => true;
}