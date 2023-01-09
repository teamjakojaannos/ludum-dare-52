using Godot;

public partial class BigBoss : Area2D {
    [Export]
    public float Speed = 1.0f;

    [Export]
    public float DashSpeed = 150.0f;

    [Export]
    public float DashCooldown = 5.0f;

    [Export]
    public TextureProgressBar Healthbar;

    private Path2D path;
    private PathFollow2D follow;
    private Timer dash_attack_timer;
    private AnimatedSprite2D sprite;

    private State state;
    private Vector2 target_position = Vector2.Zero;

    private float speed_mult = 1.0f;

    public int health = 1;

    private AudioStreamPlayer sfx;

    [Export]
    public AudioStream SfxIntro;

    [Export]
    public AudioStream SfxIdle;

    [Export]
    public AudioStream SfxStall;

    [Export]
    public AudioStream SfxAttack;

    [Export]
    public AudioStream SfxStuck;

    enum State {
        FollowPath,
        DramaticPause,
        Seek,
        Charge,
        Retreat,
    }

    private bool is_dead = false;
    private bool is_encounter_started = false;


    public override void _Ready() {
        follow = GetParent().GetNode<PathFollow2D>("BossPath/Follow");
        path = GetParent().GetNode<Path2D>("BossPath");

        sprite = GetNode<AnimatedSprite2D>("Sprite");
        sfx = GetNode<AudioStreamPlayer>("SFX");
        sfx.VolumeDb = -25.0f;

        BodyEntered += HandleCollision;

        dash_attack_timer = new Timer();
        dash_attack_timer.OneShot = true;
        dash_attack_timer.WaitTime = DashCooldown;
        dash_attack_timer.Timeout += AttackSequence;
        AddChild(dash_attack_timer);

        state = State.DramaticPause;
        target_position = Vector2.Zero;
        speed_mult = 1.0f;

        sprite.Animation = "idle";
        sprite.Play();

        Healthbar.MaxValue = health;
        Healthbar.Value = health;
    }

    public void PlayIntroSfx() {
        sfx.Autoplay = false;
        sfx.Stream = SfxIntro;
        sfx.Finished += LoopIdle;
        sfx.Play();
    }

    public void StartEncounter() {
        state = State.DramaticPause;

        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            state = State.FollowPath;
            dash_attack_timer.Start();
        };

        is_encounter_started = true;
    }

    private void LoopIdle() {
        sfx.Stream = SfxIdle;
        sfx.Play();
    }

    private void HandleCollision(Node2D other) {
        var explodingRock = other as ExplodingRock;
        if (explodingRock != null) {
            // HACK: can't destroy shit during event handlers or sth; defer the call to idle time
            explodingRock.CallDeferred(new StringName("Explode"));

            Blink(10, 0.1f);
            Healthbar.Value = --health;

            state = State.DramaticPause;

            sprite.Animation = "hurt";

            sfx.Stream = SfxStuck;
            sfx.Play();

            GetTree().CreateTimer(4.0f + 1.5f, false).Timeout += () => {
                if (health == 0) {
                    state = State.DramaticPause;
                    sprite.Animation = "dead";

                    sfx.Finished -= LoopIdle;
                    sfx.Stream = SfxStall;
                    sfx.Play();

                    is_dead = true;

                    var main = GetTree().Root.GetNode<Main>("Main");
                    main.CallDeferred(nameof(Main.Win));
                } else {
                    state = State.Retreat;
                    sprite.Animation = "idle";
                }
            };
        }
    }

    private void Blink(int blinks, float interval = 0.5f) {
        if (blinks == 0) {
            return;
        }

        var original_modulate = Modulate;
        GetTree().CreateTimer(interval / 2.0f, false).Timeout += () => {
            Modulate = new Color(1.0f, 0.0f, 0.0f);

            GetTree().CreateTimer(interval / 2.0f, false).Timeout += () => {
                Modulate = original_modulate;

                Blink(--blinks, interval);
            };
        };
    }

    private void AttackSequence() {
        state = State.DramaticPause;

        // Dramatic pause
        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            state = State.Seek;
            sprite.Animation = "seek";
            sprite.Stop();

            // Initiate charge
            GetTree().CreateTimer(1.5f, false).Timeout += () => {
                sprite.Animation = "charge";
                sprite.Play();

                sfx.Stream = SfxAttack;
                sfx.Play();

                state = State.Charge;
                speed_mult = 1.0f;

                GetTree().CreateTimer(0.75f, false).Timeout += () => {
                    speed_mult = 3.0f;
                };
            };
        };
    }

    public override void _Process(double delta) {
        if (!is_encounter_started) {
            return;
        }

        var player = GetTree().Root.GetNode<Node2D>("Main/player");
        target_position = player.Position;

        if (is_dead) {
            sfx.VolumeDb = Mathf.Lerp(sfx.VolumeDb, -60.0f, 0.01f);
        }

        switch (state) {
            case State.FollowPath:
                follow.Progress += Speed * (float)delta;
                Position = Position.Lerp(follow.Position, 0.1f);
                break;
            case State.Seek:
                Position = Position.Lerp(new Vector2(Position.x, target_position.y), 0.01f);
                break;
            case State.Charge:
                if (Position.x > target_position.x - 32.0f) {
                    Position += Vector2.Left * (DashSpeed * speed_mult) * (float)delta;
                } else {
                    state = State.DramaticPause;
                    sprite.Animation = "idle";
                    sprite.Stop();

                    // Start retreat
                    GetTree().CreateTimer(0.5f, false).Timeout += () => {
                        state = State.Retreat;

                        sprite.Animation = "seek";
                        sprite.Play();

                        sfx.Stream = SfxStall;
                        sfx.Play();
                    };
                }
                break;
            case State.Retreat:
                if (Position.x < follow.Position.x) {
                    Position += Vector2.Right * (DashSpeed * 0.75f) * (float)delta;
                } else {
                    // Reset
                    dash_attack_timer.Start();
                    target_position = Vector2.Zero;
                    state = State.FollowPath;
                    speed_mult = 1.0f;

                    sprite.Animation = "idle";
                    sprite.Play();

                    sfx.Stream = SfxIdle;
                    sfx.Play();
                }
                break;
            case State.DramaticPause:
            // TODO: VROOOM!! VROOM!!
            default:
                break;
        }
    }
}
