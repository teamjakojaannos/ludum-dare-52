using Godot;
using System;

public partial class Fly : RigidBody2D {

    [Export]
    public float speed = 150.0f;

    [Export]
    public float path_speed_modifier = 0.75f;

    private Node2D target;

    private Path2D path;
    private PathFollow2D follow;

    private double passed_time = 0.0;

    // how often scale and rotation of flight path are rerolled
    [Export]
    public double randomize_time = 2.5f;

    private Random random;

    private AnimatedSprite2D animation;

    public override void _Ready() {
        follow = GetNode<PathFollow2D>("Path/Follow");
        target = GetNode<Node2D>("Target");
        path = GetNode<Path2D>("Path");
        animation = GetNode<AnimatedSprite2D>("Animation");

        animation.Playing = true;

        random = new Random();
    }

    public override void _Process(double delta) {
        passed_time += delta;

        if (passed_time >= randomize_time) {
            passed_time = 0;
            var new_scale = random.NextDouble() + 0.5;
            var new_rotation = (random.NextDouble() - 0.5) * Math.PI;
            rotate_and_scale_path((float)new_scale, (float)new_rotation);
        }

        locate_player();


        var old_position = this.Position;

        move((float)delta);

        var new_position = this.Position;

        flip_animation(old_position, new_position);
    }

    private void move(float delta) {
        follow.Progress += speed * (delta * path_speed_modifier);


        var position_on_path = follow.GlobalPosition - this.GlobalPosition;

        var targe_position = target.Position + position_on_path;
        var velocity = (targe_position - Position).Normalized() * speed;
        var new_position = Position + velocity * delta;

        Position = Position.Lerp(new_position, 0.5f);
    }

    private void flip_animation(Vector2 old_position, Vector2 new_position) {
        this.animation.FlipH = old_position.x < new_position.x;
    }

    private void locate_player() {
        if (!GetTree().Root.HasNode("Main/player")) {
            return;
        }

        var player = GetTree().Root.GetNode<Node2D>("Main/player");
        if (player == null) {
            return;
        }

        this.target.Position = player.Position;
    }

    private void rotate_and_scale_path(float scale, float rotation) {
        this.path.Scale = new Vector2(scale, scale);
        this.path.Rotation = rotation;
    }
}
