// SPDX-FileCopyrightText: 2025 EmoGarbage404
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Stellar.Shared._ES.Chat.Obfuscation.Components;

/// <summary>
/// Component that provides a "general" voice to use when the entity's voice is obfuscated via <see cref="ESVoiceObfuscatorComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ESSharedVoiceObfuscatorSystem))]
public sealed partial class ESGenericVoiceComponent : Component
{
    /// <summary>
    /// String that will be used as the voice name
    /// </summary>
    [DataField(required: true)]
    public LocId Voice;
}
