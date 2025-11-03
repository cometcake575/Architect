using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class WinPacketData : IPacketData
{
    public string WinnerName;

    public void WriteData(IPacket packet)
    {
        packet.Write(WinnerName);
    }

    public void ReadData(IPacket packet)
    {
        WinnerName = packet.ReadString();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}