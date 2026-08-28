// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    /// <summary>
    /// A sidebar for the skin editor that contains a tab bar for switching
    /// between skin-level settings and per-component settings.
    /// </summary>
    internal partial class SkinEditorSettingsSidebar : Container
    {
        public const float WIDTH = EditorSidebar.WIDTH;
        public const float TAB_HEIGHT = 36;

        private readonly Box background;
        private readonly FillFlowContainer<Drawable> content;

        private readonly BindableInt selectedTab = new BindableInt(0);

        private Container<Drawable> skinPanel = null!;
        private Container<Drawable> componentPanel = null!;

        public SkinEditorSettingsSidebar()
        {
            Width = WIDTH;
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // Scroll region fills the whole sidebar, with its top padded below the tab bar
                // so the content never extends underneath or beyond it.
                new OsuScrollContainer
                {
                    ScrollbarOverlapsContent = false,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = TAB_HEIGHT },
                    Child = content = new FillFlowContainer<Drawable>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding(EditorSidebar.PADDING),
                        Direction = FillDirection.Vertical,
                    },
                },
                // Pinned to the top and drawn above the scroll region.
                new TabBar
                {
                    RelativeSizeAxes = Axes.X,
                    Height = TAB_HEIGHT,
                    SelectedTab = { BindTarget = selectedTab },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selectedTab.BindValueChanged(_ => updateTabVisibility(), true);
        }

        public void SetSkinContent(Drawable[] sections)
        {
            content.Clear();
            skinPanel = new FillFlowContainer<Drawable>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(0, 10),
                Children = sections,
            };
            content.Add(skinPanel);
            updateTabVisibility();
        }

        public void SetComponentContent(Drawable[] sections)
        {
            componentPanel = new FillFlowContainer<Drawable>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new osuTK.Vector2(0, 10),
                Children = sections,
            };
            content.Add(componentPanel);
            updateTabVisibility();
        }

        public void ClearAll()
        {
            content.Clear();
            skinPanel = null!;
            componentPanel = null!;
        }

        private void updateTabVisibility()
        {
            bool showingSkin = selectedTab.Value == 0;

            if (skinPanel != null)
                skinPanel.Alpha = showingSkin ? 1 : 0;

            if (componentPanel != null)
                componentPanel.Alpha = showingSkin ? 0 : 1;
        }

        private partial class TabBar : CompositeDrawable
        {
            public readonly BindableInt SelectedTab = new BindableInt(0);

            private readonly string[] tabLabels = { "Skin", "Component" };
            private TabButton[] buttons = null!;
            private Container indicator = null!;

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.X;
                Height = TAB_HEIGHT;

                buttons = new TabButton[tabLabels.Length];
                float tabWidth = WIDTH / tabLabels.Length;

                InternalChildren = new Drawable[]
                {
                    // Background.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4,
                    },
                    // The full-width bottom line (dim).
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = colourProvider.Background5,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                    },
                    // The sliding white indicator.
                    indicator = new Container
                    {
                        Width = tabWidth,
                        Height = 2,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White,
                        },
                    },
                    // Tab buttons overlay.
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Children = buttons = tabLabels.Select((label, i) => new TabButton(label, i, tabWidth, colourProvider)
                        {
                            SelectedTab = { BindTarget = SelectedTab },
                        }).ToArray(),
                    },
                };

                SelectedTab.BindValueChanged(e =>
                {
                    indicator.MoveToX(e.NewValue * tabWidth, 200, Easing.OutQuint);
                }, true);
            }

            private partial class TabButton : OsuClickableContainer
            {
                private readonly int index;
                private readonly OverlayColourProvider colourProvider;
                private readonly Box background;
                private readonly OsuSpriteText label;

                public readonly BindableInt SelectedTab = new BindableInt();

                public TabButton(string text, int index, float width, OverlayColourProvider colourProvider)
                {
                    this.index = index;
                    this.colourProvider = colourProvider;

                    Width = width;
                    Height = TAB_HEIGHT;

                    Action = () => SelectedTab.Value = index;

                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        label = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Style.Heading2,
                            Text = text,
                        },
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    SelectedTab.BindValueChanged(_ => updateState(), true);
                }

                private void updateState()
                {
                    bool isActive = SelectedTab.Value == index;

                    if (isActive)
                    {
                        background.Colour = colourProvider.Background5;
                        label.Colour = Colour4.White;
                    }
                    else
                    {
                        background.Colour = colourProvider.Background4;
                        label.Colour = colourProvider.Light3;
                    }
                }
            }
        }
    }
}
