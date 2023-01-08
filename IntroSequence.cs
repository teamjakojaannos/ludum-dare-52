using Godot;
using Godot.Collections;

public partial class IntroSequence : AnimatedSprite2D {
	[Export]
	public Array<string> FallDialogueLines = new Array<string>();

	[Export]
	public Array<string> FallDialogue2Lines = new Array<string>();

    private Dialogue dialogue;
    private Player player;

    public override void _Ready() {
        player = GetTree().Root.GetNode<Player>("Main/player");
        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;

        player.Hide();

        Animation = "fall";
        Stop();

        GetTree().CreateTimer(2.0f).Timeout += StartSequence;
    }

    public void StartSequence() {
        AnimationFinished += Fall;
        Play("fall");
    }

    private void Fall() {
        AnimationFinished -= Fall;

        dialogue.DialogueFinished += FallDialogueFinished;
		dialogue.set_queue(new System.Collections.Generic.List<string>(FallDialogueLines));
    }

    private void FallDialogueFinished() {
        dialogue.DialogueFinished -= FallDialogueFinished;

        GetTree().CreateTimer(2.0f).Timeout += () => {
            AnimationFinished += GetUp;
            Play("get_up");
        };
    }

    private void GetUp() {
        AnimationFinished -= GetUp;
        player.Show();

        var player_sprite = player.GetNode<AnimatedSprite2D>("Sprite");

        player_sprite.Play("default");
        player_sprite.AnimationFinished += Monologue;

        Play("idle");
    }

    private void Monologue() {
		var player_sprite = player.GetNode<AnimatedSprite2D>("Sprite");
		player_sprite.AnimationFinished -= Monologue;

        dialogue.DialogueFinished += ReleasePlayer;
		dialogue.set_queue(new System.Collections.Generic.List<string>(FallDialogue2Lines));
    }

    private void ReleasePlayer() {
		dialogue.DialogueFinished -= ReleasePlayer;

        if (!player.is_game_started) {
            player.StartGame();
        }
    }
}
