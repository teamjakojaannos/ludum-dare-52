using Godot;

public partial class Wat : Sprite2D {
    [Export]
    public float JiggleAmount = 4.0f;

    [Export]
    public float JiggleSpeed = 4.0f;

    private float x = 0.0f;

    public override void _Process(double delta) {
        x += (float)delta * JiggleSpeed;
        Offset = Vector2.Up * (Mathf.Sin(x) * (JiggleAmount / 2.0f));
    }
}
