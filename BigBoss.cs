using Godot;

public partial class BigBoss : Area2D {
    [Export]
    public float Speed = 1.0f;

    [Export]
    public float DashSpeed = 150.0f;

    [Export]
    public float DashCooldown = 5.0f;

    private Path2D path;
    private PathFollow2D follow;
    private Timer dash_attack_timer;

    private State state;
    private Vector2 target_position = Vector2.Zero;

    private float speed_mult = 1.0f;

    enum State {
        FollowPath,
        DramaticPause,
        Seek,
        Charge,
        Retreat,
    }


    public override void _Ready() {
        follow = GetParent().GetNode<PathFollow2D>("BossPath/Follow");
        path = GetParent().GetNode<Path2D>("BossPath");

        dash_attack_timer = new Timer();
        dash_attack_timer.OneShot = true;
        dash_attack_timer.WaitTime = DashCooldown;
        dash_attack_timer.Timeout += AttackSequence;
        AddChild(dash_attack_timer);

        dash_attack_timer.Start();
        state = State.FollowPath;
        target_position = Vector2.Zero;
        speed_mult = 1.0f;
    }

    private void AttackSequence() {
        state = State.DramaticPause;

        // Dramatic pause
        GetTree().CreateTimer(0.5f, false).Timeout += () => {
            state = State.Seek;

            // Initiate charge
            GetTree().CreateTimer(1.5f, false).Timeout += () => {
                state = State.Charge;
                speed_mult = 1.0f;

                GetTree().CreateTimer(0.75f, false).Timeout += () => {
                    speed_mult = 3.0f;
                };
            };
        };
    }

    public override void _Process(double delta) {
        var player = GetTree().Root.GetNode<Node2D>("Main/player");
        target_position = player.GlobalPosition - GlobalPosition;

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

                    // Start retreat
                    GetTree().CreateTimer(0.5f, false).Timeout += () => {
                        state = State.Retreat;
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
                }
                break;
            case State.DramaticPause:
            // TODO: VROOOM!! VROOM!!
            default:
                break;
        }
    }
}
