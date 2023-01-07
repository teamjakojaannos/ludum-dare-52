using Godot;

public partial class Player : CharacterBody2D {
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

    private AnimatedSprite2D sprite;

    private bool is_dead;

    public override void _Ready() {
        GetNode<Area2D>("Hitbox").AreaEntered += HandleCollision;

        sprite = GetNode<AnimatedSprite2D>("Sprite");
        sprite.Animation = "default";

        dash_cooldown_timer = new Timer();
        dash_cooldown_timer.WaitTime = DashCooldown;
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

        is_dead = false;
    }

    private void Reset() {
        is_dead = false;

        is_dashing = false;
        dash_cooldown_timer.Stop();
        DashSweat.Emitting = false;
        DashPoof.Emitting = false;

        velocity = Vector2.Zero;
        current_move_smoothness = MoveSmoothness;
    }

    public void TransitionToRoom(Node2D entrypoint) {
        Position = entrypoint.Position;
        Reset();
    }

    private void HandleCollision(Area2D other) {
        var boss = other as BigBoss;
        if (boss != null) {
            is_dead = true;
        }

        var projectile = other as Projectile;
        if (projectile != null) {
            is_dead = true;
        }
    }


    public override void _Process(double delta) {
        if (is_dead) {
            RotationDegrees = -90.0f;
            sprite.Animation = "default";
            sprite.Stop();
            return;
        } else {
            RotationDegrees = 0.0f;

            if (!sprite.Playing) {
                sprite.Play();
            }
        }

        var input_direction = is_dashing
            ? velocity.Normalized()
            : Input.GetVector("move_left", "move_right", "move_up", "move_down");
        var speed = is_dashing ? DashSpeed : Speed;

        if (input_direction.LengthSquared() > 0.1f) {
            var new_velocity = input_direction * speed;
            var weight = current_move_smoothness * (1.0f - (float)delta);

            if (!is_dashing) {
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

        Velocity = velocity;
        MoveAndSlide();

        if (Velocity.LengthSquared() > 0.01f) {
            sprite.Animation = "walk";
            sprite.FlipH = Velocity.x < 0.0f;
        } else {
            sprite.Animation = "default";
            sprite.FlipH = false;
        }
    }
}
