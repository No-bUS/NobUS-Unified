using System;
using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Service;

public interface ILocationProvider : IDisposable
{
    Coordinate? Location { get; }

    IObservable<Coordinate> LocationChanges { get; }
}
