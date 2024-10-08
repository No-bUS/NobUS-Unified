﻿using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Service;

internal interface ILocationProvider : IDisposable
{
    public Coordinate Location { get; }
}
