using System.Collections.Generic;
using Architect.Content.Custom;
using Architect.MultiplayerHook.Packets;
using Architect.Objects;
using Architect.Util;
using Hkmp.Api.Client;
using Modding.Converters;
using Newtonsoft.Json;

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
            //if (EditorManager.IsEditing) return;
            
            Architect.GlobalSettings.Edits[packet.SceneName] = packet.Edits;
            EditorManager.ScheduleReloadScene();
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
    protected override string Version => "0.1.0";
    public override bool NeedsNetwork => true;
}