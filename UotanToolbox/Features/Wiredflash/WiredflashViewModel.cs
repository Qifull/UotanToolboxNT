﻿using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using UotanToolbox.Common;

namespace UotanToolbox.Features.Wiredflash;

public partial class WiredflashViewModel : MainPageBase
{
    private static string GetTranslation(string key) => FeaturesHelper.GetTranslation(key);
    public WiredflashViewModel() : base(GetTranslation("Sidebar_WiredFlash"), MaterialIconKind.WrenchCogOutline, -600)
    {
    }
}