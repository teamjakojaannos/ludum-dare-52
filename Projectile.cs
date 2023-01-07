using Godot;

public partial class Projectile : Area2D {
    [Export]
    public Vector2 Velocity = Vector2.Zero;

	[Export]
    public float Lifetime = 3.0f;

	public bool ShouldPoof = true;

    public override void _Ready() {
        GetTree().CreateTimer(Lifetime, false).Timeout += () => {
			var delete_after = 0.0f;
			if (ShouldPoof) {
				var poof = GetNodeOrNull<GPUParticles2D>("Poof");
				if (poof != null) {
					poof.Emitting = true;
					delete_after = (float) poof.Lifetime;
				}
			}

			GetTree().CreateTimer(delete_after, false).Timeout += () => QueueFree();
		};
    }

    public override void _Process(double delta) {
		Position += Velocity * (float) delta;
    }
}
