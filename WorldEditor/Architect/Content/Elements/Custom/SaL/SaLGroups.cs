using Architect.Attributes.Config;
using Architect.Content.Groups;

namespace Architect.Content.Elements.Custom.SaL;

public static class SaLGroups
{
    public static ConfigGroup BubbleConfig;
    public static ConfigGroup BumperConfig;
    public static ConfigGroup ZipperConfig;
    public static ConfigGroup CoinConfig;
    public static ConfigGroup CoinDoorConfig;

    public static void InitializeConfig()
    {
        const string bubblePath = "HK8YPlando.Scripts.Platforming.BubbleController";
        BubbleConfig = new ConfigGroup(ConfigGroup.Generic,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Bubble Speed", (o, value) =>
            {
                SaLObjects.SetField(o, bubblePath, value.GetValue(), "Speed");
            }), "sal_bubble_speed"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Stall Time", (o, value) =>
            {
                SaLObjects.SetField(o, bubblePath, value.GetValue(), "StallTime");
            }), "sal_stall_time"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Respawn Delay", (o, value) =>
            {
                SaLObjects.SetField(o, bubblePath, value.GetValue(), "RespawnDelay");
            }), "sal_respawn_delay")
        );
        
        const string bumperPath = "HK8YPlando.Scripts.Platforming.Bumper";
        BumperConfig = new ConfigGroup(ConfigGroup.Generic,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Bump Strength", (o, value) =>
            {
                SaLObjects.SetField(o, bumperPath, value.GetValue(), "VerticalScale");
                SaLObjects.SetField(o, bumperPath, value.GetValue()/1.2f * SaLObjects.GetField(o, bumperPath, "HorizontalBumpX"), "HorizontalBumpX");
                SaLObjects.SetField(o, bumperPath, value.GetValue()/1.2f * SaLObjects.GetField(o, bumperPath, "HorizontalBumpY"), "HorizontalBumpY");
                SaLObjects.SetField(o, bumperPath, value.GetValue()/1.2f * SaLObjects.GetField(o, bumperPath, "HorizontalBumpYMax"), "HorizontalBumpYMax");
            }), "sal_bump_strength"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Oscillation Radius", (o, value) =>
            {
                SaLObjects.SetField(o, bumperPath, value.GetValue(), "OscillateRadius");
            }), "sal_oscillation_radius"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Oscillation Period", (o, value) =>
            {
                SaLObjects.SetField(o, bumperPath, value.GetValue(), "OscillatePeriod");
            }), "sal_oscillation_period")
        );
        
        const string zipperPath = "HK8YPlando.Scripts.Platforming.Zipper";
        
        ZipperConfig = new ConfigGroup(ConfigGroup.Generic,
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Y Track Length", (o, value) =>
            {
                SaLObjects.SetTrackY(o, zipperPath, value.GetValue());
            }), "sal_y_track_length"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("X Track Length", (o, value) =>
            {
                SaLObjects.SetTrackX(o, zipperPath, value.GetValue());
            }), "sal_x_track_length"),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Top Spikes", (o, value) =>
            {
                SaLObjects.SetTopSpikes(o, value.GetValue());
            }), "sal_top_spikes"),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Bottom Spikes", (o, value) =>
            {
                SaLObjects.SetBottomSpikes(o, value.GetValue());
            }), "sal_bot_spikes"),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Left Spikes", (o, value) =>
            {
                SaLObjects.SetLeftSpikes(o, value.GetValue());
            }), "sal_left_spikes"),
            Attributes.ConfigManager.RegisterConfigType(new BoolConfigType("Right Spikes", (o, value) =>
            {
                SaLObjects.SetRightSpikes(o, value.GetValue());
            }), "sal_right_spikes"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Start Speed", (o, value) =>
            {
                SaLObjects.SetField(o, zipperPath, value.GetValue(), "StartSpeed");
            }), "sal_start_speed"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Acceleration", (o, value) =>
            {
                SaLObjects.SetField(o, zipperPath, value.GetValue(), "Accel");
            }), "sal_acceleration"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Zipper Pause Time", (o, value) =>
            {
                SaLObjects.SetField(o, zipperPath, value.GetValue(), "PauseTime");
            }), "sal_pause_time"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Rewind Speed", (o, value) =>
            {
                SaLObjects.SetField(o, zipperPath, value.GetValue(), "RewindSpeed");
            }), "sal_rewind_speed"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Cooldown", (o, value) =>
            {
                SaLObjects.SetField(o, zipperPath, value.GetValue(), "RewindCooldown");
            }), "sal_cooldown")
        );

        const string coinPath = "HK8YPlando.Scripts.Platforming.Coin";
        
        CoinConfig = new ConfigGroup(ConfigGroup.Generic,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Group ID", (o, value) =>
            {
                SaLObjects.SetProperty(o, coinPath, value.GetValue(), "GateNumber");
            }, true), "sal_group_id")
        );

        const string coinDoorPath = "HK8YPlando.Scripts.Platforming.CoinDoor";
        
        CoinDoorConfig = new ConfigGroup(ConfigGroup.Stretchable,
            Attributes.ConfigManager.RegisterConfigType(new IntConfigType("Door ID", (o, value) =>
            {
                SaLObjects.SetProperty(o, coinDoorPath, value.GetValue(), "GateNumber");
            }, true), "sal_door_id"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("X Move Distance", (o, value) =>
            {
                SaLObjects.SetXMove(o, coinDoorPath, value.GetValue());
            }), "sal_x_move_dist"),
            Attributes.ConfigManager.RegisterConfigType(new FloatConfigType("Y Move Distance", (o, value) =>
            {
                SaLObjects.SetYMove(o, coinDoorPath, value.GetValue());
            }), "sal_y_move_dist")
        );
    }
}