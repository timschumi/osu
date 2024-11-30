// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsReadyButton : ReadyButton
    {
        [Resolved(typeof(Room), nameof(Room.EndDate))]
        private Bindable<DateTimeOffset?> endDate { get; set; }

        [Resolved(typeof(Room), nameof(Room.MaxAttempts))]
        private Bindable<int?> maxAttempts { get; set; }

        [Resolved(typeof(Room), nameof(Room.UserScore))]
        private Bindable<PlaylistAggregateScore> userScore { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public PlaylistsReadyButton()
        {
            Text = "Start";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Green;
        }

        private bool hasRemainingAttempts = true;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userScore.BindValueChanged(aggregate =>
            {
                if (maxAttempts.Value == null)
                    return;

                int remaining = maxAttempts.Value.Value - aggregate.NewValue.PlaylistItemAttempts.Sum(a => a.Attempts);

                hasRemainingAttempts = remaining > 0;
            });
        }

        protected override void Update()
        {
            base.Update();

            Enabled.Value = hasRemainingAttempts && enoughTimeLeft();
        }

        public override LocalisableString TooltipText
        {
            get
            {
                if (!enoughTimeLeft())
                    return "No time left!";

                if (!hasRemainingAttempts)
                    return "Attempts exhausted!";

                return base.TooltipText;
            }
        }

        private bool enoughTimeLeft()
        {
            // this doesn't consider mods which apply variable rates, yet.
            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            double hitLength = Math.Round(gameBeatmap.Value.Track.Length / rate);

            // This should probably consider the length of the currently selected item, rather than a constant 30 seconds.
            return endDate.Value != null && DateTimeOffset.UtcNow.AddSeconds(30 + 9 * 60 * 60).AddMilliseconds(hitLength) < endDate.Value;
        }
    }
}
