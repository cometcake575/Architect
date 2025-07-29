namespace Architect.Util;

public static class Updater
{
    public static string UpdateConfig(string id)
    {
        return id switch
        {
            "sal_respawn_delay" => "sal_bubble_respawn_delay",
            "sal_y_track_length" => "sal_zipper_y_move",
            "sal_x_track_length" => "sal_zipper_x_move",
            "sal_top_spikes" => "sal_zipper_top_spikes",
            "sal_bot_spikes" => "sal_zipper_bot_spikes",
            "sal_left_spikes" => "sal_zipper_left_spikes",
            "sal_right_spikes" => "sal_zipper_right_spikes",
            "sal_rewind_speed" => "sal_zipper_rewind_speed",
            "sal_group_id" => "sal_switch_group",
            "sal_door_id" => "sal_door_group",
            "sal_x_move_dist" => "sal_door_x_move",
            "sal_y_move_dist" => "sal_door_y_move",
            "lore_tabet_content" => "lore_tablet_content",
            _ => id
        };
    }

    public static string UpdateObject(string name)
    {
        return name switch
        {
            "Coin Switch" => "Switch",
            "Coin Switch Door" => "Switch Door",
            "Spell Twister" => "Soul Twister",
            _ => name
        };
    }
}