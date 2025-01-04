using Godot;

public partial class FreeLookCamera3D : Camera3D
{
	[Export] public float MoveSpeed { get; set; } = 10.0f; // Base movement speed
	[Export] public float SprintMultiplier { get; set; } = 2.0f; // Speed multiplier when sprinting
	[Export] public float MouseSensitivity { get; set; } = 0.2f; // Mouse look sensitivity
	[Export] public bool InvertY { get; set; } = false; // Invert mouse Y-axis

	private Vector3 _velocity = Vector3.Zero;
	private Vector2 _rotation = Vector2.Zero;

	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured; // Capture mouse for camera control
	}

	public override void _Process(double delta)
	{
		ProcessInput(delta);
		ProcessMovement(delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			ProcessMouseMotion(mouseMotionEvent);
		}
	}

	private void ProcessInput(double delta)
	{
		// Reset velocity
		_velocity = Vector3.Zero;

		// Movement input
		Vector3 inputDir = Vector3.Zero;
		if (Input.IsActionPressed("move_forward"))
			inputDir.Z += 1;
		if (Input.IsActionPressed("move_backward"))
			inputDir.Z -= 1;
		if (Input.IsActionPressed("move_left"))
			inputDir.X -= 1;
		if (Input.IsActionPressed("move_right"))
			inputDir.X += 1;
		if (Input.IsActionPressed("move_up"))
			inputDir.Y += 1;
		if (Input.IsActionPressed("move_down"))
			inputDir.Y -= 1;

		// Normalize input direction to avoid faster diagonal movement
		inputDir = inputDir.Normalized();

		// Apply speed and sprint multiplier
		float speed = MoveSpeed;
		if (Input.IsActionPressed("sprint"))
			speed *= SprintMultiplier;

		// Apply rotation to movement direction
		Vector3 forward = -Transform.Basis.Z;
		Vector3 right = Transform.Basis.X;
		Vector3 up = Transform.Basis.Y;
		_velocity += (forward * inputDir.Z + right * inputDir.X + up * inputDir.Y) * speed;
	}

	private void ProcessMovement(double delta)
	{
		// Move camera based on velocity
		GlobalTranslate(_velocity * (float)delta);
	}

	private void ProcessMouseMotion(InputEventMouseMotion mouseMotionEvent)
	{
		// Adjust rotation based on mouse motion
		_rotation.X -= mouseMotionEvent.Relative.Y * MouseSensitivity * (InvertY ? -1 : 1);
		_rotation.Y -= mouseMotionEvent.Relative.X * MouseSensitivity;

		// Clamp pitch to avoid flipping
		_rotation.X = Mathf.Clamp(_rotation.X, -90, 90);

		// Apply rotation to camera
		RotationDegrees = new Vector3(_rotation.X, _rotation.Y, 0);
	}
}
