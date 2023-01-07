using Godot;

public partial class RoomTransition : Area2D {
    [Export]
    public Room From;

    [Export]
    public Room To;

    [Export]
    public Node2D Entrypoint;

    public override void _Ready() {

        BodyShapeEntered += (body_rid, other, body_shape_index, local_shape_index) => {
            var player = other as Player;
            if (player != null) {
                var camera = GetTree().Root.GetNode<Camera2D>("Main/Camera");
                camera.Position = To.Position;

                player.TransitionToRoom(Entrypoint);

                CallDeferred(new StringName(nameof(UpdateSceneTree)));
            }
        };
    }

    public override void _Process(double delta) {
    }

    private void UpdateSceneTree() {
        var rooms = GetTree().Root.GetNode("Main/Rooms");

        From.OnPlayerExit();
        rooms.RemoveChild(From);

        rooms.AddChild(To);
        To.OnPlayerEnter();
    }
}
