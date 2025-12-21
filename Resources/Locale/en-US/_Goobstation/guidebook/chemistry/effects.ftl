# SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
# SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
#
# SPDX-License-Identifier: AGPL-3.0-or-later

reagent-effect-guidebook-deal-stamina-damage =
    { $chance ->
        [1] { $deltasign ->
                [1] Deals
                *[-1] Heals
            }
        *[other]
            { $deltasign ->
                [1] deal
                *[-1] heal
            }
    } { $amount } { $immediate ->
                    [true] immediate
                    *[false] overtime
                  } stamina damage

reagent-effect-guidebook-stealth-entities = Маскирует живых мобов поблизости.

reagent-effect-guidebook-change-faction = Меняет фракцию моба на «{$faction}».

reagent-effect-guidebook-mutate-plants-nearby = Случайным образом мутирует ближайшие растения.

reagent-effect-guidebook-dnascramble = Шифрует ДНК человека.

reagent-effect-guidebook-change-species = Превращает цель в {$species}.

reagent-effect-guidebook-sex-change = Меняет гендер человека.

reagent-effect-guidebook-immunity-modifier =
    { $chance ->
        [1] Modifies
        *[other] modify
    } immunity gain rate by {NATURALFIXED($gainrate, 5)}, strength by {NATURALFIXED($strength, 5)} for at least {NATURALFIXED($time, 3)} {MANY("second", $time)}

reagent-effect-guidebook-disease-progress-change =
    { $chance ->
        [1] Modifies
        *[other] modify
    } progress of {$type} diseases by {NATURALFIXED($amount, 5)}
