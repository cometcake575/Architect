using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook;

public class ServerRefreshPacketData : IPacketData
{
    public string SerializedEdits;
    public string SceneName;
    
    public void WriteData(IPacket packet)
    {
        packet.Write(SerializedEdits);
        packet.Write(SceneName);
    }

    public void ReadData(IPacket packet)
    {
        SerializedEdits = packet.ReadString();
        SceneName = packet.ReadString();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}