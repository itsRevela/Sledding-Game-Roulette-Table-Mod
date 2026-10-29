using FluentAssertions;
using RouletteRevival.Spawning;
using Xunit;

namespace RouletteRevival.Tests.Spawning
{
    public sealed class PrefabNamesTests
    {
        [Theory]
        [InlineData("Roulette Setup")]
        [InlineData("Roulette Setup(Clone)")]
        [InlineData("Roulette Setup (Clone)")]
        public void IsRouletteSetup_PrefabAndClones_ReturnsTrue(string name)
        {
            PrefabNames.IsRouletteSetup(name).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Roulette Table")]
        [InlineData("roulette setup")]
        [InlineData("Roulette Setup Extra")]
        public void IsRouletteSetup_OtherNames_ReturnsFalse(string name)
        {
            PrefabNames.IsRouletteSetup(name).Should().BeFalse();
        }
    }
}
