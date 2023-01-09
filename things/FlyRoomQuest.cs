using Godot;
using Godot.Collections;

public partial class FlyRoomQuest : Node {

    private HansIntro intro;
    private FertilizerBlocker blocker;

    [Export]
    public Array<string> FrogAsleepLines = new();


    public override void _Ready() {
        var frog = GetNodeOrNull<Frog>("../Frog");
        if (frog == null) {
            GD.Print("Can't find frog");
        } else {
            frog.FrogStartsSleeping += on_frog_sleep;
        }

        blocker = GetNodeOrNull<FertilizerBlocker>("../Blocker");
        if (blocker == null) {
            GD.Print("Can't find blocker.");
        }

        intro = GetParent().GetNode<HansIntro>("HansIntro");
    }


    public void on_frog_sleep() {
        var frog = GetNodeOrNull<Frog>("../Frog");

        var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
        camera.OverridePlayerPosition = true;
        camera.IntroOffset = frog.Position;

        GetTree().CreateTimer(2.5f).Timeout += () => {
            var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
            dialogue.DialogueFinished += DialogueFinished;

            dialogue.set_queue(new System.Collections.Generic.List<string>(FrogAsleepLines));
        };
    }

    private void DialogueFinished() {
        var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue.DialogueFinished -= DialogueFinished;

        var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
        camera.OverridePlayerPosition = false;
        camera.IntroOffset = Vector2.Zero;

        GetTree().Root.GetNode<Main>("Main")?.BackgroundMusicPlayer.Stop();
        intro.StartIntro();
    }
}
