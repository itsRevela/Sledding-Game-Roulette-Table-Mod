using System;
using System.Linq;
using FluentAssertions;
using RouletteRevival.Hud;
using Xunit;

namespace RouletteRevival.Tests.Hud
{
    public sealed class HudPathsTests
    {
        [Fact]
        public void Required_ListsEveryHudElementOnce()
        {
            HudPaths.Required.Should().HaveCount(9).And.OnlyHaveUniqueItems();
        }

        [Fact]
        public void Required_ElementsAllLiveUnderTheMenuPanelOrAreItsAncestors()
        {
            var elements = HudPaths.Required.Where(p => p != HudPaths.Canvas && p != HudPaths.Panel);

            elements.Should().OnlyContain(p => p.StartsWith(HudPaths.Panel + "/", StringComparison.Ordinal));
            HudPaths.Panel.Should().StartWith(HudPaths.Canvas + "/");
        }

        [Fact]
        public void FindMissing_AllPresent_ReturnsEmpty()
        {
            HudPaths.FindMissing(_ => true).Should().BeEmpty();
        }

        [Fact]
        public void FindMissing_ReportsOnlyAbsentPaths()
        {
            var missing = HudPaths.FindMissing(p => p != HudPaths.MaxWinText && p != HudPaths.ClearBetsButton);

            missing.Should().Equal(HudPaths.MaxWinText, HudPaths.ClearBetsButton);
        }

        [Fact]
        public void FindMissing_NothingPresent_ReturnsAllRequired()
        {
            HudPaths.FindMissing(_ => false).Should().Equal(HudPaths.Required);
        }

        [Fact]
        public void FindMissing_NullLookup_Throws()
        {
            Action act = () => HudPaths.FindMissing(null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
