using Godot;
using Godot.Collections;

public partial class SpawnerArea : Area2D {
    [Export]
    public Array<PackedScene> Templates = new Array<PackedScene>();

    [Export]
    public int SpawnCount = 1;

    private Timer timer;

    private RandomNumberGenerator rng = new RandomNumberGenerator();


    public override void _Ready() {
        var shape = GetNodeOrNull<CollisionShape2D>("Shape");
        var area_size = (shape?.Shape as RectangleShape2D)?.Size ?? new Vector2(16 * 32, 9 * 32);
		var half_w = area_size.x / 2.0f;
		var half_h = area_size.y / 2.0f;

        timer = GetNode<Timer>("SpawnTimer");
        timer.Timeout += () => {
            for (var i = 0; i < SpawnCount; ++i) {
                var instance = Templates[(int)(rng.Randi() % Templates.Count)]?.Instantiate<Spade>();
				instance.Position = new Vector2(
					shape.Position.x + rng.RandfRange(-half_w + 32.0f, half_w - 32.0f),
					shape.Position.y + rng.RandfRange(-half_h + 3 * 32.0f, half_h - 32.0f)
				);

				GetParent().AddChild(instance);
            }
        };
    }

    public void Start() {
        timer.Start();
    }

    public void Stop() {
        timer.Stop();
    }
}
