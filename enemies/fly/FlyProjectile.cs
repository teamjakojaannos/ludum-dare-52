using Godot;
using System;

public partial class FlyProjectile : Area2D {

    [Export]
    public Vector2 Velocity = Vector2.Zero;

    [Export]
    public float Lifetime = 1.0f;

    public override void _Ready() {
        GetTree().CreateTimer(Lifetime, false).Timeout += () => QueueFree();
        this.BodyEntered += on_collision;
    }

    public override void _Process(double delta) {
        Position += Velocity * (float)delta;
    }

    public void on_collision(Node other) {
        var player = other as Player;
        if (player != null) {
            // TODO:
            // player.take_damage();
        }

        QueueFree();
    }
}
