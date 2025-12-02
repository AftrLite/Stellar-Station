using Content.Stellar.Shared._ES.Chat.Obfuscation;
using Content.Stellar.Shared._ES.Chat.Obfuscation.Components;
using Content.Shared.Chat;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Stellar.Server._ES.Chat.Obfuscation;

/// <inheritdoc/>
public sealed class ESVoiceObfuscatorSystem : ESSharedVoiceObfuscatorSystem
{
    [Dependency] private readonly MaskSystem _mask = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESVoiceObfuscatorComponent, InventoryRelayedEvent<TransformSpeakerNameEvent>>(OnTransformSpeakerName);
    }

    private void OnTransformSpeakerName(Entity<ESVoiceObfuscatorComponent> ent, ref InventoryRelayedEvent<TransformSpeakerNameEvent> args)
    {
        if (_mask.IsToggled(ent.Owner))
            return;

        args.Args.VoiceName = GetObfuscatedVoice(args.Owner);
    }
}
