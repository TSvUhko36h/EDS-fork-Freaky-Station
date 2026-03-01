// SPDX-FileCopyrightText: 2026 ChatGPT
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using System.Numerics;
using Content.Goobstation.Common.ServerCurrency;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Goobstation.Client.ServerCurrency.UI;

public sealed class RouletteWindow : DefaultWindow
{
    [Dependency] private readonly ICommonCurrencyManager _currency = default!;

    public event Action<int, int>? SpinRequested;

    private readonly Label _balanceLabel;
    private readonly Label _slotOne;
    private readonly Label _slotTwo;
    private readonly Label _slotThree;
    private readonly LineEdit _betInput;
    private readonly Button _spinButton;
    private readonly Label _resultLabel;

    private bool _isSpinning;
    private int _pendingSpinId;
    private DateTime _spinStartedAt;

    private readonly string[] _slotValues =
    {
        "[   ]", "[=  ]", "[== ]", "[===]", "[ ==]", "[  =]"
    };

    public RouletteWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("gs-balanceui-roulette-label");
        MinSize = new Vector2(420, 300);

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(8)
        };
        Contents.AddChild(root);

        _balanceLabel = new Label { HorizontalAlignment = HAlignment.Center };
        root.AddChild(_balanceLabel);
        root.AddChild(new Control { MinSize = new Vector2(0, 8) });

        var slotsRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            SeparationOverride = 8
        };
        root.AddChild(slotsRow);

        _slotOne = BuildSlot(slotsRow);
        _slotTwo = BuildSlot(slotsRow);
        _slotThree = BuildSlot(slotsRow);

        root.AddChild(new Control { MinSize = new Vector2(0, 10) });

        var controlsRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center
        };
        root.AddChild(controlsRow);

        _betInput = new LineEdit
        {
            PlaceHolder = Loc.GetString("gs-balanceui-roulette-bet"),
            MinWidth = 170
        };
        controlsRow.AddChild(_betInput);

        _spinButton = new Button
        {
            Text = Loc.GetString("gs-balanceui-roulette-spin")
        };
        _spinButton.AddStyleClass("ButtonColorRed");
        controlsRow.AddChild(_spinButton);

        root.AddChild(new Control { MinSize = new Vector2(0, 6) });

        _resultLabel = new Label { HorizontalAlignment = HAlignment.Center };
        root.AddChild(_resultLabel);

        _spinButton.OnPressed += _ => RequestSpin();
        _currency.ClientBalanceChange += UpdateBalanceLabel;
        OnClose += () => _currency.ClientBalanceChange -= UpdateBalanceLabel;

        UpdateSlotsForResult(0f, 0);
        UpdateBalanceLabel();
    }

    private Label BuildSlot(Control parent)
    {
        var panel = new PanelContainer
        {
            MinSize = new Vector2(110, 70)
        };
        parent.AddChild(panel);

        var slot = new Label
        {
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            Text = "x1"
        };
        panel.AddChild(slot);
        return slot;
    }

    private void RequestSpin()
    {
        if (_isSpinning)
            return;

        if (!TryParsePositiveInt(_betInput.Text, out var bet))
        {
            _resultLabel.Text = Loc.GetString("gs-balanceui-roulette-invalid-bet");
            return;
        }

        _isSpinning = true;
        _pendingSpinId++;
        _spinStartedAt = DateTime.UtcNow;
        _spinButton.Disabled = true;
        _resultLabel.Text = Loc.GetString("gs-balanceui-roulette-spinning");
        TickSlots(0);
        SpinRequested?.Invoke(bet, _pendingSpinId);
    }

    public void ApplyResult(int spinId, int bet, int payout, float multiplier)
    {
        if (spinId != _pendingSpinId)
            return;

        var elapsed = DateTime.UtcNow - _spinStartedAt;
        var minSpinTime = TimeSpan.FromSeconds(2.8);
        if (elapsed < minSpinTime)
        {
            Timer.Spawn(minSpinTime - elapsed, () => FinalizeResult(bet, payout, multiplier));
            return;
        }

        FinalizeResult(bet, payout, multiplier);
    }

    public void SetBalance(int balance)
    {
        _balanceLabel.Text = Loc.GetString("gs-balanceui-roulette-balance", ("balance", _currency.Stringify(balance)));
    }

    private void FinalizeResult(int bet, int payout, float multiplier)
    {
        _isSpinning = false;
        _spinButton.Disabled = false;
        UpdateSlotsForResult(multiplier, payout);

        var formattedMultiplier = multiplier.ToString("0.##", CultureInfo.InvariantCulture);
        _resultLabel.Text = payout <= 0
            ? Loc.GetString("gs-balanceui-roulette-result-lose", ("bet", bet), ("multiplier", formattedMultiplier))
            : Loc.GetString("gs-balanceui-roulette-result-win", ("bet", bet), ("multiplier", formattedMultiplier), ("payout", payout));
    }

    private void TickSlots(int frame)
    {
        if (!_isSpinning)
            return;

        var i1 = frame % _slotValues.Length;
        var i2 = (frame + 2) % _slotValues.Length;
        var i3 = (frame + 4) % _slotValues.Length;
        _slotOne.Text = _slotValues[i1];
        _slotTwo.Text = _slotValues[i2];
        _slotThree.Text = _slotValues[i3];

        Timer.Spawn(TimeSpan.FromMilliseconds(170), () => TickSlots(frame + 1));
    }

    private void UpdateSlotsForResult(float multiplier, int payout)
    {
        if (Math.Abs(multiplier - 10f) < 0.001f)
        {
            _slotOne.Text = "JACK";
            _slotTwo.Text = "POT!";
            _slotThree.Text = "x10";
            return;
        }

        if (payout <= 0)
        {
            _slotOne.Text = "LOSE";
            _slotTwo.Text = "LOSE";
            _slotThree.Text = "LOSE";
            return;
        }

        if (multiplier >= 2f)
        {
            _slotOne.Text = "BIG";
            _slotTwo.Text = "WIN";
            _slotThree.Text = "!!!";
            return;
        }

        _slotOne.Text = "WIN";
        _slotTwo.Text = "WIN";
        _slotThree.Text = "WIN";
    }

    private void UpdateBalanceLabel()
    {
        SetBalance(_currency.GetBalance());
    }

    private static bool TryParsePositiveInt(string? value, out int amount)
    {
        if (!int.TryParse(value, out amount))
            return false;

        return amount > 0;
    }
}
