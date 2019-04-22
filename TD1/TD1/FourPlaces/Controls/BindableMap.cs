using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace TD1.FourPlaces.Controls
{
    public class BindableMap : Map
    {
        public bool _positionToUser = false;
        public bool PositionToUser {
            get => _positionToUser;
            set => _positionToUser = value;
        }

        public BindableMap() : base(MapSpan.FromCenterAndRadius(new Position(0, 0), Distance.FromMiles(1)))
        {
            this.PropertyChanged += (object sender, PropertyChangedEventArgs e) =>
            {
                    BindableMap map = sender as BindableMap;
                    if (map.VisibleRegion != null)
                    {
                        this.MapSpan = (map.VisibleRegion);
                    }
            };
        }

        public static readonly BindableProperty MapPinsProperty = BindableProperty.Create(
                 nameof(Pins),
                 typeof(ObservableCollection<Pin>),
                 typeof(BindableMap),
                 new ObservableCollection<Pin>(),
                 propertyChanged: (b, o, n) =>
                 {
                     var bindable = (BindableMap)b;
                     bindable.Pins.Clear();

                     var collection = (ObservableCollection<Pin>)n;
                     foreach (var item in collection)
                         bindable.Pins.Add(item);
                     collection.CollectionChanged += (sender, e) =>
                     {
                         Device.BeginInvokeOnMainThread(() =>
                         {
                             switch (e.Action)
                             {
                                 case NotifyCollectionChangedAction.Add:
                                 case NotifyCollectionChangedAction.Replace:
                                 case NotifyCollectionChangedAction.Remove:
                                     if (e.OldItems != null)
                                         foreach (var item in e.OldItems)
                                             bindable.Pins.Remove((Pin)item);
                                     if (e.NewItems != null)
                                         foreach (var item in e.NewItems)
                                             bindable.Pins.Add((Pin)item);
                                     break;
                                 case NotifyCollectionChangedAction.Reset:
                                     bindable.Pins.Clear();
                                     break;
                             }
                         });
                     };
                 });
        public IList<Pin> MapPins {
            get { return (IList<Pin>)GetValue(MapPinsProperty); }
            set { SetValue(MapPinsProperty, value); }
        }

        public static readonly BindableProperty MapPositionProperty = BindableProperty.Create(
                 nameof(MapPosition),
                 typeof(Position),
                 typeof(BindableMap),
                 new Position(0, 0),
                 propertyChanged: (b, o, n) =>
                 {
                    ((BindableMap)b).MoveToRegion(MapSpan.FromCenterAndRadius(
                        (Position)n,
                        Distance.FromMiles(1)));
                 });

        public Position MapPosition {
            get { return (Position)GetValue(MapPositionProperty); }
            set { SetValue(MapPositionProperty, value); }
        }

        public static readonly BindableProperty MapSpanProperty = BindableProperty.Create(
                                                         propertyName: "MapSpan",
                                                         returnType: typeof(MapSpan),
                                                         declaringType: typeof(BindableMap),
                                                         defaultValue: null,
                                                         defaultBindingMode: BindingMode.TwoWay,
                                                         validateValue: null);
      
        public MapSpan MapSpan
        {
            get { return (MapSpan)GetValue(MapSpanProperty); }
            set { SetValue(MapSpanProperty, value); }
        }

    }
}
