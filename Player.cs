using Godot;

public partial class Player : Area2D {
    [Export]
    public float Speed = 200.0f;
	[Export]
    public float MoveSmoothness = 0.1f;
	[Export]
    public float StopSmoothness = 0.65f;

    private Vector2 Velocity = Vector2.Zero;

    public override void _Ready() {
    }


    public override void _Process(double delta) {
        var input_direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (input_direction.LengthSquared() > 0.1f) {
			var new_velocity = input_direction * Speed;
			var weight = MoveSmoothness * (1.0f - (float) delta);
			Velocity = Velocity.Lerp(new_velocity, 1.0f - weight);
		} else {
			Velocity = Velocity * (StopSmoothness * (1.0f - (float) delta));
		}

        Position = Position + Velocity * (float) delta;
    }
}
