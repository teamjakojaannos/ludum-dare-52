using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;


public partial class FertilizerBlocker : StaticBody2D {

    private Dialogue dialogue;

    [Export]
    public DialogueTrigger dialogue_trigger;

    [Export]
    public Array<string> dialogue_when_removable = new();

    [Export]
    public Array<string> dialogue_after_removed = new();

    private Player player;



    private bool removable = false;

    public override void _Ready() {
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue_trigger = GetNode<DialogueTrigger>("DialogueTrigger");

        player = GetTree().Root.GetNodeOrNull<Player>("Main/player");
        if (player == null) {
            GD.Print("Can't find player");
        }

        dialogue_trigger.Inactive = true;
    }

    public void make_removable() {
        removable = true;
        dialogue_trigger.Inactive = false;

        dialogue_trigger.lines.Clear();
        dialogue_trigger.lines = new Array<string>(dialogue_when_removable);

        dialogue.DialogueFinished += removal_dialogue_finished;
    }

    private void removal_dialogue_finished() {
        dialogue.DialogueFinished -= removal_dialogue_finished;

        if (player != null) {
            player.learn_dash();
        }

        this.Hide();

        dialogue.DialogueFinished += HeadingBackDialogue;
        dialogue.set_queue(new List<string>(dialogue_after_removed));
    }

    private void HeadingBackDialogue() {
        dialogue.DialogueFinished -= HeadingBackDialogue;
        //GetTree().Root.GetNode<Main>("Main").TeleportToGreenhouse();
        this.QueueFree();
    }

}
