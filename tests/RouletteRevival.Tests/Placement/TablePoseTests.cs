using FluentAssertions;
using RouletteRevival.Placement;
using Xunit;

namespace RouletteRevival.Tests.Placement
{
    public sealed class TablePoseTests
    {
        [Fact]
        public void IsValid_FiniteInRangeValues_ReturnsTrue()
        {
            new TablePose(33.5f, 30.3f, 133f, 90f).IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(float.NaN, 0f, 0f, 0f)]
        [InlineData(0f, float.PositiveInfinity, 0f, 0f)]
        [InlineData(0f, 0f, float.NegativeInfinity, 0f)]
        [InlineData(0f, 0f, 0f, float.NaN)]
        [InlineData(TablePose.MaxCoordinate + 1f, 0f, 0f, 0f)]
        [InlineData(0f, -TablePose.MaxCoordinate - 1f, 0f, 0f)]
        public void IsValid_BrokenValues_ReturnsFalse(float x, float y, float z, float yaw)
        {
            new TablePose(x, y, z, yaw).IsValid.Should().BeFalse();
        }

        [Fact]
        public void WithY_ReturnsCopyAndLeavesOriginalUnchanged()
        {
            var original = new TablePose(1f, 2f, 3f, 4f);

            var moved = original.WithY(9f);

            moved.Should().Be(new TablePose(1f, 9f, 3f, 4f));
            original.Y.Should().Be(2f);
        }

        [Fact]
        public void Equality_ComparesAllComponents()
        {
            new TablePose(1f, 2f, 3f, 4f).Should().Be(new TablePose(1f, 2f, 3f, 4f));
            new TablePose(1f, 2f, 3f, 4f).Should().NotBe(new TablePose(1f, 2f, 3f, 5f));
        }
    }
}
