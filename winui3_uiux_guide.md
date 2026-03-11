# WinUI 3 UI/UX Guide

_Derived from reviewing the UnigetUI production application_

---

## 1. Window Chrome & Backdrop

### Mica Background

Use `MicaBackdrop` for the window backdrop to achieve the modern Windows 11 translucent glass effect.

```xml
<Window.SystemBackdrop>
    <MicaBackdrop Kind="Base" />
</Window.SystemBackdrop>
```

**Variants:**

- `Kind="Base"` — subtle tint (recommended for main windows)
- `Kind="BaseAlt"` — slightly stronger tint for secondary surfaces

### TitleBar

Use the WinUI 3 `TitleBar` control (not legacy `AppWindow.TitleBar` APIs). Place a global search box in `TitleBar.Content` to maximize space:

```xml
<TitleBar
    Title="MyApp"
    IsBackButtonVisible="False"
    IsPaneToggleButtonVisible="True"
    PaneToggleRequested="TitleBar_PaneToggleRequested">
    <TitleBar.Content>
        <AutoSuggestBox Width="400" Height="32" QueryIcon="Find" />
    </TitleBar.Content>
    <TitleBar.RightHeader>
        <!-- optional: user avatar, notification bell, etc. -->
    </TitleBar.RightHeader>
</TitleBar>
```

---

## 2. Navigation Pattern

### NavigationView (Sidebar)

Use `NavigationView` as the top-level shell. Key configuration:

```xml
<NavigationView
    CompactModeThresholdWidth="800"
    CompactPaneLength="68"
    ExpandedModeThresholdWidth="1600"
    IsBackButtonVisible="Collapsed"
    IsPaneOpen="False"
    IsPaneToggleButtonVisible="False"
    IsSettingsVisible="False"
    OpenPaneLength="250"
    SelectionChanged="NavigationView_SelectionChanged">

    <!-- Remove borders between nav pane and content -->
    <NavigationView.Resources>
        <SolidColorBrush x:Key="NavigationViewContentBackground" Color="Transparent" />
        <Thickness x:Key="NavigationViewContentGridBorderThickness">0,0,0,0</Thickness>
        <Thickness x:Key="NavigationViewMinimalContentGridBorderThickness">0,0,0,0</Thickness>
    </NavigationView.Resources>
```

### CustomNavViewItem

Wrap `NavigationViewItem` in a custom control so you can expose `GlyphIcon`, `FontSize`, `FontWeight`, and `InfoBadge` as first-class properties:

```xml
<!-- Main nav items -->
<NavigationView.MenuItems>
    <controls:CustomNavViewItem
        FontSize="16"
        FontWeight="SemiBold"
        GlyphIcon="&#xF6FA;"   <!-- Segoe MDL2 glyph -->
        Tag="Discover"
        Text="Discover Packages" />

    <controls:CustomNavViewItem Tag="Updates" Text="Software Updates">
        <controls:CustomNavViewItem.InfoBadge>
            <InfoBadge Name="UpdatesBadge" Visibility="Collapsed" Value="0" />
        </controls:CustomNavViewItem.InfoBadge>
    </controls:CustomNavViewItem>
</NavigationView.MenuItems>

<!-- Pinned footer items (Settings, More…) -->
<NavigationView.FooterMenuItems>
    <controls:CustomNavViewItem Tag="Settings" Text="Settings">
        <controls:CustomNavViewItem.Icon>
            <AnimatedIcon>
                <AnimatedIcon.Source>
                    <animatedvisuals:AnimatedSettingsVisualSource />
                </AnimatedIcon.Source>
                <AnimatedIcon.FallbackIconSource>
                    <FontIconSource Glyph="&#xE713;" />
                </AnimatedIcon.FallbackIconSource>
            </AnimatedIcon>
        </controls:CustomNavViewItem.Icon>
    </controls:CustomNavViewItem>
</NavigationView.FooterMenuItems>
```

**Rules:**

- Primary nav items → `MenuItems`, use `FontSize="16" FontWeight="SemiBold"`
- Utility items (Settings, Help, More) → `FooterMenuItems`, use `IconSize="20"` only
- Use `InfoBadge` with `Visibility="Collapsed"` by default; show only when count > 0
- **Never** use `IsSettingsVisible="True"` — create an explicit settings nav item instead

---

## 3. Page Layout Conventions

### Three-Column Centering Grid

Every page should center its content up to a max width using a 3-column grid:

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="1000000*" MaxWidth="2000" />  <!-- content column -->
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <!-- Place all content in Column="1" -->
</Grid>
```

For settings pages use `MaxWidth="800"`.

### Page Header

Each page should have a consistent header with:

- A large `FontIcon` or `Image` (50–60px) on the left
- A `StackPanel` with a **bold title** (`FontSize="30"`, `FontFamily="Segoe UI Variable Display"`) and a muted subtitle (`FontSize="11"`, reduced opacity)
- Action controls top-right (reload button, view mode selector)

```xml
<Grid Name="MainHeader" Grid.Row="0" Grid.Column="1" ColumnSpacing="8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="80" />
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <FontIcon x:Name="HeaderIcon" Grid.Column="0" FontSize="50" Width="60" MinHeight="60" />

    <StackPanel Grid.Column="1" VerticalAlignment="Center" Spacing="0">
        <TextBlock
            x:Name="MainTitle"
            FontFamily="Segoe UI Variable Display"
            FontSize="30"
            FontWeight="Bold"
            TextWrapping="Wrap" />
        <TextBlock
            x:Name="MainSubtitle"
            FontSize="11"
            Foreground="{ThemeResource AppBarItemDisabledForegroundThemeBrush}"
            TextWrapping="Wrap" />
    </StackPanel>

    <!-- Top-right actions -->
    <Button Grid.Column="2" Width="32" Height="32" Padding="0" CornerRadius="4">
        <FontIcon FontSize="16" Glyph="&#xE72C;" />
    </Button>
</Grid>
```

---

## 4. Animations & Loading States

> **Package:** `CommunityToolkit.WinUI.Animations` for all `Implicit.*` and `Explicit.*` animation APIs.

---

### 4a. Page / UserControl Enter Animation (Implicit)

Every `Page` and `UserControl` that navigates into view should animate in with a **vertical slide up + fade**. Place this directly on the root element of the page:

```xml
<Page ...>
    <animations:Implicit.ShowAnimations>
        <animations:TranslationAnimation From="0,100,0" To="0,0,0" Duration="0:0:0.25" />
        <animations:OpacityAnimation    From="0"       To="1"     Duration="0:0:0.25" />
    </animations:Implicit.ShowAnimations>

    <!-- page content -->
</Page>
```

**Timing:** 250ms is the standard. Do not go shorter (feels glitchy) or longer (feels sluggish).  
**Direction:** Always slide upward (`From="0,100,0"`) — never downward. This matches Windows 11 system animations.  
**Easing:** The default `EaseOut` on implicit animations is correct — do not override it.

---

### 4b. Tab Panel Enter Animation (Implicit — triggered by SwitchPresenter)

When switching between tabs or `SwitchPresenter` cases, apply a **shorter vertical slide + fade** to each panel's root element. This fires automatically whenever the case becomes active (i.e., made visible):

```xml
<!-- Inside each Toolkit:Case -->
<StackPanel Spacing="8">
    <animations:Implicit.ShowAnimations>
        <animations:TranslationAnimation From="0,20,0" To="0,0,0" Duration="0:0:0.3" />
        <animations:OpacityAnimation    From="0"      To="1.0"   Duration="0:0:0.3" />
    </animations:Implicit.ShowAnimations>

    <!-- tab content -->
</StackPanel>
```

**Difference from page animation:** Slide distance is `20px` not `100px` — the context is smaller (sub-panel, not a full page).  
**Duration:** 300ms — slightly slower than the page transition because the panel is smaller and needs more time to feel deliberate.

> **How it works:** `Implicit.ShowAnimations` fires whenever the element's `Visibility` changes from `Collapsed → Visible`. `SwitchPresenter` collapses inactive cases, so this is triggered automatically on every tab switch — no code-behind needed.

---

### 4c. Splash Screen / App-Load Animation (Explicit)

The splash that appears while the app loads is built from two elements (logo image + title text) that **slide in from opposite sides**, creating a converging effect. These are explicit named animations triggered from code-behind.

```xml
<!-- Logo slides in from the right -->
<Border Margin="-500,0,20,0"
        BorderBrush="{ThemeResource ApplicationPageBackgroundThemeBrush}"
        BorderThickness="500,0,0,0"
        Canvas.ZIndex="1">
    <animations:Explicit.Animations>
        <animations:AnimationSet x:Name="InAnimation_Border">
            <animations:TranslationAnimation
                EasingMode="EaseOut"
                From="225,0" To="0,0"
                Duration="0:0:0.7" />
        </animations:AnimationSet>
    </animations:Explicit.Animations>
    <Image Width="96" Source="ms-appx:///Assets/Images/icon.png" />
</Border>

<!-- Title text slides in from the left + fades in -->
<Border Margin="0" Padding="0,10" HorizontalAlignment="Right">
    <animations:Explicit.Animations>
        <animations:AnimationSet x:Name="InAnimation_Text">
            <animations:TranslationAnimation
                EasingMode="EaseOut"
                From="-225,0" To="0,0"
                Duration="0:0:0.7" />
            <animations:OpacityAnimation
                EasingMode="EaseOut"
                From="0" To="1"
                Duration="0:0:0.7" />
        </animations:AnimationSet>
    </animations:Explicit.Animations>
    <StackPanel VerticalAlignment="Center">
        <TextBlock FontFamily="Segoe UI Variable Display"
                   FontSize="90" FontWeight="ExtraBlack"
                   Text="MyApp" />
    </StackPanel>
</Border>
```

Trigger from code-behind after startup completes:
```csharp
await InAnimation_Border.StartAsync();
await InAnimation_Text.StartAsync();
// or fire both in parallel:
_ = InAnimation_Border.StartAsync();
await InAnimation_Text.StartAsync();
```

**Design notes:**
- The `Border` with `BorderThickness="500,0,0,0"` and matching `BorderBrush` is a clipping trick — it masks the left edge of the logo so it appears to slide out from behind the title text.
- Logo translates `+225px → 0` (enters from right); text translates `-225px → 0` (enters from left).
- Duration is 700ms — deliberately slow to give the user a moment of brand recognition on first load.

---

### 4d. Hover / Press Transition (BrushTransition)

For any `Border` or `Button` whose background changes on hover/press (e.g., `AppBarButton`, custom toolbar buttons), add a `BrushTransition` to smooth the color change:

```xml
<Border Background="{ThemeResource ControlFillColorDefaultBrush}">
    <Border.BackgroundTransition>
        <BrushTransition Duration="0:0:0.083" />
    </Border.BackgroundTransition>
</Border>
```

**Duration:** 83ms (≈ 5 frames at 60fps) — fast enough to feel immediate, slow enough to avoid jarring snaps.  
Apply this in the `AppBarButton` style override in [App.xaml](file:///c:/Users/DELL/projects/UnigetUI/src/UniGetUI/App.xaml) so it applies globally.

---

### 4e. AnimatedIcon — Interactive State Animations

Use `AnimatedIcon` instead of `FontIcon` for interactive elements that benefit from micro-animations (back button, settings gear, search magnifier):

```xml
<AnimatedIcon>
    <AnimatedIcon.Source>
        <animatedvisuals:AnimatedBackVisualSource />    <!-- back button -->
        <!-- or: AnimatedSettingsVisualSource, AnimatedFindVisualSource -->
    </AnimatedIcon.Source>
    <AnimatedIcon.FallbackIconSource>
        <SymbolIconSource Symbol="Back" />
    </AnimatedIcon.FallbackIconSource>
</AnimatedIcon>
```

Always provide a `FallbackIconSource` — it renders if Lottie animations are unavailable.

**Where to use `AnimatedIcon`:**
| Location | Source |
|---|---|
| Nav item → Settings | `AnimatedSettingsVisualSource` |
| Settings back button | `AnimatedBackVisualSource` |
| Search / Find button | `AnimatedFindVisualSource` |
| Any `AppBarButton` with hover feedback | wrap in `AnimatedIcon` |

---

### 4f. Loading States

#### ① ProgressBar — Page-Level Loading Indicator

Place an indeterminate `ProgressBar` at the very top of the content area (above the list card), **outside** the scrollable area. It sits `Margin="1,-6,1,0"` so it overlaps the top border of the list card:

```xml
<Grid Name="PackagesListGrid">
    <!-- Indeterminate bar — collapse when loading finishes -->
    <ProgressBar
        Name="LoadingProgressBar"
        Margin="1,-6,1,0"
        HorizontalAlignment="Stretch"
        VerticalAlignment="Top"
        IsIndeterminate="True"
        Visibility="Visible" />   <!-- set Collapsed from code-behind when done -->

    <!-- List card below -->
    <Grid Padding="4,6" CornerRadius="8" ... />
</Grid>
```

Dismiss from code-behind:
```csharp
LoadingProgressBar.Visibility = Visibility.Collapsed;
```

**Never** use `ProgressRing` for page-level loading — it takes up too much space and blocks content. Use the thin `ProgressBar` so the content layout is already visible while data loads.

---

#### ② ProgressBar — Inline / Field-Level Loading

When a `ComboBox` (or any field) is loading its options asynchronously, overlay a thin `ProgressBar` on the **top edge** of the field using the same grid cell:

```xml
<Grid ColumnSpacing="8">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="*" MaxWidth="200" />
    </Grid.ColumnDefinitions>

    <TextBlock Grid.Column="0" VerticalAlignment="Center" Text="Version to install:" />

    <ComboBox Name="VersionComboBox"
              Grid.Column="1" HorizontalAlignment="Stretch"
              SelectionChanged="VersionComboBox_SelectionChanged" />

    <!-- Overlaid at the top of the ComboBox, same grid cell -->
    <ProgressBar Name="VersionProgress"
                 Grid.Column="1"
                 Margin="1,0,1,0"
                 VerticalAlignment="Top"
                 CornerRadius="4,4,0,0"
                 IsIndeterminate="True"
                 Visibility="Visible" />
</Grid>
```

**Key details:**
- `VerticalAlignment="Top"` + `Margin="1,0,1,0"` pins it to the top edge of the cell
- `CornerRadius="4,4,0,0"` rounds only the top corners so it looks like it belongs to the ComboBox
- Collapse it as soon as the `ComboBox.ItemsSource` is set

---

#### ③ ProgressRing — Splash / App Startup

Use `ProgressRing` **only** on the startup/splash screen, where the UI is otherwise empty:

```xml
<ProgressRing
    Name="LoadingIndicator"
    Margin="0,0,0,-48"
    VerticalAlignment="Bottom"
    HorizontalAlignment="Center"
    Foreground="#08a9c3"
    IsIndeterminate="True"
    Visibility="Visible" />
```

**Notes:**
- Hardcoded `Foreground` color ties it to the brand accent — intentional on a splash screen.
- `-48` bottom margin offsets it below the logo/text so it doesn't overlap.
- Collapse it from code-behind once the main UI is ready to navigate.

---

#### ④ BackdropBlur Overlay — Locked / Disabled State

When a section of a dialog or page is temporarily locked (e.g., "follow global settings" is ON so the local settings panel is inaccessible), cover it with a frosted-glass overlay using `BackdropBlurBrush`:

```xml
<Border x:Name="PlaceholderBanner"
        Grid.RowSpan="5"
        Margin="-8"
        Padding="32,0">
    <Border.Background>
        <media:BackdropBlurBrush Amount="5.0" />
    </Border.Background>
    <Grid>
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center"
                    Orientation="Vertical" Spacing="12">
            <TextBlock x:Name="PlaceholderText"
                       FontSize="20" FontWeight="SemiBold"
                       HorizontalAlignment="Center"
                       TextWrapping="Wrap" />
            <Button x:Name="UnlockButton"
                    HorizontalAlignment="Center"
                    Click="UnlockButton_Click">
                <TextBlock Text="Change this and unlock" />
            </Button>
        </StackPanel>
    </Grid>
</Border>
```

**xmlns:** `xmlns:media="using:CommunityToolkit.WinUI.Media"`  
**Amount:** `5.0` — enough to clearly frost the background without making the text underneath completely unreadable.  
**Margin="-8"** — bleed slightly outside the parent padding so the blur covers right to the edges.  
Show/hide this overlay by toggling `Visibility` — the blur is rendered live so no dispose needed.

---

#### ⑤ Empty-State TextBlock

When a list has no items (no results, no packages), show a centred muted `TextBlock` overlaid on the list area:

```xml
<!-- In the same Grid row as the ItemsView -->
<TextBlock Name="BackgroundText"
           Grid.Row="1"
           VerticalAlignment="Center"
           TextAlignment="Center"
           TextWrapping="Wrap"
           FontFamily="Segoe UI Variable Display"
           FontSize="30"
           FontWeight="Bold"
           Opacity="0.5" />
```

Set the text from code-behind based on context:
```csharp
BackgroundText.Text = CoreTools.Translate("No packages found");
BackgroundText.Visibility = FilteredItems.Count == 0
    ? Visibility.Visible
    : Visibility.Collapsed;
```

**Sizing:** `FontSize="30"`, `FontWeight="Bold"`, `Opacity="0.5"` — large enough to be instantly understood, muted enough not to compete with actual content.

---

#### ⑥ InfoBadge — Count Indicator on Nav Items

Show a numeric badge on nav items when there is something to act on (e.g. pending updates):

```xml
<controls:CustomNavViewItem Tag="Updates" Text="Software Updates">
    <controls:CustomNavViewItem.InfoBadge>
        <InfoBadge Name="UpdatesBadge" Visibility="Collapsed" Value="0" />
    </controls:CustomNavViewItem.InfoBadge>
</controls:CustomNavViewItem>
```

Update from code-behind:
```csharp
UpdatesBadge.Value = pendingCount;
UpdatesBadge.Visibility = pendingCount > 0
    ? Visibility.Visible
    : Visibility.Collapsed;
```

**Rule:** Always start `Visibility="Collapsed"` — never show a badge with `Value="0"`.

---

### Animation Quick Reference

| Scenario | Type | Duration | From → To |
|---|---|---|---|
| Page enter | `Implicit.ShowAnimations` | 250ms | `0,100,0 → 0,0,0` + opacity `0→1` |
| Tab / panel switch | `Implicit.ShowAnimations` | 300ms | `0,20,0 → 0,0,0` + opacity `0→1` |
| Splash logo (right→centre) | `Explicit` EaseOut | 700ms | `225,0 → 0,0` |
| Splash text (left→centre) | `Explicit` EaseOut | 700ms | `-225,0 → 0,0` + opacity `0→1` |
| Hover/press color | `BrushTransition` | 83ms | automatic |
| Nav/Settings/Back icons | `AnimatedIcon` | system-managed | — |
| Page loading | `ProgressBar` (top edge, `IsIndeterminate`) | — | Collapse when done |
| Field loading (ComboBox) | `ProgressBar` (top edge, `CornerRadius="4,4,0,0"`) | — | Collapse when ItemsSource set |
| App startup spinner | `ProgressRing` | — | Collapse after first navigate |
| Locked section | `BackdropBlurBrush Amount="5.0"` | — | Toggle `Visibility` |
| Empty list | `TextBlock Opacity="0.5"` | — | Toggle `Visibility` |
| Pending count | `InfoBadge` on nav item | — | Set `Value`, toggle `Visibility` |

---

## 5. Notification & Feedback Patterns

### InfoBar — Full-Width Banners

Use `InfoBar` for app-level or page-level messages. Full-width banners sit between the `TitleBar` and the page content:

```xml
<InfoBar
    Name="ErrorBanner"
    BorderThickness="0,1,0,1"
    CornerRadius="0"
    IsOpen="False"
    Severity="Error"
    Visibility="{x:Bind ErrorBanner.IsOpen, Mode=OneWay}" />
```

**Severities:** `Informational` · `Success` · `Warning` · `Error`

Use `Visibility="{x:Bind Banner.IsOpen, Mode=OneWay}"` to collapse the space when closed.

For page-level notifications (e.g., "restart required"), use `CornerRadius="8"` instead.

### TeachingTip — Dismissable Tooltips

Use `TeachingTip` for first-run or feature-discovery hints, floating over the UI:

```xml
<TeachingTip
    x:Name="DismissableNotification"
    PlacementMargin="20"
    PreferredPlacement="Auto" />
```

Set content in code-behind:

```csharp
DismissableNotification.Title = "New in this version";
DismissableNotification.Subtitle = "...";
DismissableNotification.IsOpen = true;
```

---

## 6. Settings UI Pattern

### SettingsCard-Based Widgets

Extend `CommunityToolkit.WinUI.Controls.SettingsCard` to create reusable settings controls. Each card provides a label (`Header`) and a value control (`Content`).

**CheckboxCard (Toggle):**

```csharp
public class CheckboxCard : SettingsCard
{
    public ToggleSwitch _checkbox;
    public TextBlock _textblock;

    public CheckboxCard()
    {
        _checkbox = new ToggleSwitch
        {
            OnContent  = new TextBlock { Text = "Enabled"  },
            OffContent = new TextBlock { Text = "Disabled" },
        };
        _textblock = new TextBlock { TextWrapping = TextWrapping.Wrap };

        Content = _checkbox;
        Header  = _textblock;

        _checkbox.Toggled += OnToggled;
    }
}
```

**Variants to build:**
| Widget | Content Control |
|---|---|
| [CheckboxCard](file:///c:/Users/DELL/projects/UnigetUI/src/UniGetUI/Controls/SettingsWidgets/CheckboxCard.cs#13-106) | `ToggleSwitch` |
| `ComboboxCard` | `ComboBox` |
| `TextboxCard` | `TextBox` |
| `ButtonCard` | `Button` |
| `CheckboxButtonCard` | `CheckBox` + `Button` side by side |

Group related cards in `SettingsExpander` or use section `TextBlock` headers with `FontWeight="SemiBold"`.

### Settings Page Shell

```xml
<Page Background="Transparent">
    <Grid RowSpacing="8">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="1000*" MaxWidth="800" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />  <!-- InfoBar (restart required) -->
            <RowDefinition Height="Auto" />  <!-- Header with back button + title -->
            <RowDefinition Height="*" />     <!-- Content Frame -->
        </Grid.RowDefinitions>

        <InfoBar Name="RestartRequired" Grid.Row="0" Grid.Column="1" CornerRadius="8" Severity="Warning" />

        <Grid Name="SettingsHeaderGrid" Grid.Row="1" Grid.Column="1" ColumnSpacing="10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            <Button Name="BackButton" Width="40" Height="40" Padding="6"
                    Background="Transparent" BorderThickness="0">
                <AnimatedIcon>
                    <AnimatedIcon.Source><animatedvisuals:AnimatedBackVisualSource /></AnimatedIcon.Source>
                    <AnimatedIcon.FallbackIconSource><SymbolIconSource Symbol="Back" /></AnimatedIcon.FallbackIconSource>
                </AnimatedIcon>
            </Button>
            <TextBlock Name="SettingsTitle" Grid.Column="1"
                       FontSize="30" FontWeight="Bold" TextWrapping="Wrap" />
        </Grid>

        <Frame Name="MainNavigationFrame" Grid.Row="2" Grid.Column="1" />
    </Grid>
</Page>
```

---

## 7. Data List Pattern (MVVM + Multi-View)

### Wrapper Pattern (INotifyPropertyChanged)

Never bind UI directly to domain model objects. Create a `*Wrapper` (or `*ViewModel`) class. Keep all display-state logic in the wrapper so the XAML templates stay declarative:

```csharp
public partial class ItemWrapper : INotifyPropertyChanged, IDisposable
{
    public IDomainItem Item { get; }
    public event PropertyChangedEventHandler? PropertyChanged;

    // Two-way checked state forwarded to the model
    public bool IsChecked
    {
        get => Item.IsChecked;
        set {
            Item.IsChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
        }
    }

    // Computed display values — never put this logic in converters
    public string DisplayVersion => Item.IsUpgradable
        ? $"{Item.Version} → {Item.NewVersion}"
        : Item.Version;

    // Opacity dimming for in-progress / unavailable states
    public float ListedOpacity => Item.Tag switch {
        ItemTag.OnQueue        => 0.5f,
        ItemTag.BeingProcessed => 0.5f,
        ItemTag.Unavailable    => 0.5f,
        _                      => 1.0f,
    };

    // Badge icon id for overlaid status indicators
    public IconType StatusIconId => Item.Tag switch {
        ItemTag.AlreadyInstalled => IconType.Installed_Filled,
        ItemTag.IsUpgradable     => IconType.Upgradable_Filled,
        ItemTag.Pinned           => IconType.Pin_Filled,
        ItemTag.Failed           => IconType.Warning_Filled,
        _                        => IconType.Empty,
    };

    // Icon loading with cache
    private static readonly ConcurrentDictionary<long, Uri?> _iconCache = new();
    public ImageSource? MainIconSource { get; private set; }
    public bool ShowCustomIcon { get; private set; }
    public bool ShowDefaultIcon { get; private set; } = true;

    public void UpdateIcon()
    {
        if (_iconCache.TryGetValue(Item.GetHash(), out Uri? uri))
        {
            MainIconSource = new BitmapImage { UriSource = uri, DecodePixelWidth = 64 };
            ShowCustomIcon  = true;
            ShowDefaultIcon = false;
        }
        else { ShowCustomIcon = false; ShowDefaultIcon = true; }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainIconSource)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowCustomIcon)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowDefaultIcon)));
    }

    public void Dispose() => Item.PropertyChanged -= Item_PropertyChanged;
}
```

---

### Column-Header Row (List View)

The list container is a named `Grid` with `CornerRadius="8"` and a **37px sticky header row** above the `ItemsView`. Each column header is a zero-border `Button` (so it's keyboard-focusable and clickable to sort), spanning the same column widths as the items below.

```xml
<Grid Padding="4,6" Background="{ThemeResource SystemFillColorNeutralBackgroundBrush}"
      BorderBrush="{StaticResource ExpanderContentBorderBrush}"
      BorderThickness="1" CornerRadius="8">
    <Grid.RowDefinitions>
        <RowDefinition Height="37" />     <!-- column headers -->
        <RowDefinition Height="*" />      <!-- ItemsView -->
    </Grid.RowDefinitions>

    <!-- Header row — mirror the column widths of PackageTemplate_List exactly -->
    <Grid Grid.Row="0" Margin="4,2,4,5" ColumnSpacing="0">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="8" />
            <ColumnDefinition Width="32" />           <!-- checkbox area -->
            <ColumnDefinition Width="28" />           <!-- icon gutter -->
            <ColumnDefinition Width="*" MinWidth="100" />  <!-- Name -->
            <ColumnDefinition Width="32" />
            <ColumnDefinition Width="*" MinWidth="100" />  <!-- Id -->
            <ColumnDefinition Width="32" />
            <ColumnDefinition Width="*" MaxWidth="125" />  <!-- Version -->
            <ColumnDefinition Width="32" MaxWidth="{x:Bind NewVersionIconWidth}" />
            <ColumnDefinition Width="*" MaxWidth="{x:Bind NewVersionLabelWidth}" />  <!-- New Version (updates only) -->
            <ColumnDefinition Width="32" />
            <ColumnDefinition Width="*" MaxWidth="150" />  <!-- Source -->
            <ColumnDefinition Width="11" />
        </Grid.ColumnDefinitions>

        <!-- Select-all checkbox wrapped in a button so the whole cell is clickable -->
        <Button Name="CheckboxHeader" Grid.Column="0" Grid.ColumnSpan="3"
                Padding="0" HorizontalAlignment="Stretch"
                BorderThickness="0" CornerRadius="4,0,0,4">
            <CheckBox Name="SelectAllCheckBox" Margin="12,4,4,4"
                      Checked="SelectAll_ValueChanged" Unchecked="SelectAll_ValueChanged" />
        </Button>

        <!-- Sortable column headers — label text set in code-behind -->
        <Button Name="NameHeader"       Grid.Column="3"  Grid.ColumnSpan="1" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" BorderThickness="0" CornerRadius="0" />
        <Button Name="IdHeader"         Grid.Column="4"  Grid.ColumnSpan="2" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" BorderThickness="0" CornerRadius="0" />
        <Button Name="VersionHeader"    Grid.Column="6"  Grid.ColumnSpan="2" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" BorderThickness="0" CornerRadius="0" />
        <Button Name="NewVersionHeader" Grid.Column="8"  Grid.ColumnSpan="2" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" BorderThickness="0" CornerRadius="0"
                Visibility="{x:Bind RoleIsUpdateLike}" />
        <Button Name="SourceHeader"     Grid.Column="10" Grid.ColumnSpan="3" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" BorderThickness="0" CornerRadius="0,4,4,0" />
    </Grid>

    <!-- SwitchPresenter swaps the ItemsView without destroying + rebuilding it -->
    <Toolkit:SwitchPresenter Grid.Row="1" TargetType="x:Int32"
                             Value="{x:Bind ViewModeSelector.SelectedIndex, Mode=OneWay}">
        <Toolkit:Case IsDefault="True" Value="0">
            <ItemsView x:Name="PackageList_List" Padding="4,0"
                       ItemTemplate="{StaticResource ItemTemplate_List}"
                       ItemsSource="{x:Bind FilteredItems}"
                       Layout="{StaticResource Layout_List}"
                       CanBeScrollAnchor="False"
                       CharacterReceived="PackageList_CharacterReceived" />
        </Toolkit:Case>
        <Toolkit:Case Value="1">
            <ItemsView x:Name="PackageList_Grid" Padding="4,0"
                       ItemTemplate="{StaticResource ItemTemplate_Grid}"
                       ItemsSource="{x:Bind FilteredItems}"
                       Layout="{StaticResource Layout_Grid}"
                       CanBeScrollAnchor="False"
                       CharacterReceived="PackageList_CharacterReceived" />
        </Toolkit:Case>
        <Toolkit:Case Value="2">
            <ItemsView x:Name="PackageList_Icons" Padding="4,0"
                       ItemTemplate="{StaticResource ItemTemplate_Icons}"
                       ItemsSource="{x:Bind FilteredItems}"
                       Layout="{StaticResource Layout_Icons}"
                       CanBeScrollAnchor="False"
                       CharacterReceived="PackageList_CharacterReceived" />
        </Toolkit:Case>
    </Toolkit:SwitchPresenter>

    <!-- Empty-state overlay — bold 30px muted text, centered over the list -->
    <TextBlock Name="BackgroundText" Grid.Row="1"
               VerticalAlignment="Center" TextAlignment="Center" TextWrapping="Wrap"
               FontFamily="Segoe UI Variable Display" FontSize="30" FontWeight="Bold"
               Opacity="0.5" />
</Grid>
```

> **Why `SwitchPresenter` instead of `Visibility` toggling?**  
> It keeps only one `ItemsView` in the visual tree at a time, avoiding layout thrash from three overlapping panels, while still letting the runtime cache each view's scroll position.

---

### Three View DataTemplates

**List DataTemplate** — compact row, 30px height, icon-glyph + text in same column:

```xml
<DataTemplate x:Key="ItemTemplate_List" x:DataType="local:ItemWrapper">
    <widgets:ItemContainer AutomationProperties.Name="{x:Bind Item.Name}" CornerRadius="4">
        <Grid Padding="12,3,8,3" ColumnSpacing="4" Opacity="{x:Bind ListedOpacity, Mode=OneWay}">
            <Grid.RowDefinitions><RowDefinition Height="30" /></Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="24" />                    <!-- checkbox -->
                <ColumnDefinition Width="*" MinWidth="125" />      <!-- name (icon+label share column) -->
                <ColumnDefinition Width="*" MinWidth="125" />      <!-- id -->
                <ColumnDefinition Width="*" MaxWidth="150" />      <!-- version -->
                <ColumnDefinition Width="*" MaxWidth="125" />      <!-- new version (updates only) -->
                <ColumnDefinition Width="*" MaxWidth="175" />      <!-- source -->
            </Grid.ColumnDefinitions>

            <CheckBox Grid.Column="0" VerticalAlignment="Center"
                      IsChecked="{x:Bind IsChecked, Mode=TwoWay}" />

            <!-- The icon glyph and the label sit in the same column.
                 The glyph is Width=24, the label has Margin="28,..." to slide right. -->
            <TextBlock Grid.Column="1" Width="24" HorizontalAlignment="Left"
                       VerticalAlignment="Center"
                       FontSize="24" FontWeight="ExtraLight"
                       widgets:IconBuilder.Icon="Package"
                       Visibility="{x:Bind ShowDefaultIcon, Mode=OneWay}"
                       ToolTipService.ToolTip="{x:Bind Item.Name}" />
            <Image    Grid.Column="1" Width="24" HorizontalAlignment="Left"
                      VerticalAlignment="Center"
                      Source="{x:Bind MainIconSource, Mode=OneWay}"
                      Visibility="{x:Bind ShowCustomIcon, Mode=OneWay}"
                      ToolTipService.ToolTip="{x:Bind Item.Name}" />
            <!-- Status badge overlaid bottom-left of the icon -->
            <TextBlock Grid.Column="1" Width="20" Height="20" Margin="8,0,-4,-2"
                       HorizontalAlignment="Left" VerticalAlignment="Bottom"
                       FontSize="20" FontWeight="ExtraLight"
                       Foreground="{ThemeResource AccentAAFillColorTertiaryBrush}"
                       widgets:IconBuilder.Icon="{x:Bind StatusIconId, Mode=OneWay}" />

            <TextBlock Grid.Column="1" Margin="28,-2,0,0" VerticalAlignment="Center"
                       FontSize="13" Text="{x:Bind Item.Name}"
                       ToolTipService.ToolTip="{x:Bind Item.Name}" />
            <TextBlock Grid.Column="2" Margin="28,-2,0,0" VerticalAlignment="Center"
                       FontSize="13" Text="{x:Bind Item.Id}"
                       ToolTipService.ToolTip="{x:Bind Item.Id}" />
            <TextBlock Grid.Column="3" Margin="28,-2,0,0" VerticalAlignment="Center"
                       FontSize="13" Text="{x:Bind Item.VersionString, Mode=OneWay}" />
            <TextBlock Grid.Column="5" Margin="28,-2,0,0" VerticalAlignment="Center"
                       FontSize="13" Text="{x:Bind Item.Source.DisplayName}" />
        </Grid>
    </widgets:ItemContainer>
</DataTemplate>
```

**Grid DataTemplate** — 56px card, icon + stacked name/id/version:

```xml
<DataTemplate x:Key="ItemTemplate_Grid" x:DataType="local:ItemWrapper">
    <widgets:ItemContainer Background="{ThemeResource ControlFillColorDefaultBrush}" CornerRadius="4">
        <Grid Padding="4" ColumnSpacing="4" Opacity="{x:Bind ListedOpacity, Mode=OneWay}">
            <Grid.RowDefinitions><RowDefinition Height="48" /></Grid.RowDefinitions>
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="48" />
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="22" />
            </Grid.ColumnDefinitions>

            <!-- 44px icon with status badge bottom-right -->
            <Image Grid.Column="0" Width="44" HorizontalAlignment="Center" VerticalAlignment="Center"
                   Source="{x:Bind MainIconSource, Mode=OneWay}"
                   Visibility="{x:Bind ShowCustomIcon, Mode=OneWay}" />
            <TextBlock Grid.Column="0" HorizontalAlignment="Center" VerticalAlignment="Center"
                       FontSize="48" FontWeight="ExtraLight"
                       widgets:IconBuilder.Icon="Package"
                       Visibility="{x:Bind ShowDefaultIcon, Mode=OneWay}" />
            <TextBlock Grid.Column="0" Margin="0,0,-4,-2"
                       HorizontalAlignment="Right" VerticalAlignment="Bottom"
                       FontSize="30" FontWeight="ExtraLight"
                       Foreground="{ThemeResource AccentAAFillColorTertiaryBrush}"
                       widgets:IconBuilder.Icon="{x:Bind StatusIconId, Mode=OneWay}" />

            <!-- Stacked name (SemiBold top), id (80% opacity middle), version (50% opacity bottom) -->
            <TextBlock Grid.Column="1" VerticalAlignment="Top"
                       FontFamily="Segoe UI Variable Text" FontSize="14" FontWeight="SemiBold"
                       Text="{x:Bind Item.Name}" />
            <TextBlock Grid.Column="1" VerticalAlignment="Center"
                       FontFamily="Segoe UI Variable Text" FontSize="11" Opacity="0.8"
                       Text="{x:Bind Item.Id}" />
            <TextBlock Grid.Column="1" VerticalAlignment="Bottom"
                       FontFamily="Segoe UI Variable Text" FontSize="11" Opacity="0.5"
                       Text="{x:Bind DisplayVersion, Mode=OneWay}" />

            <CheckBox Grid.Column="2" Margin="1,-4,0,0" HorizontalAlignment="Left" VerticalAlignment="Top"
                      IsChecked="{x:Bind IsChecked, Mode=TwoWay}" />
            <!-- Context-menu button bottom-right of the card -->
            <Button Grid.Column="2" Width="22" Height="22" Padding="0"
                    VerticalAlignment="Bottom" Background="Transparent" BorderThickness="0"
                    Click="{x:Bind ShowContextMenu}">
                <TextBlock widgets:IconBuilder.Glyph="&#xE712;" FontSize="18" />
            </Button>
        </Grid>
    </widgets:ItemContainer>
</DataTemplate>
```

**Icons DataTemplate** — 120px wide 4-row tile:

```xml
<DataTemplate x:Key="ItemTemplate_Icons" x:DataType="local:ItemWrapper">
    <widgets:ItemContainer Background="{ThemeResource ControlFillColorDefaultBrush}" CornerRadius="4">
        <Grid Padding="4" RowSpacing="0" Opacity="{x:Bind ListedOpacity, Mode=OneWay}">
            <Grid.RowDefinitions>
                <RowDefinition Height="22" />    <!-- checkbox + kebab -->
                <RowDefinition Height="60" />    <!-- 64px icon -->
                <RowDefinition Height="30" />    <!-- name -->
                <RowDefinition Height="15" />    <!-- version -->
            </Grid.RowDefinitions>
            <Grid.ColumnDefinitions><ColumnDefinition Width="120" /></Grid.ColumnDefinitions>

            <CheckBox Grid.Row="0" Margin="1,-4,0,0" HorizontalAlignment="Left" VerticalAlignment="Top"
                      IsChecked="{x:Bind IsChecked, Mode=TwoWay}" />
            <Button Grid.Row="0" Width="22" Height="22" Padding="0"
                    HorizontalAlignment="Right" VerticalAlignment="Top"
                    Background="Transparent" BorderThickness="0" Click="{x:Bind ShowContextMenu}">
                <TextBlock widgets:IconBuilder.Glyph="&#xE712;" FontSize="18" />
            </Button>

            <Image Grid.Row="1" Width="64" Height="64" Margin="0,-8,0,4"
                   HorizontalAlignment="Center" VerticalAlignment="Center"
                   Source="{x:Bind MainIconSource, Mode=OneWay}"
                   Visibility="{x:Bind ShowCustomIcon, Mode=OneWay}" />
            <TextBlock Grid.Row="1" Height="64" Margin="0,-8,0,4"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       FontSize="74" FontWeight="ExtraLight"
                       widgets:IconBuilder.Icon="Package"
                       Visibility="{x:Bind ShowDefaultIcon, Mode=OneWay}" />
            <!-- Accent badge overlay -->
            <TextBlock Grid.Row="1" Margin="0,0,20,-2"
                       HorizontalAlignment="Right" VerticalAlignment="Bottom"
                       FontSize="40" FontWeight="ExtraLight"
                       Foreground="{ThemeResource AccentAAFillColorTertiaryBrush}"
                       widgets:IconBuilder.Icon="{x:Bind StatusIconId, Mode=OneWay}" />

            <TextBlock Grid.Row="2" MaxWidth="120" MaxHeight="30"
                       HorizontalAlignment="Center" VerticalAlignment="Center"
                       FontFamily="Segoe UI Variable Text" FontSize="12" FontWeight="SemiBold"
                       HorizontalTextAlignment="Left" TextWrapping="Wrap"
                       Text="{x:Bind Item.Name}" />
            <TextBlock Grid.Row="3" HorizontalAlignment="Center"
                       FontFamily="Segoe UI Variable Text" FontSize="11" Opacity="0.5"
                       Text="{x:Bind DisplayVersion, Mode=OneWay}" />
        </Grid>
    </widgets:ItemContainer>
</DataTemplate>
```

**Layout resources — set once in `Page.Resources`:**

```xml
<UniformGridLayout x:Key="Layout_Grid"
    ItemsStretch="Fill" MinItemHeight="56" MinItemWidth="275"
    MinColumnSpacing="8" MinRowSpacing="8" />

<UniformGridLayout x:Key="Layout_Icons"
    ItemsJustification="Start" MinItemHeight="134" MinItemWidth="128"
    MinColumnSpacing="8" MinRowSpacing="8" />

<StackLayout x:Key="Layout_List" Spacing="3" />
```

**Column sizing summary:**

| Mode  | Min item width | Min item height | Note                                        |
| ----- | -------------- | --------------- | ------------------------------------------- |
| List  | unbounded      | 30 px           | 6-column grid matching header               |
| Grid  | 275 px         | 56 px           | `ItemsStretch="Fill"` — fills row           |
| Icons | 128 px         | 134 px          | `ItemsJustification="Start"` — left-aligned |

---

## 8. Split Operation/Log Panel

Provide a resizable bottom panel for live operation output using `GridSplitter` from CommunityToolkit:

```xml
<Grid RowSpacing="0">
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />         <!-- main content -->
        <RowDefinition Height="16" />        <!-- splitter handle -->
        <RowDefinition Height="Auto" MinHeight="0" MaxHeight="200" />  <!-- operations -->
    </Grid.RowDefinitions>

    <Frame Name="ContentFrame" Grid.Row="0" />

    <controls:GridSplitter Grid.Row="1" Height="12" Margin="1,0" CornerRadius="4" Orientation="Horizontal" />

    <ListView
        Name="OperationList"
        Grid.Row="2"
        SelectionMode="None"
        ItemTemplate="{StaticResource OperationTemplate}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel Margin="-16,0,-12,0" Orientation="Vertical" Spacing="8" />
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ListView.Resources>
            <!-- Remove hover highlight from operation items -->
            <SolidColorBrush x:Key="ItemContainerPointerOverBackground" Color="Transparent" />
            <SolidColorBrush x:Key="ItemContainerPressedBackground" Color="Transparent" />
        </ListView.Resources>
    </ListView>
</Grid>
```

**Operation card template:**

- `CornerRadius="8"`, `BorderThickness="1"`
- Left: 32×24 package icon
- Centre: title TextBlock + live output Button (monospaced `Consolas`) + `ProgressBar`
- Right: split button (action | `SymbolIcon Symbol="More"` flyout)

---

## 9. Context Menus

### BetterMenu Style

Override the default `MenuFlyoutPresenter` to get rounded corners and cleaner borders globally in [App.xaml](file:///c:/Users/DELL/projects/UnigetUI/src/UniGetUI/App.xaml):

```xml
<Style x:Name="BetterContextMenu" BasedOn="{StaticResource DefaultMenuFlyoutPresenterStyle}"
       TargetType="MenuFlyoutPresenter">
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Margin" Value="0" />
    <Setter Property="BorderBrush" Value="{ThemeResource DividerStrokeColorDefaultBrush}" />
</Style>

<Style x:Key="BetterMenuItem" BasedOn="{StaticResource DefaultMenuFlyoutItemStyle}"
       TargetType="MenuFlyoutItem">
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="VerticalContentAlignment" Value="Center" />
    <Setter Property="Height" Value="36" />
</Style>
```

Use `MenuFlyoutSeparator` with `Height="5"` for visual grouping.

---

## 10. Custom Icon Font

Use a custom symbol font for app-specific icons instead of mixing multiple icon sets:

```xml
<!-- In App.xaml -->
<FontFamily x:Key="SymbolFont">
    /Assets/Symbols/Font/fonts/MyApp-Symbols.ttf#MyApp-Symbols
</FontFamily>
```

Usage:

```xml
<TextBlock FontFamily="{StaticResource SymbolFont}" Text="&#xF6FA;" FontSize="24" FontWeight="ExtraLight" />
```

**Icon sizing conventions:**
| Context | FontSize | FontWeight |
|---|---|---|
| Nav item glyph | 20–24 | Normal |
| Page header icon | 50 | Normal |
| List row icon | 24 | ExtraLight |
| Grid card icon | 48 | ExtraLight |
| Overlay badge icon | 30–40 | ExtraLight |

---

## 11. Typography Conventions

Use the **Segoe UI Variable** family throughout:

| Element            | FontFamily                  | FontSize | FontWeight               |
| ------------------ | --------------------------- | -------- | ------------------------ |
| Page title         | `Segoe UI Variable Display` | 30       | Bold                     |
| Splash/brand title | `Segoe UI Variable Display` | 90       | ExtraBlack               |
| Section heading    | `Segoe UI Variable Text`    | 14–16    | SemiBold                 |
| Nav item label     | default                     | 16       | SemiBold                 |
| Body / row text    | `Segoe UI Variable Text`    | 13       | Normal                   |
| Secondary/detail   | `Segoe UI Variable Text`    | 11       | Normal / Opacity 0.5–0.8 |
| Monospaced output  | `Consolas`                  | 12       | Normal                   |
| Caption / hint     | default                     | 12       | Normal                   |

---

## 12. Filtering Sidebar (SplitView + PropertySizer)

### Overall Structure

The filtering sidebar uses an **inline** `SplitView` (not overlay — it pushes content) on the left. A `PropertySizer` handle lets the user resize the pane width live. The pane contains a `ScrollViewer` wrapping a vertical stack of `Expander` groups.

```xml
<SplitView
    Name="FilteringPanel"
    BorderThickness="0"
    DisplayMode="Inline"
    PaneBackground="Transparent"
    PanePlacement="Left"
    PaneClosing="FilteringPanel_PaneClosing"
    SizeChanged="FilteringPanel_SizeChanged">

    <SplitView.Pane>
        <ScrollViewer Name="SidePanel"
                      BorderBrush="{ThemeResource AccentAAFillColorDefaultBrush}"
                      CornerRadius="0,8,8,0"
                      HorizontalScrollMode="Disabled"
                      SizeChanged="SidepanelWidth_SizeChanged">
            <Grid Name="SidePanelGrid" RowSpacing="8">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />    <!-- Sources expander -->
                    <RowDefinition Height="Auto" />    <!-- Filter options expander -->
                    <RowDefinition Height="Auto" />    <!-- Search mode expander -->
                </Grid.RowDefinitions>

                <!-- ① Sources expander -- see below -->
                <!-- ② Filter Options expander -- see below -->
                <!-- ③ Search Mode expander -- see below -->
            </Grid>
        </ScrollViewer>
    </SplitView.Pane>

    <SplitView.Content>
        <Grid Name="PackagesListGrid">
            <!-- Loading progress bar at the very top of the content area -->
            <ProgressBar Name="LoadingProgressBar"
                         Margin="1,-6,1,0" HorizontalAlignment="Stretch" VerticalAlignment="Top"
                         IsIndeterminate="True" />

            <!-- The rounded package list card (see section 7) -->
            <!-- ... -->

            <!-- PropertySizer — drag handle overlaid on the left edge of content -->
            <Toolkit:PropertySizer
                x:Name="FiltersResizer"
                Width="12" Margin="-12,0,0,0" Padding="0"
                HorizontalAlignment="Left"
                Binding="{x:Bind FilteringPanel.OpenPaneLength, Mode=TwoWay}"
                CornerRadius="2"
                Visibility="{x:Bind FilteringPanel.IsPaneOpen, Mode=OneWay}">
                <Toolkit:PropertySizer.RenderTransform>
                    <TranslateTransform X="0" />
                </Toolkit:PropertySizer.RenderTransform>
            </Toolkit:PropertySizer>
        </Grid>
    </SplitView.Content>
</SplitView>
```

> **`PropertySizer` vs `GridSplitter`:** `PropertySizer` directly binds to `SplitView.OpenPaneLength` so the pane width is the single source of truth. No manual event handlers needed.

---

### Toggle Button

Place the toggle button in the toolbar row above the `SplitView`. Override the checked-state background so it matches control fill rather than accent:

```xml
<ToggleButton x:Name="ToggleFiltersButton"
              Height="36" Margin="1,4" Padding="8,4"
              Background="Transparent" BorderThickness="0"
              CornerRadius="4"
              Foreground="{ThemeResource TextFillColorPrimaryBrush}"
              Click="ToggleFiltersButton_Click">
    <ToggleButton.Resources>
        <StaticResource x:Key="ToggleButtonBackgroundChecked"        ResourceKey="ControlFillColorDefaultBrush" />
        <StaticResource x:Key="ToggleButtonBackgroundCheckedPointerOver" ResourceKey="ControlFillColorSecondaryBrush" />
        <StaticResource x:Key="ToggleButtonBackgroundCheckedPressed" ResourceKey="ControlFillColorTertiaryBrush" />
    </ToggleButton.Resources>
    <StackPanel Orientation="Horizontal" Spacing="8">
        <FontIcon FontSize="20" Glyph="&#xE71C;"
                  Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
        <TextBlock VerticalAlignment="Center" FontSize="12" FontWeight="Medium"
                   Foreground="{ThemeResource TextFillColorPrimaryBrush}"
                   Text="Filters" />
    </StackPanel>
</ToggleButton>
```

---

### Expander ① — Sources (TreeView with bulk-select)

`IsExpanded="True"` by default. Contains a multi-select `TreeView` with "Select all" / "Clear selection" hyperlink buttons above it.

```xml
<Expander Grid.Row="0"
          Padding="4,4,4,8"
          HorizontalAlignment="Stretch" HorizontalContentAlignment="Stretch"
          AutomationProperties.Name="Filter by sources"
          Background="{ThemeResource SystemFillColorNeutralBackgroundBrush}"
          CornerRadius="8"
          IsExpanded="True">
    <Expander.Header>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon FontSize="20" Glyph="&#xE74C;" />
            <TextBlock VerticalAlignment="Center" FontWeight="SemiBold" Text="Sources" />
        </StackPanel>
    </Expander.Header>
    <Expander.Content>
        <StackPanel Orientation="Vertical">
            <!-- Placeholder shown before any search has run -->
            <TextBlock x:Name="SourcesPlaceholderText"
                       MinHeight="60" HorizontalAlignment="Center" VerticalAlignment="Center"
                       FontSize="15" FontWeight="Bold" Opacity="0.5"
                       Text="Search for packages to start" TextWrapping="Wrap" />

            <!-- Actual source list (Collapsed until packages load) -->
            <Grid Name="SourcesTreeViewGrid" HorizontalAlignment="Stretch"
                  ColumnSpacing="4" RowSpacing="4" Visibility="Collapsed">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="*" />
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto" />
                    <RowDefinition Height="Auto" />
                </Grid.RowDefinitions>

                <!-- Bulk-action hyperlinks -->
                <HyperlinkButton Name="SelectAllSourcesButton" Grid.Column="0"
                                 Padding="2" HorizontalAlignment="Stretch"
                                 HorizontalContentAlignment="Center"
                                 Click="SelectAllSourcesButton_Click">
                    <TextBlock FontSize="12" FontWeight="SemiBold" Text="Select all" />
                </HyperlinkButton>
                <HyperlinkButton Name="ClearSourceSelectionButton" Grid.Column="1"
                                 Padding="2" HorizontalAlignment="Stretch"
                                 HorizontalContentAlignment="Center"
                                 Click="ClearSourceSelectionButton_Click">
                    <TextBlock FontSize="12" FontWeight="SemiBold" Text="Clear selection" />
                </HyperlinkButton>

                <!-- Multi-select TreeView spanning both columns -->
                <TreeView Name="SourcesTreeView"
                          Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="2"
                          Padding="0" HorizontalAlignment="Stretch"
                          SelectionChanged="SourcesTreeView_SelectionChanged"
                          SelectionMode="Multiple" />
            </Grid>
        </StackPanel>
    </Expander.Content>
</Expander>
```

---

### Expander ② — Filter Options (Checkboxes)

`IsExpanded="False"` by default (power-user options). Contains simple `CheckBox` controls for instant-search, case-sensitivity, and special-character handling:

```xml
<Expander Grid.Row="1"
          Padding="16,8,16,8"
          HorizontalAlignment="Stretch" HorizontalContentAlignment="Stretch"
          AutomationProperties.Name="Filter options"
          Background="{ThemeResource SystemFillColorNeutralBackgroundBrush}"
          CornerRadius="8"
          IsExpanded="False">
    <Expander.Header>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon FontSize="20" Glyph="&#xE71C;" />
            <TextBlock VerticalAlignment="Center" FontWeight="SemiBold" Text="Filters" />
        </StackPanel>
    </Expander.Header>
    <Expander.Content>
        <StackPanel HorizontalAlignment="Stretch" Orientation="Vertical" Spacing="0">
            <CheckBox x:Name="InstantSearchCheckbox"
                      Checked="InstantSearchValueChanged" Unchecked="InstantSearchValueChanged">
                <TextBlock Text="Instant search" TextWrapping="Wrap" />
            </CheckBox>
            <CheckBox x:Name="UpperLowerCaseCheckbox"
                      Checked="FilterOptionsChanged" Unchecked="FilterOptionsChanged">
                <TextBlock Text="Distinguish between uppercase and lowercase" TextWrapping="Wrap" />
            </CheckBox>
            <CheckBox x:Name="IgnoreSpecialCharsCheckbox"
                      IsChecked="True"
                      Checked="FilterOptionsChanged" Unchecked="FilterOptionsChanged">
                <TextBlock Text="Ignore special characters" TextWrapping="Wrap" />
            </CheckBox>
        </StackPanel>
    </Expander.Content>
</Expander>
```

---

### Expander ③ — Search Mode (RadioButtons)

`IsExpanded="True"` by default. Uses `RadioButtons` (not raw `RadioButton` elements) so WinUI handles grouping and keyboard navigation automatically. Each item is `Height="30"` with tight negative margins to reduce whitespace:

```xml
<Expander Grid.Row="2"
          Padding="16,8,16,8"
          HorizontalAlignment="Stretch" HorizontalContentAlignment="Stretch"
          AutomationProperties.Name="Search mode"
          Background="{ThemeResource SystemFillColorNeutralBackgroundBrush}"
          CornerRadius="8"
          IsExpanded="True">
    <Expander.Header>
        <StackPanel Orientation="Horizontal" Spacing="8">
            <FontIcon FontSize="20" Glyph="&#xE773;" />
            <TextBlock VerticalAlignment="Center" FontWeight="SemiBold" Text="Search mode" />
        </StackPanel>
    </Expander.Header>
    <Expander.Content>
        <RadioButtons Name="QueryOptionsGroup"
                      Margin="0,-4,0,6" CharacterSpacing="0"
                      SelectionChanged="FilterOptionsChanged">
            <RadioButton x:Name="QueryNameRadio"  Height="30" Margin="0,-2,0,-2">
                <TextBlock Text="Package Name" TextWrapping="Wrap" />
            </RadioButton>
            <RadioButton x:Name="QueryIdRadio"    Height="30" Margin="0,-2,0,-2">
                <TextBlock Text="Package ID" TextWrapping="Wrap" />
            </RadioButton>
            <RadioButton x:Name="QueryBothRadio"  Height="30" Margin="0,-2,0,-2" IsChecked="True">
                <TextBlock Text="Both" TextWrapping="Wrap" />
            </RadioButton>
            <RadioButton x:Name="QueryExactMatch" Height="30" Margin="0,-2,0,-2">
                <TextBlock Text="Exact match" TextWrapping="Wrap" />
            </RadioButton>
            <RadioButton x:Name="QuerySimilar"    Height="30" Margin="0,-2,0,-2">
                <TextBlock Text="Show similar packages" TextWrapping="Wrap" />
            </RadioButton>
        </RadioButtons>
    </Expander.Content>
</Expander>
```

> **Tip:** Use `RadioButtons` (plural) instead of individual `RadioButton` elements — it groups them automatically, handles keyboard arrow-key navigation, and sets the correct `AutomationProperties` without extra code.

---

### MegaQueryBlock (Full-Screen Search Entry)

When no search has been run yet, show a large full-screen search box centred in the content area. Hide it after the first query.

```xml
<Grid x:Name="MegaQueryBlockGrid"
      HorizontalAlignment="Stretch" VerticalAlignment="Stretch"
      Visibility="Visible">   <!-- Collapse after first search -->
    <Grid.RowDefinitions>
        <RowDefinition Height="*" />
        <RowDefinition Height="80" />    <!-- search row -->
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="8*" MaxWidth="800" />   <!-- text box -->
        <ColumnDefinition Width="80" />                <!-- search button -->
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <!-- 40px SemiBold text box for discoverability -->
    <TextBox x:Name="MegaQueryBlock"
             Grid.Row="1" Grid.Column="1"
             Padding="20,11,10,11"
             CornerRadius="8,0,0,8"
             FontSize="40" FontWeight="SemiBold" />

    <!-- 80×80 animated search button -->
    <Button x:Name="MegaFindButton"
            Grid.Row="1" Grid.Column="2"
            Width="80" Height="80" Padding="12"
            CornerRadius="0,8,8,0"
            AutomationProperties.HelpText="Search">
        <AnimatedIcon>
            <AnimatedIcon.Source><animatedvisuals:AnimatedFindVisualSource /></AnimatedIcon.Source>
            <AnimatedIcon.FallbackIconSource><SymbolIconSource Symbol="Find" /></AnimatedIcon.FallbackIconSource>
        </AnimatedIcon>
    </Button>
</Grid>
```

**Design decisions:**

- `FontSize="40"` for the `TextBox` — unmistakably the primary action
- `CornerRadius="8,0,0,8"` on the box + `"0,8,8,0"` on the button = pill pair
- Button is `80×80` (same height as the box) for a large easy click target
- Collapse (`Visibility="Collapsed"`) after the first search so the results show instead

---

### Filter Sidebar Design Rules

| Rule                              | Value                                      |
| --------------------------------- | ------------------------------------------ |
| Expander background               | `SystemFillColorNeutralBackgroundBrush`    |
| Expander corner radius            | `8`                                        |
| Expander padding                  | `4,4,4,8` (Sources), `16,8,16,8` (others)  |
| Pane `ScrollViewer` corner radius | `0,8,8,0` (right side rounded only)        |
| Pane border accent                | `AccentAAFillColorDefaultBrush`            |
| Default expanded                  | Sources ✓, Search mode ✓, Filter options ✗ |
| Resize handle width               | `12px`, `CornerRadius="2"`                 |
| Sources placeholder min height    | `60px`                                     |

---

## 13. Toolbar Pattern

Use a **split button** for the primary action, then `CommandBar` for secondary actions:

```xml
<StackPanel Orientation="Horizontal">
    <!-- Primary action split button -->
    <Button x:Name="MainToolbarButton" Height="36" BorderThickness="0" CornerRadius="4,0,0,4">
        <StackPanel Orientation="Horizontal" Spacing="4">
            <Image x:Name="MainToolbarButtonIcon" VerticalAlignment="Center" />
            <TextBlock x:Name="MainToolbarButtonText" VerticalAlignment="Center"
                       FontSize="12" FontWeight="SemiBold" />
        </StackPanel>
    </Button>
    <Button x:Name="MainToolbarButtonDropdown" Width="30" Height="36" Padding="4"
            BorderThickness="0" CornerRadius="0,4,4,0">
        <FontIcon FontSize="14" Glyph="&#xE70D;" />
    </Button>

    <!-- Secondary actions -->
    <CommandBar Name="ToolBar" HorizontalAlignment="Left" DefaultLabelPosition="Right" />
</StackPanel>
```

---

## 14. Accessibility

- **Always** set `AutomationProperties.Name` on [ItemContainer](file:///c:/Users/DELL/projects/UnigetUI/src/UniGetUI/Controls/PackageWrapper.cs#93-95), interactive controls, and icon-only buttons.
- Use `AutomationProperties.HelpText` for extra context (e.g., `"Reload packages"`).
- Set `IsTabStop="False"` on purely decorator elements.
- Set `AutomationProperties.AccessibilityView="Raw"` on icon glyphs and overlays to hide from screen readers.
- Provide `ToolTipService.ToolTip` on truncated text and icon-only buttons.
- Add keyboard accelerators using `KeyboardAcceleratorPlacementMode="Hidden"` if the shortcut is shown elsewhere.

---

## 15. AppBarButton Customisation

The global `AppBarButton` style should support `LabelOnRight` and `Compact` states properly. Override in [App.xaml](file:///c:/Users/DELL/projects/UnigetUI/src/UniGetUI/App.xaml) to add a `BrushTransition` for smooth hover effects:

```xml
<Border.BackgroundTransition>
    <BrushTransition Duration="0:0:0.083" />
</Border.BackgroundTransition>
```

---

## Quick Reference: Key NuGet Packages

| Package                             | Usage                                                           |
| ----------------------------------- | --------------------------------------------------------------- |
| `Microsoft.WindowsAppSDK`           | Core WinUI 3 runtime                                            |
| `CommunityToolkit.WinUI.Controls`   | `SettingsCard`, `SettingsExpander`, `GridSplitter`, `Segmented` |
| `CommunityToolkit.WinUI.Animations` | Implicit + explicit animations                                  |

---

_Guide generated from review of UnigetUI (marticliment/UnigetUI) — a production WinUI 3 / .NET 9 package manager._
