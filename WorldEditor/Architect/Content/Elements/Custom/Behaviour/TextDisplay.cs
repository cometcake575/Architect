using Architect.Attributes;
using Satchel;
using UnityEngine;

namespace Architect.Content.Elements.Custom.Behaviour;

public class TextDisplay : MonoBehaviour
{
    private static TextDisplay _current;
    private static TextDisplay _prev;

    public int cost;
    public int displayType;
    private DisplayType _displayType;

    public string ID { get; private set; }

    private void Awake()
    {
        ID = gameObject.name;
    }

    private void Start()
    {
        _displayType = displayType switch
        {
            0 => new DreamDisplayType(),
            1 => new StillDreamDisplayType(),
            2 => new SpeakDisplayType(),
            _ => new ChoiceDisplayType()
        };
        _displayType.Start();
    }

    public static void Init()
    {
        On.PlayMakerFSM.Awake += (orig, self) =>
        {
            orig(self);
            if (self.FsmName != "Dialogue Page Control") return;

            self.InsertCustomAction("End Conversation", () =>
            {
                if (!_current) return;
                _current._displayType.Down();
                EventManager.BroadcastEvent(_current.gameObject, "BoxDown");
                _prev = _current;
                _current = null;
            }, 0);

            if (self.name != "Text YN") return;

            self.InsertCustomAction("Activate Geo Text?", fsm =>
            {
                if (_current) fsm.FsmVariables.FindFsmInt("Toll Cost").Value = _current.cost;
            }, 0);

            self.InsertCustomAction("Yes", () =>
            {
                if (!_prev) return;
                EventManager.BroadcastEvent(_prev.gameObject, "Yes");
                _prev = null;
            }, 0);

            self.InsertCustomAction("No", () =>
            {
                if (!_prev) return;
                EventManager.BroadcastEvent(_prev.gameObject, "No");
                _prev = null;
            }, 0);
        };
    }

    public void Display()
    {
        if (!isActiveAndEnabled) return;

        if (_current) _current._displayType.Down();

        _current = this;
        _displayType.Display(ID);
    }

    public abstract class DisplayType
    {
        public abstract void Start();

        public abstract void Display(string id);

        public abstract void Down();
    }

    public class DreamDisplayType : DisplayType
    {
        private PlayMakerFSM _dreamDialogue;

        public override void Start()
        {
            _dreamDialogue = GameObject.Find("_GameCameras/HudCamera/DialogueManager/Dream Msg").LocateMyFSM("Display");
        }

        public override void Display(string id)
        {
            _dreamDialogue.FsmVariables.FindFsmString("Convo Title").Value = id;
            _dreamDialogue.SendEvent("DISPLAY DREAM MSG ALT");
        }

        public override void Down()
        {
        }
    }

    public class StillDreamDisplayType : DisplayType
    {
        private PlayMakerFSM _manager;
        private DialogueBox _text;

        public override void Start()
        {
            _text = GameObject.Find("_GameCameras/HudCamera/DialogueManager/Text").GetComponent<DialogueBox>();
            _manager = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open Dream");
        }

        public override void Display(string id)
        {
            HeroController.instance.RelinquishControl();
            _manager.SendEvent("BOX UP DREAM");
            _text.StartConversation(id, "Custom");
        }

        public override void Down()
        {
            HeroController.instance.RegainControl();
            _manager.SendEvent("BOX DOWN DREAM");
        }
    }

    public class SpeakDisplayType : DisplayType
    {
        private PlayMakerFSM _manager;
        private DialogueBox _text;

        public override void Start()
        {
            _text = GameObject.Find("_GameCameras/HudCamera/DialogueManager/Text").GetComponent<DialogueBox>();
            _manager = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open");
        }

        public override void Display(string id)
        {
            HeroController.instance.RelinquishControl();
            _manager.SendEvent("BOX UP");
            _text.StartConversation(id, "Custom");
        }

        public override void Down()
        {
            HeroController.instance.RegainControl();
            _manager.SendEvent("BOX DOWN");
        }
    }

    public class ChoiceDisplayType : DisplayType
    {
        private PlayMakerFSM _manager;
        private DialogueBox _text;

        public override void Start()
        {
            _text = GameObject.Find("_GameCameras/HudCamera/DialogueManager/Text YN").GetComponent<DialogueBox>();
            _manager = GameObject.Find("_GameCameras/HudCamera/DialogueManager").LocateMyFSM("Box Open YN");
        }

        public override void Display(string id)
        {
            HeroController.instance.RelinquishControl();
            _manager.SendEvent("BOX UP YN");
            _text.StartConversation(id, "Custom");
        }

        public override void Down()
        {
            HeroController.instance.RegainControl();
            _manager.SendEvent("BOX DOWN YN");
        }
    }
}