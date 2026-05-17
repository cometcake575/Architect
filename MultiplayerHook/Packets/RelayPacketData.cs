using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class RelayPacketData : IPacketData
{
    public string EventName;

    public void WriteData(IPacket packet)
    {
        packet.Write(EventName);
    }

    public void ReadData(IPacket packet)
    {
        EventName = packet.ReadString();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}