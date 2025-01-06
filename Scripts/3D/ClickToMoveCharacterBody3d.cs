using Godot;
using System.Threading.Tasks;

public partial class ClickToMoveCharacterBody3d : CharacterBody3D
{

	[Export]
	public Camera3D camera;

	[Export] public float reactionTime = 1;
	[Export] public float decelerationTime = 0.5f; // Faster than reactionTime


	private NavigationAgent3D _navigationAgent;

	[Export] public float maxSpeed = 2.0f;


	public Vector3 MovementTarget
	{
		get { return _navigationAgent.TargetPosition; }
		set { _navigationAgent.TargetPosition = value; }
	}

	private bool navigationLock = true;

	public override async void _Ready()
	{
		base._Ready();

		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		if (_navigationAgent == null)
		{
			GD.PrintErr("NavigationAgent3D node not found!");
			return;
		}



		// Ensure the NavigationServer is synchronized
		await WaitForNavigationSync();

		// Adjust these values for the actor's speed and navigation layout.
		_navigationAgent.PathDesiredDistance = 0.7f;
		_navigationAgent.TargetDesiredDistance = 0.7f;

		// Unlock navigation after setup is complete
		navigationLock = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;
		Vector3 hVelocity = velocity;

		if (!IsOnFloor())
		{
			velocity.Y += GetGravity().Y * (float)delta;
		}

		if (!navigationLock && !_navigationAgent.IsNavigationFinished())
		{
			Vector3 currentAgentPosition = GlobalTransform.Origin;
			Vector3 nextPathPosition = _navigationAgent.GetNextPathPosition();

			Vector3 desiredVelocity = currentAgentPosition.DirectionTo(nextPathPosition) * maxSpeed;


			float dt = (float)delta / reactionTime;
			hVelocity = hVelocity.Lerp(desiredVelocity, dt); // Adjust the multiplier for smoothness

		}
		else if (_navigationAgent.IsNavigationFinished())
		{
			float dt = (float)delta / decelerationTime;
			hVelocity = hVelocity.Lerp(Vector3.Zero, dt);
		}

		Velocity = new Vector3(hVelocity.X, velocity.Y, hVelocity.Z);
		MoveAndSlide();
	}



	private async Task WaitForNavigationSync()
	{
		// Wait for the NavigationServer to finish synchronizing
		while (_navigationAgent.GetNavigationMap() != null &&
			   NavigationServer3D.MapGetIterationId(_navigationAgent.GetNavigationMap()) == 0)
		{
			await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		}
	}


	public override void _UnhandledInput(InputEvent @event)
	{
		if (navigationLock) return;

		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
			{
				Vector2 mousePos = GetViewport().GetMousePosition();
				Vector3 origin = camera.ProjectRayOrigin(mousePos);
				Vector3 normal = camera.ProjectRayNormal(mousePos);

				var spaceState = GetWorld3D().DirectSpaceState;

				var query = PhysicsRayQueryParameters3D.Create(origin, origin + normal * 1000);
				query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };

				var result = spaceState.IntersectRay(query);

				if (result.Count > 0)
				{
					MovementTarget = result["position"].AsVector3();
				}

			}
		}
	}
}
