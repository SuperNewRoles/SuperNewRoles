using FluentAssertions;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

public class HawkPatchTests
{
    // 親設定がOFFなら、子設定の内容に関係なく死亡後手動ズーム自体が無効。
    [Fact]
    public void EvaluateManualDeadZoom_IgnoresChildRestrictions_WhenParentOptionIsDisabled()
    {
        var result = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead: false,
            isDead: true,
            isMeetingOpen: false,
            requireCompletedTasks: true,
            isTaskTriggerRole: true,
            completedTasks: 0,
            totalTasks: 3,
            disableForGhostOrGuardianAngel: true,
            ghostRole: GhostRoleId.Cantera,
            isGuardianAngel: true
        );

        result.CanUseManualZoom.Should().BeFalse();
        result.ShouldResetToDefault.Should().BeFalse();
    }

    // タスク条件は「未完了なら不可 / 完了済みなら可 / 必要数0なら可」を確認する。
    [Theory]
    [InlineData(1, 2, false, true)]
    [InlineData(2, 2, true, false)]
    [InlineData(0, 0, true, false)]
    public void EvaluateManualDeadZoom_AppliesTaskCompletionRestriction(int completedTasks, int totalTasks, bool expectedCanUse, bool expectedReset)
    {
        var result = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead: true,
            isDead: true,
            isMeetingOpen: false,
            requireCompletedTasks: true,
            isTaskTriggerRole: true,
            completedTasks: completedTasks,
            totalTasks: totalTasks,
            disableForGhostOrGuardianAngel: false,
            ghostRole: GhostRoleId.None,
            isGuardianAngel: false
        );

        result.CanUseManualZoom.Should().Be(expectedCanUse);
        result.ShouldResetToDefault.Should().Be(expectedReset);
    }

    // 幽霊役職制限は Mod 幽霊役職と GuardianAngel の両方に掛かる。
    [Theory]
    [InlineData(GhostRoleId.Cantera, false, false, true)]
    [InlineData(GhostRoleId.None, true, false, true)]
    [InlineData(GhostRoleId.None, false, true, false)]
    public void EvaluateManualDeadZoom_AppliesGhostRestriction(GhostRoleId ghostRole, bool isGuardianAngel, bool expectedCanUse, bool expectedReset)
    {
        var result = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead: true,
            isDead: true,
            isMeetingOpen: false,
            requireCompletedTasks: false,
            isTaskTriggerRole: false,
            completedTasks: 0,
            totalTasks: 0,
            disableForGhostOrGuardianAngel: true,
            ghostRole: ghostRole,
            isGuardianAngel: isGuardianAngel
        );

        result.CanUseManualZoom.Should().Be(expectedCanUse);
        result.ShouldResetToDefault.Should().Be(expectedReset);
    }

    // タスク条件をONにしていても、非タスクトリガー役職ならこの制限は無視される。
    [Fact]
    public void EvaluateManualDeadZoom_IgnoresTaskRestriction_ForNonTaskTriggerRoles()
    {
        var result = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead: true,
            isDead: true,
            isMeetingOpen: false,
            requireCompletedTasks: true,
            isTaskTriggerRole: false,
            completedTasks: 0,
            totalTasks: 5,
            disableForGhostOrGuardianAngel: false,
            ghostRole: GhostRoleId.None,
            isGuardianAngel: false
        );

        result.CanUseManualZoom.Should().BeTrue();
        result.ShouldResetToDefault.Should().BeFalse();
    }

    // 子設定は OR 条件なので、どちらか一方でも失敗すれば死亡後手動ズーム不可。
    [Fact]
    public void EvaluateManualDeadZoom_DisablesZoom_WhenAnyAdditionalRestrictionFails()
    {
        var result = DeadZoomHelper.EvaluateManualDeadZoom(
            enabledZoomOnDead: true,
            isDead: true,
            isMeetingOpen: false,
            requireCompletedTasks: true,
            isTaskTriggerRole: true,
            completedTasks: 1,
            totalTasks: 3,
            disableForGhostOrGuardianAngel: true,
            ghostRole: GhostRoleId.Revenant,
            isGuardianAngel: false
        );

        result.CanUseManualZoom.Should().BeFalse();
        result.ShouldResetToDefault.Should().BeTrue();
    }
}
