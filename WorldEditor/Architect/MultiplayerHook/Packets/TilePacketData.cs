using System.Collections.Generic;
using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class TilePacketData : IPacketData
{
    public List<(int, int)> Tiles;
    public bool Empty;
    public string SceneName;

    public void WriteData(IPacket packet)
    {
        packet.Write(SceneName);
        packet.Write(Empty);
        packet.Write(Tiles.Count);
        foreach (var r in Tiles)
        {
            packet.Write(r.Item1);
            packet.Write(r.Item2);
        }
    }

    public void ReadData(IPacket packet)
    {
        SceneName = packet.ReadString();
        Empty = packet.ReadBool();
        Tiles = [];
        var count = packet.ReadInt();
        for (var i = 0; i < count; i++)
        {
            Tiles.Add((packet.ReadInt(), packet.ReadInt()));
        }
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}