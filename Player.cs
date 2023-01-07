using Godot;

public partial class Player : Area2D {
    [Export]
    public float Speed = 100.0f;
    [Export]
    public float MoveSmoothness = 0.1f;
    [Export]
    public float StopSmoothness = 0.65f;


    [Export]
    public float DashCooldown = 2.0f;

    [Export]
    public float DashDuration = 0.15f;

    [Export]
    public float DashSpeed = 1000.0f;
    [Export]

    public float DashSmoothness = 0.95f;

    private Vector2 velocity = Vector2.Zero;
    private float current_move_smoothness = 0.0f;

    private bool is_dashing = false;

    private Timer dash_cooldown_timer;

	private GPUParticles2D DashSweat;
	private GPUParticles2D DashPoof;

    public override void _Ready() {
        dash_cooldown_timer = new Timer();
        dash_cooldown_timer.Autostart = false;
        dash_cooldown_timer.OneShot = true;
        dash_cooldown_timer.Stop();
		dash_cooldown_timer.Timeout += () => {
			DashSweat.Emitting = false;
		};

        AddChild(dash_cooldown_timer);

		current_move_smoothness = MoveSmoothness;

		DashSweat = GetNode<GPUParticles2D>("DashSweat");
		DashPoof = GetNode<GPUParticles2D>("DashParticles");
		DashSweat.Emitting = false;
		DashPoof.Emitting = false;
    }


    public override void _Process(double delta) {
        var input_direction = is_dashing
			? velocity.Normalized()
			: Input.GetVector("move_left", "move_right", "move_up", "move_down");
		var speed = is_dashing ? DashSpeed : Speed;

        if (input_direction.LengthSquared() > 0.1f) {
            var new_velocity = input_direction * speed;
            var weight = current_move_smoothness * (1.0f - (float)delta);

			if (!is_dashing)  {
				current_move_smoothness = Mathf.Lerp(current_move_smoothness, MoveSmoothness, 0.01f);
			}
            velocity = velocity.Lerp(new_velocity, 1.0f - weight);
        } else {
            velocity = velocity * (StopSmoothness * (1.0f - (float)delta));
        }

        var is_dash_ready = dash_cooldown_timer.IsStopped();
        if (Input.IsActionJustPressed("dash") && is_dash_ready) {
            GetTree().CreateTimer(DashDuration, false).Timeout += () => {
                is_dashing = false;
            };

            is_dashing = true;
            dash_cooldown_timer.Start();
			current_move_smoothness = DashSmoothness;

			DashSweat.Emitting = true;
			DashPoof.Emitting = true;
        }

        Position = Position + velocity * (float)delta;
    }
}
