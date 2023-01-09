using Godot;

public partial class SpawnRoom : Room {
    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

		var player = GetTree().Root.GetNode<Player>("Main/player");
		var hans = GetNode<Node2D>("Hans");
		var gretaA = GetNode<Node2D>("GretaA");
		var gretaB = GetNode<Node2D>("GretaB");
		if (player.dash_learned) {
			hans?.Show();

			gretaA?.QueueFree();
			gretaB?.Show();
		} else {
			hans.Hide();

			gretaB?.Hide();
			gretaA?.Show();
		}
    }
}
