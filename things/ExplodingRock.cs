using Godot;
using Godot.Collections;

public partial class ExplodingRock : StaticBody2D {
    [Export]
    public Array<PackedScene> SpawnTemplates = new Array<PackedScene>();

    [Export]
    public float ProjectileSpeedMin = 100.0f;
    [Export]
    public float ProjectileSpeedMax = 150.0f;

    [Export]
    public float ExplodeDuration = 4.0f;

    [Export]
    public float ProjectileLifetime = 1.25f;

    [Export]
    public int BurstCount = 16;

    [Export]
    public int SpawnCount = 4;

    private RandomNumberGenerator rng = new RandomNumberGenerator();

    private GPUParticles2D poof;

    private bool is_exploding = false;

    public override void _Ready() {
        poof = GetNode<GPUParticles2D>("Poof");
    }

    public void Explode() {
        if (is_exploding) {
            return;
        }

        is_exploding = true;

        var emit_interval = ExplodeDuration / SpawnCount;

        poof.Emitting = true;
        SpawnShrapnel();
        for (int i = 1; i < BurstCount; i++) {
            GetTree().CreateTimer(i * emit_interval, false).Timeout += () => {
                poof.Emitting = true;
                SpawnShrapnel();
            };
        }

        GetTree().CreateTimer(ExplodeDuration, false).Timeout += () => {
            poof.Emitting = true;

            GetTree().CreateTimer(poof.Lifetime + 0.1f, false).Timeout += () => QueueFree();
        };

        GetTree().CreateTimer(ExplodeDuration + poof.Lifetime + 0.1f, false).Timeout += () => QueueFree();
    }

    private void SpawnShrapnel() {
        if (SpawnTemplates.Count == 0) {
            GD.PushError("No spawn templates defined! Skipping `SpawnShrapnel`!");
            return;
        }

		var angle_increment = 360.0f / SpawnCount;
		var angle_offset = rng.Randf() * angle_increment;
        for (int i = 0; i < SpawnCount; i++) {
            var template = SpawnTemplates[(int)rng.Randi() % SpawnTemplates.Count];

            var shrapnel = template.Instantiate<Projectile>();

			var speed = rng.RandfRange(ProjectileSpeedMin, ProjectileSpeedMax);
			var angle = angle_offset + i * angle_increment;
			var velocity = Vector2.Right.Rotated(Mathf.DegToRad(angle)) * speed;
            shrapnel.Velocity = velocity;
            shrapnel.Lifetime = ProjectileLifetime;

            shrapnel.GlobalPosition = GlobalPosition;
            GetParent().AddChild(shrapnel);
        }
    }
}
