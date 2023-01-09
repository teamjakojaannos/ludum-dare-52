using Godot;

public partial class SpawnRoom : Room {
    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

		var player = GetTree().Root.GetNode<Player>("Main/player");
		var hans = GetNode<Node2D>("Hans");
		if (player.dash_learned) {
			hans.Show();
		} else {
			hans.Hide();
		}
    }
}
