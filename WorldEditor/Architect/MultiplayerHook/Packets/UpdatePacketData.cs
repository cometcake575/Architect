using Hkmp.Networking.Packet;

namespace Architect.MultiplayerHook.Packets;

public class UpdatePacketData : IPacketData
{
    public string Id;
    public string SceneName;

    public float X;
    public float Y;
    public float Z;
    
    public void WriteData(IPacket packet)
    {
        packet.Write(Id);
        packet.Write(SceneName);
        
        packet.Write(X);
        packet.Write(Y);
        packet.Write(X);
    }

    public void ReadData(IPacket packet)
    {
        Id = packet.ReadString();
        SceneName = packet.ReadString();

        X = packet.ReadFloat();
        Y = packet.ReadFloat();
        Z = packet.ReadFloat();
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}