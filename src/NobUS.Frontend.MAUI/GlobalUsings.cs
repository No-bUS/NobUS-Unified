﻿// Standard libraries
global using System;
global using System.Linq;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;

// MAUI Reactor
global using MauiReactor;

// MAUI
#if ANDROID
global using MauiApplication = Microsoft.Maui.MauiApplication;
global using MauiAppCompatActivity = Microsoft.Maui.MauiAppCompatActivity;
#endif
#if WINDOWS
global using MauiWinUIApplication = Microsoft.Maui.MauiWinUIApplication;
#endif
global using MauiApp = Microsoft.Maui.Hosting.MauiApp;

// MAUI control aliases
global using SwipeItems = Microsoft.Maui.Controls.SwipeItems;
global using SwipeItem = Microsoft.Maui.Controls.SwipeItem;
global using SwipeItemView = Microsoft.Maui.Controls.SwipeItemView;
global using SwipeMode = Microsoft.Maui.SwipeMode;
global using ItemSizingStrategy = Microsoft.Maui.Controls.ItemSizingStrategy;
global using LayoutOptions = Microsoft.Maui.Controls.LayoutOptions;
global using Colors = Microsoft.Maui.Graphics.Colors;
global using Color = Microsoft.Maui.Graphics.Color;
global using ScrollBarVisibility = Microsoft.Maui.ScrollBarVisibility;
global using SelectionMode = Microsoft.Maui.Controls.SelectionMode;
global using ListViewSelectionMode = Microsoft.Maui.Controls.ListViewSelectionMode;
global using SeparatorVisibility = Microsoft.Maui.Controls.SeparatorVisibility;
global using TextDecorations = Microsoft.Maui.TextDecorations;
global using FontAttributes = Microsoft.Maui.Controls.FontAttributes;
global using ClearButtonVisibility = Microsoft.Maui.ClearButtonVisibility;
