using System.Collections.Generic;
using Architect.Content.Custom;
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
        
        var netReceiver = clientApi.NetClient.GetNetworkReceiver<ServerPacketId>(this, HkmpHook.InstantiatePacket);
        
        netReceiver.RegisterPacketHandler<ServerRefreshPacketData>(ServerPacketId.Refresh, packet =>
        {
            if (EditorManager.IsEditing) return;
            
            Architect.GlobalSettings.Edits.SetPlacements(packet.SceneName, JsonConvert.DeserializeObject<List<ObjectPlacement>>(packet.SerializedEdits, CustomSceneData.PartConverter));
            EditorManager.ScheduleReloadScene();
        });
        
        netReceiver.RegisterPacketHandler<ServerWinPacketData>(ServerPacketId.Win, packet =>
        {
            ZoteTrophy.WinScreen(packet.WinnerName);
        });
    }

    public void Refresh()
    {
        if (!_api.NetClient.IsConnected) return;
        
        var scene = GameManager.instance.sceneName;
        var serialized = JsonConvert.SerializeObject(Architect.GlobalSettings.Edits.GetPlacements(scene), CustomSceneData.PartConverter, new Vector3Converter());
        
        _api.NetClient.GetNetworkSender<ServerPacketId>(this)
            .SendSingleData(ServerPacketId.Refresh, new ServerRefreshPacketData
            {
                SerializedEdits = serialized,
                SceneName = scene
            });
    }

    public void BroadcastWin()
    {
        if (!_api.NetClient.IsConnected) return;
        _api.NetClient.GetNetworkSender<ServerPacketId>(this)
            .SendSingleData(ServerPacketId.Win, new ServerWinPacketData
            {
                WinnerName = _api.ClientManager.Username
            });
    }

    protected override string Name => "Architect";
    protected override string Version => "0.1.0";
    public override bool NeedsNetwork => true;
}