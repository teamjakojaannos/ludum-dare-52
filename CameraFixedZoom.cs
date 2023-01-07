using Godot;
using System;

public partial class CameraFixedZoom : Camera2D {
	[Export]
	public int TileSize = 32;

	[Export]
	public int ScreenWidthInTiles = 16;

	[Export]
	public float ZoomFactor = 1.0f;


    public override void _Process(double delta) {
		var viewport_size = GetViewportRect().Size;
		var screen_width = (float) ScreenWidthInTiles * TileSize;
		var ratio = viewport_size.x / screen_width;

		Zoom = new Vector2(ratio * ZoomFactor, ratio * ZoomFactor);
    }
}
