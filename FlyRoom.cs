using Godot;
using Godot.Collections;

public partial class FlyRoom : Room {
    [Export]
    public Array<string> EntryDialogueLines = new();

    private bool is_first_time = true;

    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

        if (is_first_time) {
            GetTree().CreateTimer(1.5f).Timeout += () => {
                var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
                dialogue.DialogueFinished += EntryDialogueFinished;
                dialogue.set_queue(new System.Collections.Generic.List<string>(EntryDialogueLines));

                var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
                camera.OverridePlayerPosition = true;
                camera.IntroOffset = GetNode<Node2D>("Frog").Position;

                is_first_time = false;
            };
        }
    }

    private void EntryDialogueFinished() {
        var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue.DialogueFinished -= EntryDialogueFinished;

        var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
        camera.OverridePlayerPosition = false;
        camera.IntroOffset = Vector2.Zero;

        GetNode<Frog>("Frog").indicator.Show();
    }
}
