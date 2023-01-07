using Godot;
using System;
using System.Collections.Generic;

public partial class Fly : Area2D {

    /// the fly will try to go this close to the current target
    [Export]
    public float target_distance = 5.0f;

    [Export]
    public float speed = 150.0f;

    [Export]
    public float path_speed_modifier = 0.75f;

    private Node2D chased_entity;
    private Vector2 target_position;

    private Path2D path;
    private PathFollow2D follow;

    private double passed_time = 0.0;

    // how often scale and rotation of flight path are rerolled
    [Export]
    public double randomize_time = 2.5f;

    private Random random;

    private AnimatedSprite2D animation;

    private List<Node2D> visible_items = new();

    public Vector2 screen_size;



    public override void _Ready() {
        screen_size = GetViewportRect().Size;


        follow = GetNode<PathFollow2D>("Path/Follow");
        path = GetNode<Path2D>("Path");
        animation = GetNode<AnimatedSprite2D>("Animation");

        animation.Playing = true;

        var eyes = GetNode<Area2D>("SightArea");
        eyes.BodyEntered += on_body_entered;
        eyes.BodyExited += on_body_exited;

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

        this.target_position = get_target();


        var old_position = this.Position;

        move((float)delta);

        var new_position = this.Position;

        flip_animation(old_position, new_position);
    }

    /// caught our target? great, find new one. Target out of sight? find new one. otherwise pursue old target
    private Vector2 get_target() {
        if (chased_entity != null) {
            // I'm chasing something
            if (visible_items.Contains(chased_entity)) {
                // I can still see it
                var dist2_to_chased = this.Position.DistanceSquaredTo(chased_entity.Position);
                if (dist2_to_chased >= target_distance) {
                    // and I haven't caught them yet
                    return chased_entity.Position;
                }
            }

            // I've lost or caught the target
            this.chased_entity = null;

        }

        var dist2_to_target = this.Position.DistanceSquaredTo(this.target_position);
        if (dist2_to_target > target_distance) {
            // I haven't reached my target, keep chasing
            return this.target_position;
        }

        // so, I now have reached my target. Find a new one
        var new_target = find_target();
        if (new_target != null) {
            this.chased_entity = new_target;
            return chased_entity.Position;
        }

        return random_position_around_me();
    }

    /// if player is nearby, start chasing them, otherwise find crap
    private Node2D find_target() {
        if (visible_items.Count == 0) {
            return null;
        }

        var player = visible_items.Find(node => node.GetType() == typeof(Player));
        if (player != null) {
            return player;
        }


        // lol we hungry just find first one
        var dung = visible_items.Find(node => node.GetType() == typeof(Fertilizer));
        // doesn't matter if it's null
        return dung;
    }


    /// if the fly doesn't find a target, it will pick a random point inside {random_position_radius} circle
    private float random_position_radius = 300.0f;
    private float random_position_radius_min = 50.0f;
    private Vector2 random_position_around_me() {
        var length = random.NextDouble() * random_position_radius + random_position_radius_min;
        var rand = new Vector2((float)length, 0.0f);
        var angle = random.NextDouble() * 2.0 * Math.PI;

        rand = rand.Rotated((float)angle);

        var pos = this.Position + rand;
        pos = pos.Clamp(Vector2.Zero, screen_size);

        return pos;
    }

    private void move(float delta) {
        follow.Progress += speed * (delta * path_speed_modifier);

        var position_on_path = follow.GlobalPosition - this.GlobalPosition;

        var targe_position = target_position + position_on_path;
        var velocity = (targe_position - Position).Normalized() * speed;
        var new_position = Position + velocity * delta;

        Position = Position.Lerp(new_position, 0.5f);
    }

    private void flip_animation(Vector2 old_position, Vector2 new_position) {
        this.animation.FlipH = old_position.x > new_position.x;
    }

    private void on_body_entered(Node2D body) {
        this.visible_items.Add(body);
    }

    private void on_body_exited(Node2D body) {
        this.visible_items.Remove(body);
    }

    private void rotate_and_scale_path(float scale, float rotation) {
        this.path.Scale = new Vector2(scale, scale);
        this.path.Rotation = rotation;
    }
}
