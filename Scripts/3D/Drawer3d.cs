using Godot;
using System;

public partial class Drawer3d : Control
{
	[Export] public Camera3D camera;

	public void DrawLine3D(Vector3 from, Vector3 to, Color color, float width = -1.0f, bool antialised = false)
	{
		if (camera == null)
		{
			GD.PushWarning("Camera is not assigned.");
			return;
		}

		if (camera.IsPositionBehind(from) || camera.IsPositionBehind(to))
		{
			GD.PushWarning("One or more points are behind the camera and cannot be projected.");
			return;
		}

		Vector2 from2 = camera.UnprojectPosition(from);

		Vector2 to2 = camera.UnprojectPosition(to);

		DrawLine(from2, to2, color, width, antialised);
	}

	public void DrawVector3D(Vector3 origin, Vector3 direction, Color color, float width = -1.0f, bool antialised = false)
	{
		if (camera == null)
		{
			GD.PushWarning("Camera is not assigned.");
			return;
		}

		// Calculate the end point in 3D space
		Vector3 end = origin + direction;

		// Project the points to 2D canvas space
		Vector2 origin2D = camera.UnprojectPosition(origin);
		Vector2 end2D = camera.UnprojectPosition(end);

		// Skip drawing if the points are behind the camera
		if (camera.IsPositionBehind(origin) || camera.IsPositionBehind(end))
		{
			GD.PushWarning("One or more points are behind the camera and cannot be projected.");
			return;
		}

		// Draw the main line for the vector
		DrawLine(origin2D, end2D, color, width, antialised);

		// Calculate arrowhead size (1/3 of the vector length)
		float vectorLength = direction.Length();
		float arrowheadSize = 0.5f;

		// Draw the arrowhead
		if (arrowheadSize > 0.0f)
		{
			// Normalize direction and calculate perpendicular vectors
			Vector3 vectorDirection = direction.Normalized();
			Vector3 perpendicular1 = new Vector3(-vectorDirection.Z, 0, vectorDirection.X).Normalized() * arrowheadSize * 0.5f;
			Vector3 perpendicular2 = new Vector3(vectorDirection.Z, 0, -vectorDirection.X).Normalized() * arrowheadSize * 0.5f;

			// Create two arrowhead points
			Vector3 arrowPoint1 = end - vectorDirection * arrowheadSize + perpendicular1;
			Vector3 arrowPoint2 = end - vectorDirection * arrowheadSize + perpendicular2;

			// Project the arrowhead points to 2D
			Vector2 arrowPoint1_2D = camera.UnprojectPosition(arrowPoint1);
			Vector2 arrowPoint2_2D = camera.UnprojectPosition(arrowPoint2);

			// Draw the arrowhead lines
			DrawLine(end2D, arrowPoint1_2D, color, width, antialised);
			DrawLine(end2D, arrowPoint2_2D, color, width, antialised);
		}
	}


	//playtest


	public override void _Draw()
	{
		DrawVector3D(Vector3.One, 2 * Vector3.One, Colors.Red, 10);
	}

	public override void _Process(double delta)
	{
		QueueRedraw();
	}
}
