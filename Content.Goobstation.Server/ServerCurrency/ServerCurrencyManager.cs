// SPDX-FileCopyrightText: 2025 ChatGPT
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.ServerCurrency;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.ServerCurrency
{
    /// <summary>
    /// Lightweight currency manager ported from Goob.
    /// Runtime-only storage (no DB persistence yet).
    /// </summary>
    public sealed class ServerCurrencyManager : ICommonCurrencyManager
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        private readonly Dictionary<NetUserId, int> _balances = new();

        public event Action? ClientBalanceChange;
        public event Action<PlayerBalanceChangeEvent>? BalanceChange;

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public bool CanAfford(NetUserId? userId, int amount, out int balance)
        {
            balance = GetBalance(userId);
            return balance >= amount && balance - amount >= 0;
        }

        public string Stringify(int amount) => amount == 1
            ? $"{amount} {Loc.GetString("server-currency-name-singular")}"
            : $"{amount} {Loc.GetString("server-currency-name-plural")}";

        public int AddCurrency(NetUserId userId, int amount)
        {
            return ModifyBalance(userId, amount);
        }

        public int RemoveCurrency(NetUserId userId, int amount)
        {
            return ModifyBalance(userId, -amount);
        }

        public (int, int) TransferCurrency(NetUserId sourceUserId, NetUserId targetUserId, int amount)
        {
            var src = ModifyBalance(sourceUserId, -amount);
            var dst = ModifyBalance(targetUserId, amount);
            return (src, dst);
        }

        public int SetBalance(NetUserId userId, int amount)
        {
            var oldBalance = GetBalance(userId);

            _balances[userId] = Math.Max(0, amount);

            RaiseBalanceChanged(userId, _balances[userId], oldBalance);
            return oldBalance;
        }

        public int GetBalance(NetUserId? userId = null)
        {
            if (userId == null)
                return 0;

            return _balances.GetValueOrDefault(userId.Value, 0);
        }

        private int ModifyBalance(NetUserId userId, int delta)
        {
            var oldBalance = GetBalance(userId);
            var newBalance = Math.Max(0, oldBalance + delta);
            _balances[userId] = newBalance;

            RaiseBalanceChanged(userId, newBalance, oldBalance);
            return newBalance;
        }

        private void RaiseBalanceChanged(NetUserId userId, int newBalance, int oldBalance)
        {
            if (_playerManager.TryGetSessionById(userId, out var session))
            {
                BalanceChange?.Invoke(new PlayerBalanceChangeEvent(session, userId, newBalance, oldBalance));
            }
        }
    }
}
