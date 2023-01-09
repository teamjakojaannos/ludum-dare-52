using Godot;
using Godot.Collections;

public partial class FlyRoom : Room {
	[Export]
	public Array<string> EntryDialogueLines = new();

    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

        GetTree().CreateTimer(1.5f).Timeout += () => {
			var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
            dialogue.DialogueFinished += EntryDialogueFinished;
            dialogue.set_queue(new System.Collections.Generic.List<string>(EntryDialogueLines));

			var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
			camera.OverridePlayerPosition = true;
			camera.IntroOffset = GetNode<Node2D>("Frog").Position;
        };
    }

    private void EntryDialogueFinished() {
		var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue.DialogueFinished -= EntryDialogueFinished;
		
		var camera = GetNode<CameraFixedZoom>("/root/Main/Camera");
		camera.OverridePlayerPosition = false;
		camera.IntroOffset = Vector2.Zero;
    }
}
