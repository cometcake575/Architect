using Architect.Content.Groups;
using HutongGames.PlayMaker.Actions;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Internal.Fixers;

internal sealed class TwisterPackElement : GInternalPackElement
{
    private readonly string _fsmName;
    
    public TwisterPackElement(string scene, string path, string name, string fsmName) : base(scene, path, name, "Enemies", 0)
    {
        WithBroadcasterGroup(BroadcasterGroup.Enemies);
        WithConfigGroup(ConfigGroup.Twisters);

        _fsmName = fsmName;
    }

    public override void PostSpawn(GameObject gameObject, bool flipped, float rotation, float scale)
    {
        var fsm = gameObject.LocateMyFSM(_fsmName);

        var teleplane = new GameObject(gameObject.name + " Teleplane")
        {
            tag = "Teleplane",
            transform =
            {
                position = gameObject.transform.position
            }
        };
        var collider = teleplane.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(10, 10);
        collider.isTrigger = true;
        
        fsm.DisableAction("Select Target", 1);
        fsm.InsertAction("Select Target", new FindGameObject
        {
            withTag = "Teleplane",
            objectName = teleplane.name,
            store = fsm.FsmVariables.GetFsmGameObject("Teleplane")
        }, 2);

        gameObject.AddComponent<Teleplane>().collider = collider;
    }
}

public class Teleplane : MonoBehaviour
{
    public BoxCollider2D collider;

    private void Start()
    {
        transform.position = collider.gameObject.transform.position;
    }
}