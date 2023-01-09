using Godot;
using System;

public partial class Worm : Area2D {

    [Export]
    public Path2D path;

    private PathFollow2D follow;

    [Export]
    public float speed = 75.0f;

    private AnimatedSprite2D animation;

    /// worm walks for this amount, then pauses
    [Export]
    public Timer walk_timer;


    [Export]
    public Timer pause_timer;

    private bool paused = false;

    private int damage = 1;

    private Random random;

    [Export]
    public bool allow_timer_randomization = true;

    [Export]
    public bool allow_reverse_travel = true;

    [Export]
    public bool patrol_behaviour = false;


    private double chance_to_reverse = 0.3;

#pragma warning disable CS0162
    private const bool damage_enabled = true; // for testing, I dont like to restart the game a million times

    private bool reversing = false;
    private float base_walk_time;
    private float base_pause_time;

    public override void _Ready() {
        if (!damage_enabled) { GD.Print("NOTE: worm damage is off (for test purposes)"); }

        if (path == null) { GD.Print($"ERROR: Path is not set for {this.GetPath()}"); }
        this.follow = path.GetChild<PathFollow2D>(0);
        if (follow == null) { GD.Print($"ERROR: Path '{path.Name}' doesn't have a 'PathFollow2D' child in {this.GetPath()}"); }


        animation = GetNode<AnimatedSprite2D>("Animation");

        this.BodyEntered += on_collision;

        if (pause_timer == null) {
            // no custom timer given, use default
            pause_timer = GetNode<Timer>("PauseTimer");
        }
        pause_timer.Autostart = false;
        pause_timer.OneShot = true;
        pause_timer.Timeout += continue_journey;
        base_pause_time = (float)pause_timer.WaitTime;

        if (walk_timer == null) {
            // no custom timer given, use default
            walk_timer = GetNode<Timer>("WalkTimer");
        }
        walk_timer.Autostart = false;
        walk_timer.OneShot = true;
        walk_timer.Timeout += have_pause;
        base_walk_time = (float)walk_timer.WaitTime;

        this.random = new Random();

        if (allow_timer_randomization) {
            randomize_timer(pause_timer, base_pause_time);
            randomize_timer(walk_timer, base_walk_time);
        }

        if (patrol_behaviour) {
            allow_reverse_travel = false;
        } else {
            walk_timer.Start();
        }
    }

    public override void _Process(double delta) {
        if (!paused) {
            var direction = reversing ? -1.0 : 1.0;
            var progress = delta * speed * direction;

            var old_ratio = follow.ProgressRatio;
            follow.Progress += (float)progress;



            if (patrol_behaviour) {
                var new_ratio = follow.ProgressRatio;
                var looped_around = !reversing
                ? new_ratio < old_ratio // went from 100 -> 0
                : new_ratio > old_ratio; // went from 0 -> 100

                if (looped_around) {
                    reversing = !reversing;
                    have_pause();
                    // reset back to before O looped around
                    follow.ProgressRatio = old_ratio;
                }

            }
        }

        var old_pos = this.Position;
        var new_pos = follow.Position;
        this.Position = new_pos;

        update_sprite(old_pos, new_pos);
    }

    private void update_sprite(Vector2 old_pos, Vector2 new_pos) {
        var x_diff = old_pos.x - new_pos.x;
        var y_diff = old_pos.y - new_pos.y;
        if (Math.Abs(y_diff) > Math.Abs(x_diff)) {
            this.animation.Animation = y_diff > 0
                ? "up"
                : "down";
        } else {
            this.animation.Animation = "side";
            this.animation.FlipH = old_pos.x > new_pos.x;
        }
    }

    private void continue_journey() {
        if (allow_timer_randomization) {
            randomize_timer(walk_timer, base_walk_time);
        }

        if (allow_reverse_travel) {
            var change_direction = random.NextDouble() < chance_to_reverse;
            if (change_direction) {
                this.reversing = true;
            }
        }

        paused = false;
        if (!patrol_behaviour) {
            walk_timer.Start();
        }
    }

    private void have_pause() {
        if (allow_timer_randomization) {
            randomize_timer(pause_timer, base_pause_time);
        }

        paused = true;
        pause_timer.Start();
    }

    public void on_collision(Node2D other) {
        var player = other as Player;
        if (player == null) {
            return;
        }

        if (damage_enabled) {
            player.TakeDamage(damage);
            player.Knockback(150f, Position);
        } else {
            GD.Print("Worm hit player");
        }
    }

    private float random_float(float min, float max) {
        return (float)(random.NextDouble() * (max - min) + min);
    }

    private void randomize_timer(Timer to_be_randomized, float base_wait_time) {
        var mult = random_float(0.75f, 2.5f);
        var wait_time = base_wait_time * mult;
        to_be_randomized.WaitTime = wait_time;
    }
}
