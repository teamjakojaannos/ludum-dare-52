using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class BossRoom : Room {
	[Export]
	public Array<string> BossIntroLines = new Array<string>();
	[Export]
	public Array<string> BossDialogueLines = new Array<string>();

    private AnimationPlayer animation;
    private Dialogue dialogue;

    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

		GetNodeOrNull<Control>("UICanvas/Lobu")?.Hide();

		animation = GetNode<AnimationPlayer>("Animation");
		GetNode<CanvasLayer>("UICanvas")?.Hide();
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        GetTree().CreateTimer(1.5f).Timeout += EntryDialogue;
    }

    public void EntryDialogue() {
        dialogue.DialogueFinished += BossIntro;
		dialogue.set_queue(new List<string>(BossIntroLines));
    }

    public void BossIntro() {
        dialogue.DialogueFinished -= BossIntro;
		animation.AnimationFinished += StartBossfight;
		animation.Play("BossIntro");
    }

    public void StartBossfight(StringName name) {
		animation.AnimationFinished -= StartBossfight;

		dialogue.DialogueFinished += BossDialogueFinished;
		dialogue.set_queue(new List<string>(BossDialogueLines));
    }

	public void BossDialogueFinished() {
		dialogue.DialogueFinished -= BossDialogueFinished;

		var boss = GetNode<BigBoss>("BigBoss");
		if (boss != null) {
			GetNode<CanvasLayer>("UICanvas")?.Show();
			boss.StartEncounter();
		}
	}
}
