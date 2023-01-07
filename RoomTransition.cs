using Godot;

public partial class RoomTransition : Area2D {
    [Export]
    public Room From;
    [Export]
    public Room To;

    public override void _Ready() {
		BodyShapeEntered += (body_rid, other, body_shape_index, local_shape_index) => {
			var player = other as Player;
			if (player != null) {
				From.OnPlayerExit();
				To.OnPlayerEnter();

				var camera = GetTree().Root.GetNode<Camera2D>("Main/Camera");
				camera.Position = To.Position;
			}
		};
    }

    public override void _Process(double delta) {
    }
}
