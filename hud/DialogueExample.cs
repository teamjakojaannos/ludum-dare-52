using Godot;
using System;
using System.Collections.Generic;

public partial class DialogueExample : Control {

    [Export]
    public Dialogue dialogue;

    private List<List<string>> dialogues = new List<List<string>>
    {
        new List<string> { "This is useless chatter", "Blah blah blah", "i used to be an adventurer like you" },
        new List<string> { "I used to be an adventurer like you", "...", "but then i quit adventuring" },
        new List<string> { "what weapon does a fat jedi wield?", "...", "a HEAVY saber", "...", "....", "......", ".........", "hahaha" }
    };

    public override void _Ready() {
    }

    public void add_dialogue() {
        GD.Print("Adding dialogue");
        var random = new Random();
        var i = random.Next(3);
        var d = dialogues[i];
        this.dialogue.set_queue(d);
    }

    public void clear_dialogue() {
        GD.Print("Clearing dialogue");
        this.dialogue.clear_queue();
    }
}
