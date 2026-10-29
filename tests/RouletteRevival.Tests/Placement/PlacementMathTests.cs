using System;
using FluentAssertions;
using RouletteRevival.Placement;
using Xunit;

namespace RouletteRevival.Tests.Placement
{
    public sealed class PlacementMathTests
    {
        private const float Tolerance = 0.001f;

        [Fact]
        public void InFrontOf_FacingNorth_PlacesTableAlongPositiveZ()
        {
            var pose = PlacementMath.InFrontOf(10f, 5f, 20f, playerYawDegrees: 0f, distance: 2f);

            pose.X.Should().BeApproximately(10f, Tolerance);
            pose.Y.Should().Be(5f);
            pose.Z.Should().BeApproximately(22f, Tolerance);
        }

        [Fact]
        public void InFrontOf_FacingEast_PlacesTableAlongPositiveX()
        {
            var pose = PlacementMath.InFrontOf(0f, 0f, 0f, playerYawDegrees: 90f, distance: 3f);

            pose.X.Should().BeApproximately(3f, Tolerance);
            pose.Z.Should().BeApproximately(0f, Tolerance);
        }

        [Fact]
        public void InFrontOf_TurnsTableSideOnToThePlayer()
        {
            var pose = PlacementMath.InFrontOf(0f, 0f, 0f, playerYawDegrees: 300f);

            pose.YawDegrees.Should().BeApproximately(30f, Tolerance);
        }

        [Fact]
        public void InFrontOf_UsesRelocateDistanceByDefault()
        {
            var pose = PlacementMath.InFrontOf(0f, 0f, 0f, playerYawDegrees: 0f);

            pose.Z.Should().BeApproximately(PlacementMath.RelocateDistance, Tolerance);
        }

        [Fact]
        public void InFrontOf_NegativeDistance_Throws()
        {
            Action act = () => PlacementMath.InFrontOf(0f, 0f, 0f, 0f, distance: -1f);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(0f, 0f)]
        [InlineData(360f, 0f)]
        [InlineData(450f, 90f)]
        [InlineData(-90f, 270f)]
        [InlineData(-720f, 0f)]
        public void NormalizeYaw_WrapsIntoZeroTo360(float input, float expected)
        {
            PlacementMath.NormalizeYaw(input).Should().BeApproximately(expected, Tolerance);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void NormalizeYaw_NonFinite_ReturnsZero(float input)
        {
            PlacementMath.NormalizeYaw(input).Should().Be(0f);
        }

        [Fact]
        public void ResolveConfigured_UsesCustomPose_WhenEnabledAndValid()
        {
            var custom = new TablePose(1f, 2f, 3f, 45f);

            PlacementMath.ResolveConfigured(true, custom).Should().Be(custom);
        }

        [Fact]
        public void ResolveConfigured_FallsBackToDefault_WhenCustomDisabled()
        {
            var custom = new TablePose(1f, 2f, 3f, 45f);

            PlacementMath.ResolveConfigured(false, custom).Should().Be(PlacementMath.DefaultPose);
        }

        [Fact]
        public void ResolveConfigured_FallsBackToDefault_WhenCustomInvalid()
        {
            var broken = new TablePose(float.NaN, 2f, 3f, 45f);

            PlacementMath.ResolveConfigured(true, broken).Should().Be(PlacementMath.DefaultPose);
        }

        [Fact]
        public void DefaultPose_IsValid()
        {
            PlacementMath.DefaultPose.IsValid.Should().BeTrue();
        }
    }
}
