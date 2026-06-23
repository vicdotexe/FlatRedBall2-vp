using System;
using System.Numerics;
using FlatRedBall2.Collision;
using FlatRedBall2.Math;
using Shouldly;
using Xunit;

namespace FlatRedBall2.Tests.Collision;

public class PolygonTests
{
    [Fact]
    public void SetPoints_ReplacesAllExistingPoints()
    {
        var polygon = Polygon.FromPoints(new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 10) });

        polygon.SetPoints(new[] { new Vector2(1, 2), new Vector2(3, 4) });

        polygon.Points.Count.ShouldBe(2);
        polygon.Points[0].ShouldBe(new Vector2(1, 2));
        polygon.Points[1].ShouldBe(new Vector2(3, 4));
    }

    [Fact]
    public void SetPoints_EmptySequence_ClearsAllPoints()
    {
        var polygon = Polygon.FromPoints(new[] { new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 10) });

        polygon.SetPoints(Array.Empty<Vector2>());

        polygon.Points.ShouldBeEmpty();
    }

    // Issue #378: positive Rotation must turn the polygon counterclockwise in world space (Y+ up),
    // per the documented Angle convention ("FromDegrees(90) points up (0,1); positive is CCW")
    // and PathFollower's facing math. A thin triangle whose apex points along +X should, after a
    // +90° rotation, point along +Y (CCW). If it rotated clockwise the apex would land on -Y.
    [Fact]
    public void Rotation_PositiveAngle_RotatesCounterclockwise()
    {
        // Apex at +X; narrow base on the Y axis so the "pointing" direction is unambiguous.
        var polygon = Polygon.FromPoints(new[]
        {
            new Vector2(0f, -1f),
            new Vector2(0f,  1f),
            new Vector2(10f, 0f),
        });

        // Sanity: at rest the apex points along +X.
        polygon.Contains(new Vector2(8f, 0f)).ShouldBeTrue();

        polygon.Rotation = Angle.FromDegrees(90f);

        // CCW: the +X apex swings up to +Y.
        polygon.Contains(new Vector2(0f, 8f)).ShouldBeTrue();
        // It must NOT have swung down to -Y (that would be clockwise).
        polygon.Contains(new Vector2(0f, -8f)).ShouldBeFalse();
    }
}
