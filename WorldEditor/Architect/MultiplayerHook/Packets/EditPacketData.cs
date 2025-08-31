using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class EditPacketData : IPacketData
{
    public byte[] Edits;
    public string SceneName;

    public void WriteData(IPacket packet)
    {
        packet.Write(SceneName);

        packet.Write(Edits.Length);
        foreach (var b in Edits) packet.Write(b);
    }

    public void ReadData(IPacket packet)
    {
        SceneName = packet.ReadString();

        var count = packet.ReadInt();
        Edits = new byte[count];

        for (var i = 0; i < count; i++) Edits[i] = packet.ReadByte();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}