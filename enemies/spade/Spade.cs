using Godot;

public partial class Spade : Area2D {
	[Export]
	public float KnockbackForce = 100.0f;

    public override void _Ready() {
		var animation = GetNode<AnimationPlayer>("Animation");
		animation.AnimationFinished += (name) => {
			animation.Stop();
			QueueFree();
		};

		animation.Play("Danger");
    }

    public void DealDamage() {
		var overlapping = GetOverlappingBodies();
		Player player = null;
		foreach (var body in overlapping) {
			player = body as Player;
			if (player != null) break;
		}

		if (player == null) {
			return;
		}

		player.TakeDamage(1);
		player.Knockback(KnockbackForce, Position);
	}
}
