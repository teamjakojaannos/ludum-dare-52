using Godot;

public partial class Healthbar : Control {
    [Export]
    public Texture2D SliceTexture;

    [Export]
    public Vector2 SliceOffset;

    public int Health {
        set {
            if (value == 0) {
                Hide();
                return;
            }
            Show();
            if (value == 1) {
                GetChild<AnimatedSprite2D>(0).Play("danger");
            } else {
                GetChild<AnimatedSprite2D>(0).Play("idle");
            }

            while (GetChildCount() > 1) {
                RemoveChild(GetChild(1));
            }

            for (var i = 1; i < value; ++i) {
                var slice = new Sprite2D();
                slice.Texture = SliceTexture;
                slice.Hframes = 8;
                slice.Vframes = 1;
                slice.Frame = i == value - 1 ? 2 : 1;
                slice.Position = SliceOffset * i;
                slice.Centered = false;
                AddChild(slice);
            }
        }
    }
}
