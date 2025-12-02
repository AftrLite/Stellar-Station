using Content.Stellar.Shared._ES.Chat.Obfuscation.Components;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement.Components;

namespace Content.Stellar.Shared._ES.Chat.Obfuscation;

/// <summary>
/// This handles <see cref="ESVoiceObfuscatorComponent"/>
/// </summary>
public abstract class ESSharedVoiceObfuscatorSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ESVoiceObfuscatorComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<ESVoiceObfuscatorComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("es-voice-obfuscator-examine"));
    }

    public string GetObfuscatedVoice(Entity<HumanoidAppearanceComponent?> ent)
    {
        // Non-humanoids have special logic since they don't have identity
        if (!Resolve(ent, ref ent.Comp, false))
        {
            if (TryComp<ESGenericVoiceComponent>(ent, out var voice))
                return Loc.GetString(voice.Voice);

            return Prototype(ent)?.Name ?? string.Empty;
        }

        var species = ent.Comp.Species;
        var age = ent.Comp.Age;

        var name = Name(ent);
        var gender = ent.Comp.Gender;
        var ageRepresentation = _humanoidAppearance.GetAgeRepresentation(species, age);
        var identityRepresentation = new IdentityRepresentation(name, gender, ageRepresentation);

        return identityRepresentation.ToStringUnknown();
    }
}
