﻿using Avalonia.Collections;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI;
using SukiUI.Models;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Settings;

public partial class SettingsViewModel : MainPageBase
{
    public IAvaloniaReadOnlyList<SukiColorTheme> AvailableColors { get; }

    public AvaloniaList<string> SimpleContent { get; } = new();

    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    [ObservableProperty] private bool _isBackgroundAnimated;
    [ObservableProperty] private bool _isLightTheme;
    [ObservableProperty] private string _selectedSimpleContent = "";

    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public SettingsViewModel() : base(GetTranslation("Sidebar_Settings"), MaterialIconKind.SettingsOutline, -200)
    {
        AvailableColors = _theme.ColorThemes;
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        IsBackgroundAnimated = _theme.IsBackgroundAnimated;
        _theme.OnBaseThemeChanged += variant =>
            IsLightTheme = variant == ThemeVariant.Light;
        _theme.OnColorThemeChanged += theme =>
        {
            // TODO: Implement a way to make the correct, might need to wrap the thing in a VM, this isn't ideal.
        };
        _theme.OnBackgroundAnimationChanged += value =>
            IsBackgroundAnimated = value;
    }

    partial void OnIsLightThemeChanged(bool value) =>
        _theme.ChangeBaseTheme(value ? ThemeVariant.Light : ThemeVariant.Dark);

    partial void OnIsBackgroundAnimatedChanged(bool value) =>
        _theme.SetBackgroundAnimationsEnabled(value);

    [RelayCommand]
    public void SwitchToColorTheme(SukiColorTheme colorTheme) =>
        _theme.ChangeColorTheme(colorTheme);
}