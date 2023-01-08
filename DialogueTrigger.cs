using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class DialogueTrigger : Area2D {
	[Export]
	public Array<string> lines = new Array<string>();

    private bool is_player_near = false;

	public override void _Ready() {
        BodyEntered += (other) => {
            var player = other as Player;
            if (player != null) {
                player.IsConfused = true;
                is_player_near = true;
            }
        };

		BodyExited += (other) => {
            var player = other as Player;
            if (player != null) {
                player.IsConfused = false;
                is_player_near = false;
            }
        };
    }

    public override void _Process(double delta) {
		if (Input.IsActionJustPressed("next_dialogue")) {
			var dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;
			var player = GetTree().Root.GetNode<Player>("Main/player");

			var dialogue_visible = dialogue?.Visible ?? false;
			if (player != null && player.IsConfused && is_player_near && dialogue != null && !dialogue.Visible) {
				dialogue.set_queue(new List<string>(lines));
			}
		}
    }
}
