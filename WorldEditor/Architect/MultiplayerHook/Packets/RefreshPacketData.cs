using System.Collections.Generic;
using Architect.Objects;
using Architect.Util;
using Hkmp.Networking.Packet;
using Newtonsoft.Json;

namespace Architect.MultiplayerHook.Packets;

public class RefreshPacketData : IPacketData
{
    public List<ObjectPlacement> Edits;
    public string SceneName;
    
    public void WriteData(IPacket packet)
    {
        packet.Write(SceneName);
        var json = JsonConvert.SerializeObject(Edits, 
            ObjectPlacement.ObjectPlacementConverter.Instance, 
            ObjectPlacement.ObjectPlacementConverter.Vector3Converter);
        
        var bytes = ZipUtils.Zip(json);
        
        packet.Write(bytes.Length);
        foreach (var b in bytes) packet.Write(b);
    }

    public void ReadData(IPacket packet)
    {
        SceneName = packet.ReadString();
        
        var count = packet.ReadInt();
        
        var bytes = new byte[count];
        for (var i = 0; i < count; i++)
        {
            bytes[i] = packet.ReadByte();
        }

        var json = ZipUtils.Unzip(bytes);
        
        Edits = JsonConvert.DeserializeObject<List<ObjectPlacement>>(json);
    }

    public bool IsReliable => true;
    public bool DropReliableDataIfNewerExists => false;
}