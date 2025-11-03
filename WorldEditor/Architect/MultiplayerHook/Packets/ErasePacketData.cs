using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class ErasePacketData : IPacketData
{
    public string Id;
    public string SceneName;

    public void WriteData(IPacket packet)
    {
        packet.Write(Id);
        packet.Write(SceneName);
    }

    public void ReadData(IPacket packet)
    {
        Id = packet.ReadString();
        SceneName = packet.ReadString();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}