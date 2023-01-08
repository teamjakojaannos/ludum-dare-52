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



    private bool removable = false;

    public override void _Ready() {
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue_trigger = GetNode<DialogueTrigger>("DialogueTrigger");
    }

    public void make_removable() {
        removable = true;

        dialogue_trigger.lines.Clear();
        dialogue_trigger.lines = new Array<string>(dialogue_when_removable);

        dialogue.DialogueFinished += removal_dialogue_finished;
    }

    private void removal_dialogue_finished() {
        dialogue.DialogueFinished -= removal_dialogue_finished;

        this.QueueFree();

        dialogue.set_queue(new List<string>(dialogue_after_removed));
    }

}
