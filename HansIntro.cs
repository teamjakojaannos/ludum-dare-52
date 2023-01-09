using Godot;
using Godot.Collections;

public partial class HansIntro : Node2D {
    private Dialogue dialogue;
    private AnimationPlayer animation;
    private Player player;

	[Export]
	public Array<string> AngryFlyLines = new();
	[Export]
	public Array<string> HansComingInLines = new();

    public override void _Ready() {
        animation = GetNode<AnimationPlayer>("Animation");
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        player = GetTree().Root.GetNode<Player>("Main/player");
    }

    public void StartIntro() {
        animation.Play("HangryFlyEnter");
    }

    public void AngryFlyDialogue() {
        dialogue.DialogueFinished += AngryFlyDialogueFinished;
		dialogue.set_queue(new System.Collections.Generic.List<string>(AngryFlyLines));
    }

    private void AngryFlyDialogueFinished() {
        dialogue.DialogueFinished -= AngryFlyDialogueFinished;

		player.ProcessMode = ProcessModeEnum.Disabled;
		GetTree().CreateTimer(1.5f).Timeout += () => {
            player.ProcessMode = ProcessModeEnum.Inherit;

			dialogue.DialogueFinished += HansComingInDialogueFinished;
			dialogue.set_queue(new System.Collections.Generic.List<string>(HansComingInLines));
		};
    }

	public void HansComingInDialogueFinished() {
		dialogue.DialogueFinished -= HansComingInDialogueFinished;

		animation.Play("HansIntro");
        animation.AnimationFinished += HansLeft;
	}

    public void HansLeft(StringName name) {
        animation.AnimationFinished -= HansLeft;

        var lines = new System.Collections.Generic.List<string>();
        lines.Add("The fly dropped something!");
        dialogue.set_queue(lines);

        var blocker = GetNodeOrNull<FertilizerBlocker>("../Blocker");
        blocker.make_removable();
    }
}
