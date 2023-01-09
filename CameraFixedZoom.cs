using Godot;
using System;

public partial class CameraFixedZoom : Camera2D {
    [Export]
    public int TileSize = 32;

    [Export]
    public int ScreenWidthInTiles = 16;

    [Export]
    public float ZoomFactor = 1.0f;

    [Export]
    public Vector2 IntroOffset = Vector2.Up * 8.0f;

    public bool OverridePlayerPosition = false;

    private Player player;

    public override void _Ready() {
        player = GetTree().Root.GetNode<Player>("Main/player");
    }

    public override void _Process(double delta) {
        var viewport_size = GetViewportRect().Size;
        var screen_width = (float)ScreenWidthInTiles * TileSize;
        var ratio = viewport_size.x / screen_width;

        Zoom = new Vector2(ratio * ZoomFactor, ratio * ZoomFactor);

        Position = OverridePlayerPosition
			? IntroOffset
			: player.Position + IntroOffset;
    }
}
