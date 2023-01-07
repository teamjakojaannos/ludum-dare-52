using Godot;
using System;
using System.Collections.Generic;

public partial class Frog : StaticBody2D {

	enum State {
		ReadyToAttack,
		AttackOnCooldown,
		AttackCharging,
		Idle
	};

	private State state = State.Idle;

	// targets within frog's attack radius
	private List<Fly> tracking_targets = new();


	// frog sees a fly -> he starts winding up attack
	private Timer attack_charge;

	// frog has caught a fly -> he's chewing it and can't attack for another few seconds
	private Timer cooldown;

	// after winding up an attack, there is a small window of time to attack
	private Timer attack_window;

	private AnimatedSprite2D animation;


	public override void _Ready() {
		animation = GetNode<AnimatedSprite2D>("Animation");
		animation.Animation = "default";
		animation.Playing = true;

		var attack_area = GetNode<Area2D>("AttackArea");
		attack_area.AreaEntered += on_area_entered;
		attack_area.AreaExited += on_area_exited;

		attack_charge = GetNode<Timer>("Charge");
		attack_charge.Timeout += attack_charged;

		cooldown = GetNode<Timer>("Cooldown");
		cooldown.Timeout += cooldown_done;

		attack_window = GetNode<Timer>("AttackWindow");
		attack_window.Timeout += missed_window_to_attack;
	}

	public override void _Process(double delta) {
		var enemies_nearby = this.tracking_targets.Count > 0;

		if (this.state == State.Idle) {
			if (enemies_nearby) {
				start_attack_charge();
				return;
			}
		}

		if (this.state == State.ReadyToAttack) {
			if (enemies_nearby) {
				attack();
				return;
			}
		}

	}

	private void attack() {
		GD.Print("Slap");
		destroy_nearest_fly();
		// fly.destroy()
		cooldown.Start();
		animation.Animation = "chewing";
		this.state = State.AttackOnCooldown;
	}

	private void destroy_nearest_fly() {
		if (tracking_targets.Count == 0) {
			// how did we even get here?
			return;
		}

		var target = tracking_targets[0];

		if (tracking_targets.Count > 1) {
			var shortest_distance = dist2_to_me(target);

			foreach (var fly in tracking_targets) {
				var dist = dist2_to_me(fly);
				if (dist < shortest_distance) {
					shortest_distance = dist;
					target = fly;
				}
			}
		}

		tracking_targets.Remove(target);
		target.QueueFree();

	}

	private float dist2_to_me(Fly other) {
		return this.Position.DistanceSquaredTo(other.Position);
	}

	private void start_attack_charge() {
		GD.Print("Charging attack");
		animation.Animation = "prepared";
		state = State.AttackCharging;
		attack_charge.Start();
	}

	public void on_area_entered(Area2D area) {
		if (area.GetType() != typeof(Fly)) {
			return;
		}

		var fly = (Fly)area;
		tracking_targets.Add(fly);
	}

	public void on_area_exited(Area2D area) {
		if (area.GetType() != typeof(Fly)) {
			return;
		}

		var fly = (Fly)area;
		tracking_targets.Remove(fly);
	}

	private void attack_charged() {
		GD.Print("Ready to attack");

		this.state = State.ReadyToAttack;
		animation.Animation = "prepared";
		attack_window.Start();
	}

	private void cooldown_done() {
		GD.Print("Cooldown done");

		state = State.Idle;
		animation.Animation = "default";
	}

	private void missed_window_to_attack() {
		GD.Print("Missed attack window");

		state = State.Idle;
		animation.Animation = "default";
	}
}
