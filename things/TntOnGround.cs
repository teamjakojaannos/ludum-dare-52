using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public partial class TntOnGround : Area2D {
    [Export]
    public Array<string> lines = new Array<string>();

    [Export]
    public Node2D Target;

    private bool is_player_near = false;

    public bool Inactive = false;

    public override void _Ready() {
        BodyEntered += (other) => {
            var player = other as Player;
            if (player != null && Visible) {
                player.IsConfused = true;
                is_player_near = true;
            }
        };

        BodyExited += (other) => {
            var player = other as Player;
            if (player != null && Visible) {
                player.IsConfused = false;
                is_player_near = false;
            }
        };
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("next_dialogue") && !Inactive) {
            var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
            var player = GetTree().Root.GetNode<Player>("Main/player");

            var dialogue_visible = dialogue?.Visible ?? false;
            if (player != null && player.IsConfused && is_player_near && dialogue != null && !dialogue.Visible) {

                dialogue.set_queue(new List<string>(lines));
                dialogue.DialogueFinished += pick_up;
            }
        }
    }

    private void pick_up() {
        var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
        dialogue.DialogueFinished -= pick_up;
        var player = GetTree().Root.GetNode<Player>("Main/player");
        player.has_tnt = true;

        CallDeferred(nameof(DeleteTarget));
    }

    private void DeleteTarget() {
        Target.QueueFree();
    }
}
