using Godot;
using System;

public partial class Frog : StaticBody2D {


    public override void _Ready() {
    }

    public override void _Process(double delta) {
    }

	public void on_body_entered(Node2D body) {
		GD.Print("Body entered");
	}

	public void on_area_entered(Area2D area) {
		GD.Print("Area entered");
	}
}
