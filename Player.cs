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

    private AudioStreamPlayer SfxWalk;
    private AudioStreamPlayer SfxDash;

    private int last_sfx_walk_frame = 5;

    private bool is_dead;

    public bool is_game_started;

    private bool is_confused;
    public bool IsConfused {
        get {
            return is_confused;
        }
        set {
            is_confused = value;
            if (value) {
                GetNode<Wat>("Wat").Show();
            } else {
                GetNode<Wat>("Wat").Hide();
            }
        }
    }

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
        is_game_started = false;

        SfxWalk = GetNode<AudioStreamPlayer>("SFX/Walk");
        SfxDash = GetNode<AudioStreamPlayer>("SFX/Dash");

        Reset();
    }

    public void StartGame() {
        is_game_started = true;
    }

    private void Reset() {
        is_dead = false;

        is_dashing = false;
        dash_cooldown_timer?.Stop();
        if (DashSweat != null) { DashSweat.Emitting = false; }
        if (DashPoof != null) { DashPoof.Emitting = false; }

        velocity = Vector2.Zero;
        current_move_smoothness = MoveSmoothness;

        IsConfused = false;
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
        if (!is_game_started) {
            return;
        }

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

        if (sprite.Animation.ToString().Match("walk*")) {
            var is_contact_frame = sprite.Frame == 1 || sprite.Frame == 5;

            if (is_contact_frame && last_sfx_walk_frame != sprite.Frame) {
                SfxWalk.Play();
                last_sfx_walk_frame = sprite.Frame;
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

            SfxDash.Play();

            is_dashing = true;
            dash_cooldown_timer.Start();
            current_move_smoothness = DashSmoothness;

            DashSweat.Emitting = true;
            DashPoof.Emitting = true;
        }

        Velocity = velocity;
        MoveAndSlide();

        if (Velocity.LengthSquared() > 0.01f) {
            if (Mathf.Abs(Velocity.x) > Mathf.Abs(Velocity.y)) {
                sprite.Animation = "walk";
                sprite.FlipH = Velocity.x < 0.0f;
            } else {
                sprite.Animation = Velocity.y > 0.0f ? "walk_down" : "walk_up";
            }
        } else {
            sprite.Animation = "default";
            sprite.FlipH = false;
        }
    }
}
