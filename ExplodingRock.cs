using Godot;
using Godot.Collections;

public partial class ExplodingRock : StaticBody2D {
    [Export]
    public Array<PackedScene> SpawnTemplates;

    [Export]
    public int SpawnCount;

    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public void Explode() {
        for (int i = 0; i < SpawnCount; i++) {
            SpawnShrapnel();
        }
    }

    private void SpawnShrapnel() {
        var template = SpawnTemplates[(int)rng.Randi() % SpawnTemplates.Count];

		var shrapnel = template.Instantiate<Node2D>();
		GetParent().AddChild(shrapnel);
    }
}
