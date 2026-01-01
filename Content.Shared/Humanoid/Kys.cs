using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Dataset;
namespace Content.Shared.Humanoid
{
    public enum ERPS : byte
    {
        Yes,
        No
    }
    public record struct ERPChangEvent(ERPS OldERPS, ERPS NewERPS);
}
