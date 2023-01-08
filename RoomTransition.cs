using Godot;

public partial class RoomTransition : Area2D {
    [Export]
    public Room From;

    [Export]
    public Room To;

    [Export]
    public Node2D Entrypoint;

    [Export]
    public bool IsFirstRoom = false;

    public override void _Ready() {
        if (IsFirstRoom) {
            var player = GetTree().Root.GetNode<Player>("Main/player");
            TransitionToRoom(player);
            QueueFree();
        } else {
            BodyShapeEntered += (body_rid, other, body_shape_index, local_shape_index) => {
                var player = other as Player;
                if (player != null) {
                    TransitionToRoom(player);
                }
            };
        }
    }

    private void TransitionToRoom(Player player) {
        var camera = GetTree().Root.GetNode<Camera2D>("Main/Camera");
        camera.LimitLeft = To.CameraLimitLeft;
        camera.LimitRight = To.CameraLimitRight;
        camera.LimitTop = To.CameraLimitTop;
        camera.LimitBottom = To.CameraLimitBottom;
        camera.Position = To.Position;

        player.TransitionToRoom(Entrypoint);
        camera.ResetSmoothing();

        if (!IsFirstRoom) {
            // HACK: remove the tomato branch
            GetTree().Root.GetNodeOrNull<Node2D>("Main/IntroSequence")?.QueueFree();

            CallDeferred(new StringName(nameof(UpdateSceneTree)));
        }
    }

    private void UpdateSceneTree() {
        var rooms = GetTree().Root.GetNode("Main/Rooms");

        if (From != null) {
            From.OnPlayerExit();
            rooms.RemoveChild(From);
        }

        if (To != null) {
            rooms.AddChild(To);
            To.OnPlayerEnter();
        }
    }
}
