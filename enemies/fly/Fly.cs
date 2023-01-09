using Godot;
using System;
using System.Collections.Generic;

public partial class Fly : Area2D {

	[Export]
	public PackedScene projectile_template;

	[Export]
	public float projectile_speed = 185.0f;

	[Export]
	public float projectile_lifetime = 0.75f;

	[Export]
	public Timer projectile_cooldown;

	private bool ready_to_shoot = true;

	private bool charging_shot = false;

	/// how long the fly will charge a shot before firing
	[Export]
	public Timer charge_timer;

	/// the fly will try to go this close to the current target
	[Export]
	public float target_distance = 5.0f;

	[Export]
	public float speed = 150.0f;

	[Export]
	public float path_speed_modifier = 0.75f;

	private Node2D chased_entity;
	private Vector2 target_position;

	private Path2D path;
	private PathFollow2D follow;

	private double passed_time = 0.0;

	// how often scale and rotation of flight path are rerolled
	[Export]
	public double randomize_time = 2.5f;

	private Random random;

	private AnimatedSprite2D animation;

	private List<Node2D> visible_items = new();

	// (left-top) (right-bottom)
	public (Vector2, Vector2) room_size;

	[Export]
	public int melee_damage = 1;

	private Node2D player;

	private bool attack_animation_playing = false;

	private Vector2 attack_animation_offset = new Vector2(9.0f, 0.0f);

	public override void _Ready() {
		room_size = get_room_size_or_screen_size();


		follow = GetNode<PathFollow2D>("Path/Follow");
		path = GetNode<Path2D>("Path");
		animation = GetNode<AnimatedSprite2D>("Animation");

		animation.Playing = true;

		this.BodyEntered += body_collided;

		// make sure eyes and nose have different masks, otherwise leaving sight makes the item invisible for smell radius
		var eyes = GetNode<Area2D>("SightArea");
		eyes.BodyEntered += body_entered_sight;
		eyes.BodyExited += body_exited_sight;

		var nose = GetNode<Area2D>("SmellRadius");
		nose.BodyEntered += body_entered_sight;
		nose.BodyExited += body_exited_sight;

		projectile_cooldown.Timeout += projectile_ready;

		charge_timer.Timeout += () => {
			if (player != null) {
				shoot_projectile(player.Position);
				charging_shot = false;
			}
		};

		this.animation.AnimationFinished += () => {
			if (attack_animation_playing) {
				attack_animation_playing = false;
				this.animation.Animation = "chase player";
				this.animation.Offset = Vector2.Zero;
			}
		};

		this.target_position = this.Position;
		this.player = null;

		random = new Random();

		randomize_fields();
	}

	private void randomize_fields() {
		var speed_multiplier = random_float(1.0f, 1.75f);
		this.speed *= speed_multiplier;
		this.animation.SpeedScale = speed_multiplier;


		var projectile_speed_mult = random_float(0.90f, 1.1f);
		this.projectile_speed *= projectile_speed_mult;

		var attack_cd_mult = random_float(0.5f, 1.0f);
		this.projectile_cooldown.WaitTime *= attack_cd_mult;

		var charge_time_mult = random_float(0.5f, 1.0f);
		this.charge_timer.WaitTime = 1.0f;
	}

	private float random_float(float min, float max) {
		return (float)(random.NextDouble() * (max - min) + min);
	}

	private (Vector2, Vector2) get_room_size_or_screen_size() {
		var parent = GetParent();

		if (parent.GetType().IsAssignableTo(typeof(Room))) {
			var room = (Room)parent;
			var size = (
				new Vector2(room.CameraLimitLeft, room.CameraLimitTop),
				new Vector2(room.CameraLimitRight, room.CameraLimitBottom)
			);

			return size;
		} else {
			return (Vector2.Zero, GetViewportRect().Size);
		}
	}

	public override void _Process(double delta) {
		if (GetTree().Root.GetNode<Main>("Main").DialogueUI.Visible) {
			return;
		}
		passed_time += delta;

		if (passed_time >= randomize_time) {
			passed_time = 0;
			var new_scale = random.NextDouble() + 0.5;
			var new_rotation = (random.NextDouble() - 0.5) * Math.PI;
			rotate_and_scale_path((float)new_scale, (float)new_rotation);
		}

		if (charging_shot) {
			rotate_towards_player();
			return;
		}

		if (ready_to_shoot) {
			var started_charge = charge_shot_if_player_visible();
			if (started_charge) {
				return;
			}
		}

		this.target_position = get_target();


		var old_position = this.Position;

		move((float)delta);

		var new_position = this.Position;

		flip_animation(old_position, new_position);
	}

	/// caught our target? great, find new one. Target out of sight? find new one. otherwise pursue old target
	private Vector2 get_target() {
		if (chased_entity != null) {
			// I'm chasing something
			if (visible_items.Contains(chased_entity)) {
				// I can still see it
				var dist2_to_chased = this.Position.DistanceSquaredTo(chased_entity.Position);
				if (dist2_to_chased >= target_distance) {
					// and I haven't caught them yet
					return chased_entity.Position;
				}
			}

			// I've lost or caught the target
			this.chased_entity = null;

		}

		var dist2_to_target = this.Position.DistanceSquaredTo(this.target_position);
		if (dist2_to_target > target_distance) {
			// I haven't reached my target, keep chasing
			return this.target_position;
		}

		// so, I now have reached my target. Find a new one
		var new_target = find_target();
		if (new_target != null) {
			this.chased_entity = new_target;
			return chased_entity.Position;
		}

		return random_position_around_me();
	}

	/// if player is nearby, start chasing them, otherwise find crap
	private Node2D find_target() {
		if (visible_items.Count == 0) {
			return null;
		}

		var player = visible_items.Find(node => node.GetType() == typeof(Player));
		if (player != null) {
			this.animation.Animation = "chase player";
			return player;
		}

		var dungs = visible_items.FindAll(node => node.GetType() == typeof(Fertilizer));
		if (dungs.Count == 0) {
			return null;
		}

		this.animation.Animation = "default";
		if (dungs.Count == 1) {
			return dungs[0];
		}

		var nearest = dungs[0];
		var nearest_dist = nearest.Position.DistanceSquaredTo(this.Position);

		foreach (var dung in dungs) {
			var dist = dung.Position.DistanceSquaredTo(this.Position);
			if (dist < nearest_dist) {
				nearest = dung;
			}
		}

		return nearest;
	}


	/// if the fly doesn't find a target, it will pick a random point inside {random_position_radius} circle
	private float random_position_radius = 300.0f;
	private float random_position_radius_min = 50.0f;
	private Vector2 random_position_around_me() {
		var length = random.NextDouble() * random_position_radius + random_position_radius_min;
		var rand = new Vector2((float)length, 0.0f);
		var angle = random.NextDouble() * 2.0 * Math.PI;

		rand = rand.Rotated((float)angle);

		var pos = this.Position + rand;
		pos = pos.Clamp(room_size.Item1, room_size.Item2);

		return pos;
	}

	private void move(float delta) {
		follow.Progress += speed * (delta * path_speed_modifier);

		var position_on_path = follow.GlobalPosition - this.GlobalPosition;

		var targe_position = target_position + position_on_path;
		var velocity = (targe_position - Position).Normalized() * speed;
		var new_position = Position + velocity * delta;

		Position = Position.Lerp(new_position, 0.5f)
			.Clamp(room_size.Item1, room_size.Item2);
	}

	public bool charge_shot_if_player_visible() {
		var player = visible_items.Find(node => node.GetType() == typeof(Player));
		if (player == null) {
			return false;
		}


		charge_timer.Start();
		charging_shot = true;
		this.player = player;

		animation.Animation = "attacking";
		animation.Offset = attack_animation_offset;
		attack_animation_playing = true;

		return true;
	}

	private void shoot_projectile(Vector2 shoot_target) {
		if (GetTree().Root.GetNode<Main>("Main").DialogueUI.Visible) {
			projectile_cooldown.Start();
			ready_to_shoot = false;
			return;
		}

		var projectile = projectile_template.Instantiate<FlyProjectile>();

		var speed = projectile_speed;

		var velocity = (shoot_target - this.Position).Normalized() * projectile_speed;

		projectile.Velocity = velocity;
		projectile.Lifetime = projectile_lifetime;

		projectile.GlobalPosition = GlobalPosition;
		GetParent().AddChild(projectile);

		projectile_cooldown.Start();
		ready_to_shoot = false;
	}

	private void projectile_ready() {
		this.ready_to_shoot = true;
	}

	private void rotate_towards_player() {
		if (player == null) {
			return;
		}

		this.animation.FlipH = player.Position.x < this.Position.x;
	}

	private void flip_animation(Vector2 old_position, Vector2 new_position) {
		this.animation.FlipH = old_position.x > new_position.x;
	}

	private void body_entered_sight(Node2D body) {
		this.visible_items.Add(body);
	}

	private void body_exited_sight(Node2D body) {
		this.visible_items.Remove(body);
	}

	private bool melee_damage_enabled = false;
	private void body_collided(Node2D other) {
		if (!melee_damage_enabled) {
			return;
		}

		var player = other as Player;
		if (player == null) {
			return;
		}

		player.TakeDamage(melee_damage);
	}

	private void rotate_and_scale_path(float scale, float rotation) {
		this.path.Scale = new Vector2(scale, scale);
		this.path.Rotation = rotation;
	}
}
