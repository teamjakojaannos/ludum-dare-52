using Godot;

public partial class Main : Node2D {
	[Export]
	public Control WinUI;

	[Export]
	public Dialogue DialogueUI;

	[Export]
	public BackgroundMusic BackgroundMusicPlayer;

	private bool game_wonnered = false;
	private float bright_dist = 0.0f;
	private float darkness_dist_factor = 0.0f;

	private float target_bright_dist = 0.0f;
	private float target_darkness_dist_factor = 0.0f;

	public void Win() {
		WinUI.Show();
		game_wonnered = true;

		var lines = new System.Collections.Generic.List<string>();
		lines.Add("the little tomato had slain");
		lines.Add("the bit larger (but innocent)");
		lines.Add("robot lawn mower.");
		lines.Add("...");
		lines.Add("However; unbeknownst to them");
		lines.Add("The Heinouz Habanero had not");
		lines.Add("been a great success.");
		lines.Add("The Heinouz wasn't doing so great");
		lines.Add("financially at the moment.");
		lines.Add("...");
		lines.Add("The lawn mower, they had bought");
		lines.Add("just to irritate their neighbor");
		lines.Add("would now lead to the demise");
		lines.Add("of their business.");
		lines.Add("They were sued...");
		lines.Add("...for insurance fraud!");
		lines.Add("...");
		lines.Add("Heh, no-one would believe YOU");
		lines.Add("either, if you tried to claim");
		lines.Add("a little tomato broke a lawn mower");
		lines.Add("...");
		lines.Add("The Heinouz Company went bankrupt.");
		lines.Add("Habaneros were free again");
		lines.Add("The crops unionised");
		lines.Add("...and lived happily ever after!");
		DialogueUI.set_queue(lines);

		var overlay = GetNode<Node2D>("Overlay");
		var overlay_shader = overlay.Material as ShaderMaterial;
		bright_dist = 200.0f;
		darkness_dist_factor = 10.0f;
		overlay_shader.SetShaderParameter("bright_dist", bright_dist);
		overlay_shader.SetShaderParameter("darkness_dist_factor", darkness_dist_factor);
		
		DialogueUI.DialogueFinished += () => {
			// TODO: roll the credits
			GetTree().CreateTimer(1.5f).Timeout += () => {
				GetTree().Quit();
			};
		};

		target_bright_dist = 0.0f;
		target_darkness_dist_factor = 0f;

		overlay.Show();
	}

	public void Lose() {
		// TODO
	}

	public void Reset() {
		WinUI.Hide();
		DialogueUI.Hide();
	}

	public override void _Ready() {
		Reset();
	}

	public void TeleportToGreenhouse() {
		var player = GetNode<Player>("player");
		var transition = GetNode<RoomTransition>("InitialRoomTransition");
		transition.IsFirstRoom = false;
		transition.TransitionToRoom(player);
	}

	public override void _Process(double delta) {
		if (game_wonnered) {
			var overlay = GetNode<Node2D>("Overlay");
			var overlay_shader = overlay.Material as ShaderMaterial;

			bright_dist = Mathf.Lerp(bright_dist, target_bright_dist, 0.01f);
			darkness_dist_factor = Mathf.Lerp(darkness_dist_factor, target_darkness_dist_factor, 0.01f);
			overlay_shader.SetShaderParameter("bright_dist", bright_dist);
			overlay_shader.SetShaderParameter("darkness_dist_factor", darkness_dist_factor);
		}
	}
}
