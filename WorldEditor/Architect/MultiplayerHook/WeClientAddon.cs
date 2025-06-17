using Architect.Content.Elements.Custom;
using Architect.MultiplayerHook.Packets;
using Architect.Util;
using Hkmp.Api.Client;

namespace Architect.MultiplayerHook;

public class WeClientAddon : ClientAddon
{
    private IClientApi _api;
    
    public override void Initialize(IClientApi clientApi)
    {
        Logger.Info("Initializing client-side Architect addon!");
        _api = clientApi;
        
        var netReceiver = clientApi.NetClient.GetNetworkReceiver<PacketId>(this, HkmpHook.InstantiatePacket);
        
        netReceiver.RegisterPacketHandler<RefreshPacketData>(PacketId.Refresh, packet =>
        {
            Architect.GlobalSettings.Edits[packet.SceneName].Clear();
            Architect.GlobalSettings.Edits[packet.SceneName].AddRange(packet.Edits);
            
            if (packet.SceneName == GameManager.instance.sceneName) EditorManager.ScheduleReloadScene();
        });
        
        netReceiver.RegisterPacketHandler<WinPacketData>(PacketId.Win, packet =>
        {
            ZoteTrophy.WinScreen(packet.WinnerName);
        });
    }

    public void Refresh()
    {
        if (!_api.NetClient.IsConnected) return;
        
        var scene = GameManager.instance.sceneName;

        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Refresh, new RefreshPacketData
            {
                Edits = Architect.GlobalSettings.Edits[scene],
                SceneName = scene
            });
    }

    public void BroadcastWin()
    {
        if (!_api.NetClient.IsConnected) return;
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Win, new WinPacketData
            {
                WinnerName = _api.ClientManager.Username
            });
    }

    protected override string Name => "Architect";
    protected override string Version => "1.0.0";
    public override bool NeedsNetwork => true;
}