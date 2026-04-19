#nullable enable
using Content.IntegrationTests.Fixtures.Attributes;

namespace Content.IntegrationTests.Tests._Citadel;

/// <summary>
///     Marks a field on a GameTest inheritor as needing to be populated with a system from the given side.
/// </summary>
/// <seealso cref="GameTest"/>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SystemAttribute : Attribute
{
    public SystemAttribute(Side side)
    {
        Side = side;

        if (side == Side.Neither)
        {
            throw new NotSupportedException();
        }
    }

    public Side Side { get; }
}
