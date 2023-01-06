using Godot;
using System;

public partial class CameraFixedZoom : Camera2D {
	[Export]
	public int TileSize = 32;

	public int ScreenWidthInTiles = 16;

    public override void _Process(double delta) {
		var viewport_size = GetViewportRect().Size;
		var desired_width = ScreenWidthInTiles * TileSize;
		var zoom = viewport_size.x / desired_width;

		this.Zoom = new Vector2(zoom, zoom);
    }
}
