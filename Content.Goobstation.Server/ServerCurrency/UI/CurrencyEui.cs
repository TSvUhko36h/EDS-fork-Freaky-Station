// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency;
using Content.Goobstation.Shared.ServerCurrency.UI;
using Content.Server.Administration.Notes;
using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Random;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Goobstation.Server.ServerCurrency.UI
{
    public sealed class CurrencyEui : BaseEui
    {
        [Dependency] private readonly ICommonCurrencyManager _currencyMan = default!;
        [Dependency] private readonly IAdminNotesManager _notesMan = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IChatManager _chat = default!;

        private bool _hasRouletteResult;
        private int _lastRouletteBet;
        private int _lastRoulettePayout;
        private float _lastRouletteMultiplier;
        private int _lastRouletteSpinId;
        private bool _spinPending;
        private int _pendingBet;
        private int _pendingSpinId;

        private const float SpinResolveDelaySeconds = 2.8f;

        private static readonly object PotLock = new();
        private static int _serverDepositedCoins;

        public CurrencyEui()
        {
            IoCManager.InjectDependencies(this);
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new CurrencyEuiState(
                _hasRouletteResult,
                _lastRouletteBet,
                _lastRoulettePayout,
                _lastRouletteMultiplier,
                _lastRouletteSpinId);
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);
            if (msg is CurrencyEuiMsg.Buy buy)
            {
                BuyToken(buy.TokenId, Player);
                StateDirty();
                return;
            }

            if (msg is CurrencyEuiMsg.SpinRoulette spin)
            {
                QueueSpin(spin.Bet, spin.SpinId);
            }
        }

        private void QueueSpin(int bet, int spinId)
        {
            if (_spinPending || bet <= 0)
                return;

            _spinPending = true;
            _pendingBet = bet;
            _pendingSpinId = spinId;

            Timer.Spawn(TimeSpan.FromSeconds(SpinResolveDelaySeconds), () =>
            {
                ResolveSpin(Player.UserId, Player.Name);
                StateDirty();
            });
        }

        private void ResolveSpin(NetUserId userId, string playerName)
        {
            if (!_spinPending)
                return;

            _spinPending = false;
            var bet = _pendingBet;
            var spinId = _pendingSpinId;
            _pendingBet = 0;
            _pendingSpinId = 0;

            var balance = _currencyMan.GetBalance(userId);
            if (balance < bet)
            {
                SetResult(spinId, bet, 0, 0f);
                return;
            }

            _currencyMan.RemoveCurrency(userId, bet);
            AddToServerPot(bet);

            var multiplier = RollDynamicMultiplier(out var jackpot);
            var payout = (int) MathF.Floor(bet * multiplier);

            if (payout > 0)
                _currencyMan.AddCurrency(userId, payout);

            SetResult(spinId, bet, payout, multiplier);

            if (jackpot)
            {
                var pot = GetServerPot();
                var message = Loc.GetString("gs-roulette-jackpot-notify",
                    ("player", playerName),
                    ("amount", _currencyMan.Stringify(payout)),
                    ("pot", _currencyMan.Stringify(pot)));
                _chat.ChatMessageToAll(
                    Content.Shared.Chat.ChatChannel.OOC,
                    message,
                    message,
                    EntityUid.Invalid,
                    false,
                    true,
                    colorOverride: Color.FromHex("#fff0ff", Color.Honeydew));
            }
        }

        private void SetResult(int spinId, int bet, int payout, float multiplier)
        {
            if (payout > 0)
            {
                _hasRouletteResult = true;
                _lastRouletteBet = bet;
                _lastRoulettePayout = payout;
                _lastRouletteMultiplier = multiplier;
                _lastRouletteSpinId = spinId;
                return;
            }

            _hasRouletteResult = true;
            _lastRouletteBet = bet;
            _lastRoulettePayout = 0;
            _lastRouletteMultiplier = multiplier;
            _lastRouletteSpinId = spinId;
        }

        private float RollDynamicMultiplier(out bool jackpot)
        {
            jackpot = false;
            var roll = _random.NextFloat();

            if (roll < 0.45f)
                return 0f;

            if (roll < 0.75f)
                return RoundMultiplier(_random.NextFloat(0.15f, 1.25f));

            if (roll < 0.93f)
                return RoundMultiplier(_random.NextFloat(1.25f, 3.8f));

            if (roll < 0.99f)
                return RoundMultiplier(_random.NextFloat(3.8f, 8.8f));

            jackpot = true;
            return 10f;
        }

        private static float RoundMultiplier(float value)
        {
            return MathF.Round(value * 100f) / 100f;
        }

        private static void AddToServerPot(int amount)
        {
            lock (PotLock)
            {
                _serverDepositedCoins += amount;
            }
        }

        private static int GetServerPot()
        {
            lock (PotLock)
            {
                return _serverDepositedCoins;
            }
        }

        private async void BuyToken(ProtoId<TokenListingPrototype> tokenId, ICommonSession playerName)
        {
            var balance = _currencyMan.GetBalance(Player.UserId);

            if (!_protoMan.TryIndex<TokenListingPrototype>(tokenId, out var token))
                return;

            if (balance < token.Price)
                return;

            await _notesMan.AddAdminRemark(Player, Player.UserId, 0,
                Loc.GetString(token.AdminNote), 0, false, null);
            _currencyMan.RemoveCurrency(Player.UserId, token.Price);
        }
    }
}
