using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class RefreshPacketData : IPacketData
{
    public byte[] Edits;
    public string Guid;
    public int PacketId;
    public string SceneName;
    public int TotalPackets;

    public void WriteData(IPacket packet)
    {
        packet.Write(PacketId);
        packet.Write(TotalPackets);
        packet.Write(SceneName);
        packet.Write(Guid);

        packet.Write(Edits.Length);
        foreach (var b in Edits) packet.Write(b);
    }

    public void ReadData(IPacket packet)
    {
        PacketId = packet.ReadInt();
        TotalPackets = packet.ReadInt();
        SceneName = packet.ReadString();
        Guid = packet.ReadString();

        var count = packet.ReadInt();

        var bytes = new byte[count];
        for (var i = 0; i < count; i++) bytes[i] = packet.ReadByte();

        Edits = bytes;
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}