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
    private Vector2 impulse_velocity = Vector2.Zero;
    private float current_move_smoothness = 0.0f;

    private bool is_dashing = false;

    private Timer dash_cooldown_timer;
    private Timer invulnerable_timer;

    private GPUParticles2D DashSweat;
    private GPUParticles2D DashPoof;

    private AnimatedSprite2D sprite;

    private AudioStreamPlayer SfxWalk;
    private AudioStreamPlayer SfxDash;

    private int last_sfx_walk_frame = 5;

    private int health = 3;

    private bool is_dead;

    public bool is_game_started;

    private Dialogue dialogue;

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

    private bool dash_learned = false;

    public bool InputFrozen = false;

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

        invulnerable_timer = new Timer();
        invulnerable_timer.WaitTime = 0.5f;
        invulnerable_timer.Autostart = false;
        invulnerable_timer.OneShot = true;
        AddChild(invulnerable_timer);

        is_game_started = false;

        SfxWalk = GetNode<AudioStreamPlayer>("SFX/Walk");
        SfxDash = GetNode<AudioStreamPlayer>("SFX/Dash");

        HardReset();
    }

    public void StartGame() {
        is_game_started = true;
    }

    private void HardReset() {
        DashSweat = GetNode<GPUParticles2D>("DashSweat");
        DashPoof = GetNode<GPUParticles2D>("DashParticles");
        DashSweat.Emitting = false;
        DashPoof.Emitting = false;

        dialogue = GetTree().Root.GetNode<Main>("Main")?.DialogueUI;

        health = 3;
        Reset();
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
        InputFrozen = false;
    }

    public void TransitionToRoom(Node2D entrypoint) {
        Position = entrypoint.Position;
        Reset();
    }

    private void HandleCollision(Area2D other) {
        var boss = other as BigBoss;
        if (boss != null) {
            TakeDamage(1);
            Knockback(350f, boss.Position);
        }

        var projectile = other as Projectile;
        if (projectile != null) {
            TakeDamage(1);
            Knockback(150f, projectile.Position);
        }
    }

    public void TakeDamage(int amount) {
        if (invulnerable_timer.IsStopped()) {
            health -= amount;

            if (health == 0) {
                is_dead = true;

                GetTree().CreateTimer(3.0f).Timeout += () => {
                    GetTree().Root.GetNode<Main>("Main").TeleportToGreenhouse();
                    HardReset();
                };
            }

            invulnerable_timer.Start();
            Blink(5, 0.15f);
        }
    }

    private void Blink(int blinks, float interval = 0.5f) {
        if (blinks == 0) {
            return;
        }

        var original_modulate = Modulate;
        GetTree().CreateTimer(interval / 2.0f, false).Timeout += () => {
            sprite.Modulate = new Color(0.0f, 0.0f, 0.0f);

            GetTree().CreateTimer(interval / 2.0f, false).Timeout += () => {
                sprite.Modulate = original_modulate;

                Blink(--blinks, interval);
            };
        };
    }

    public void Knockback(float force, Vector2 force_location) {
        var direction = force_location.DirectionTo(Position);

        impulse_velocity += direction * force;
    }

    private Vector2 InputDirection {
        get {
            return is_dead || dialogue.Visible || InputFrozen
                ? Vector2.Zero
                : is_dashing
                    ? velocity.Normalized()
                    : Input.GetVector("move_left", "move_right", "move_up", "move_down");
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
            is_dashing = false;
        } else {
            RotationDegrees = 0.0f;

            if (!sprite.Playing) {
                sprite.Play();
            }
            if (sprite.Animation.ToString().Match("walk*")) {
                var is_contact_frame = sprite.Frame == 1 || sprite.Frame == 5;

                if (is_contact_frame && last_sfx_walk_frame != sprite.Frame) {
                    SfxWalk.Play();
                    last_sfx_walk_frame = sprite.Frame;
                }
            }
        }

        if (InputDirection.LengthSquared() > 0.1f) {
            var new_velocity = InputDirection * (is_dashing ? DashSpeed : Speed);
            var weight = current_move_smoothness * (1.0f - (float)delta);

            if (!is_dashing) {
                current_move_smoothness = Mathf.Lerp(current_move_smoothness, MoveSmoothness, 0.01f);
            }
            velocity = velocity.Lerp(new_velocity, 1.0f - weight);
        } else {
            velocity = velocity * (StopSmoothness * (1.0f - (float)delta));
        }

        var is_dash_ready = !is_dead && dash_cooldown_timer.IsStopped();
        if (!InputFrozen && dash_learned && Input.IsActionJustPressed("dash") && is_dash_ready) {
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

        Velocity = velocity + impulse_velocity;
        MoveAndSlide();

        impulse_velocity = impulse_velocity.Lerp(Vector2.Zero, 0.1f);
        if (impulse_velocity.LengthSquared() < 0.01f) {
            impulse_velocity = Vector2.Zero;
        }

        if (!is_dead && velocity.LengthSquared() > 0.01f) {
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

    public void learn_dash() {
        dash_learned = true;
    }
}
