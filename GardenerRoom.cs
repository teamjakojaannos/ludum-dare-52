using Godot;

public partial class GardenerRoom : Room {
    public override void OnPlayerEnter() {
        base.OnPlayerEnter();

        var spawner = GetNode<SpawnerArea>("Bounds");
        spawner.Start();
    }

    public override void OnPlayerExit() {
        base.OnPlayerExit();

		var spawner = GetNode<SpawnerArea>("Bounds");
        spawner.Stop();
    }
}
