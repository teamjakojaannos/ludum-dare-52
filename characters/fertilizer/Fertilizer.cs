using Godot;
using System;

public partial class Fertilizer : RigidBody2D {

    [Export(PropertyHint.Range, "0.0,1.0")]
    public float friction = 0.5f;

    public override void _Ready() {
    }

    public override void _Process(double delta) {
        this.LinearVelocity = this.LinearVelocity.Lerp(Vector2.Zero, friction);
    }
}
