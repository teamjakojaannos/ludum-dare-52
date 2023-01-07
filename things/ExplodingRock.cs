using Godot;
using Godot.Collections;

public partial class ExplodingRock : Area2D {
    [Export]
    public Array<PackedScene> SpawnTemplates = new Array<PackedScene>();

	[Export]
	public float ProjectileSpeedMin = 100.0f;
	[Export]
	public float ProjectileSpeedMax = 150.0f;

	[Export]
	public float ProjectileLifetime = 1.25f;

    [Export]
    public int SpawnCount = 8;

    private RandomNumberGenerator rng = new RandomNumberGenerator();

	private GPUParticles2D poof;

    public override void _Ready() {
		poof = GetNode<GPUParticles2D>("Poof");
    }

    public void Explode() {
        for (int i = 0; i < SpawnCount; i++) {
            SpawnShrapnel();
        }
		
		poof.Emitting = true;
		GetTree().CreateTimer(poof.Lifetime, false).Timeout += () => QueueFree();
    }

    private void SpawnShrapnel() {
		if (SpawnTemplates.Count == 0) {
			GD.PushError("No spawn templates defined! Skipping `SpawnShrapnel`!");
			return;
		}

        var template = SpawnTemplates[(int)rng.Randi() % SpawnTemplates.Count];

		var shrapnel = template.Instantiate<Projectile>();
		shrapnel.Velocity = new Vector2(
			(rng.Randf() * 2.0f - 1.0f) * rng.RandfRange(ProjectileSpeedMin, ProjectileSpeedMax),
			(rng.Randf() * 2.0f - 1.0f) * rng.RandfRange(ProjectileSpeedMin, ProjectileSpeedMax)
		);
		shrapnel.Lifetime = ProjectileLifetime;

		shrapnel.GlobalPosition = GlobalPosition;
		GetParent().AddChild(shrapnel);
    }
}
