using Godot;
using System;
using System.Collections.Generic;

public partial class Frog : StaticBody2D {

    [Signal]
    public delegate void FrogStartsSleepingEventHandler();

    enum State {
        ReadyToAttack,
        AttackOnCooldown,
        AttackCharging,
        Idle,
        FallingAsleep,
        Sleeping
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

    private bool just_attacked = false;

    [Export]
    public int flies_to_eat_before_sleep = 3;

    private int flies_eaten = 0;


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


        animation.AnimationFinished += on_animation_finished;
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
                check_if_sleepy();
                return;
            }
        }

    }

    private void attack() {
        destroy_nearest_fly();
        flies_eaten++;

        attack_window.Stop();
        cooldown.Start();
        animation.Animation = "attacking";
        just_attacked = true;

        this.state = State.AttackOnCooldown;
    }

    private void check_if_sleepy() {
        if (flies_eaten < flies_to_eat_before_sleep) {
            return;
        }

        // start sleeping

        // note: animation is set in "on_animation_finished", as the sequence is attack > chew > sleep
        state = State.FallingAsleep;

        EmitSignal(SignalName.FrogStartsSleeping);

        // stop all timers just in case
        cooldown.Stop();
        attack_charge.Stop();
        attack_window.Stop();
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
        animation.Animation = "prepared";
        state = State.AttackCharging;
        attack_charge.Start();
    }

    private void on_animation_finished() {
        if (just_attacked) {
            just_attacked = false;
            animation.Animation = "chewing";
            return;
        }

        if (state == State.FallingAsleep) {
            animation.Animation = "falling asleep";
            state = State.Sleeping;
            return;
        }

        if (state == State.Sleeping) {
            animation.Animation = "sleeping";
            return;
        }
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
        this.state = State.ReadyToAttack;
        animation.Animation = "prepared";
        attack_window.Start();
    }

    private void cooldown_done() {
        state = State.Idle;
        animation.Animation = "default";
    }

    private void missed_window_to_attack() {
        state = State.Idle;
        animation.Animation = "default";
    }
}
