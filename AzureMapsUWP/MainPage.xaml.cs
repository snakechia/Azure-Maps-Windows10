using AzureMapsLite;
using AzureMapsUWP.Helpers;
using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AzureMapsUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // TODO Replace the placeholder string below with your own Bing Maps key from https://www.bingmapsportal.com
        const string BingMapServiceToken = "<Your Bing Map Key>";

        // TODO Replace the placeholder string below with your own Azure Maps Subscription key from https://portal.azure.com
        const string AzureMapsSubscriptionKey = "<Your Azure Maps Subscription Key>";

        public static AzureMaps azureMaps;

        MapTileSource basicMapTile;
        MapTileSource labelsMapTile;
        MapTileSource satelliteMapTile;
        MapTileSource trafficTile;
        MapTileSource trafficIncidentsTile;

        DateTime travelTime = DateTime.Now;
        TravelMode travelMode = TravelMode.car;
        RouteType routeType = RouteType.fastest;

        /// <summary>
        /// Gets or sets the route waypoints locations. 
        /// </summary>
        public ObservableCollection<LocationData> RouteWaypoints { get; private set; }

        /// <summary>
        /// Gets or sets the pinned locations. 
        /// </summary>
        public ObservableCollection<LocationData> Locations { get; private set; }

        /// <summary>
        /// Gets or sets the locations represented on the map; this is a superset of Locations, and 
        /// includes the current location and any locations being added but not yet saved. 
        /// </summary>
        public ObservableCollection<LocationData> MappedLocations { get; set; }

        /// <summary>
        /// Gets or sets the nearby poi locations
        /// </summary>
        public ObservableCollection<LocationData> NearbyLocations { get; set; }


        private object selectedLocation;
        /// <summary>
        /// Gets or sets the LocationData object corresponding to the current selection in the locations list. 
        /// </summary>
        public object SelectedLocation
        {
            get { return selectedLocation; }
            set
            {
                if (selectedLocation != value)
                {
                    var oldValue = selectedLocation as LocationData;
                    var newValue = value as LocationData;
                    if (oldValue != null)
                    {
                        oldValue.IsSelected = false;
                    }
                    if (newValue != null)
                    {
                        newValue.IsSelected = true;
                    }

                    selectedLocation = newValue;
                }
            }
        }


        public ObservableCollection<RouteData> RouteDatas { get; set; }

        private object selectedRouteData;
        public object SelectedRouteData
        {
            get { return selectedRouteData; }
            set
            {
                if (selectedRouteData != value)
                {
                    var oldValue = selectedRouteData as RouteData;
                    var newValue = value as RouteData;
                    if (oldValue != null)
                    {
                        oldValue.IsSelected = false;
                        oldValue.IsShowed = false;
                    }
                    if (newValue != null)
                    {
                        newValue.IsSelected = true;
                        newValue.IsShowed = false;
                    }

                    selectedRouteData = newValue;
                    PlotRouteDirectionsAsyc(newValue);
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            MyMap.MapServiceToken = BingMapServiceToken;
            azureMaps = new AzureMaps(AzureMapsSubscriptionKey);

            NearbyLocations = new ObservableCollection<LocationData>();
            RouteWaypoints = new ObservableCollection<LocationData>();
            Locations = new ObservableCollection<LocationData>();
            MappedLocations = new ObservableCollection<LocationData>(Locations);

            RouteDatas = new ObservableCollection<RouteData>();

            // MappedLocations is a superset of Locations, so any changes in Locations
            // need to be reflected in MappedLocations. 
            Locations.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null) foreach (LocationData item in e.NewItems) MappedLocations.Add(item);
                if (e.OldItems != null) foreach (LocationData item in e.OldItems) MappedLocations.Remove(item);

                if (Locations.Count > 0)
                {
                    PinnedLocationExpander.Visibility = Visibility.Visible;
                }
                else
                {
                    PinnedLocationExpander.Visibility = Visibility.Collapsed;
                }
            };

            // GoButton to be enable only when RouteWaypoints contain two or more waypoints.
            // Rearrange the waypoint will trigger this block of code and  will perform 
            // GetDirections automatically.
            RouteWaypoints.CollectionChanged += (s, e) =>
            {
                if (RouteWaypoints.Count >= 2)
                {
                    GetRouteDirections();
                    GoButton.IsEnabled = true;
                }
                else
                {
                    GoButton.IsEnabled = false;
                    RouteDatas.Clear();
                }
            };

            /// Search nearby POIs by selected category, populate NearbyLocations list and together
            /// update Locations list so the pin will shown on map.
            NearbyLocations.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null) foreach (LocationData item in e.NewItems) Locations.Add(item);
                if (e.OldItems != null) foreach (LocationData item in e.OldItems) Locations.Remove(item);

                if (NearbyLocations.Count > 0)
                {
                    PinnedLocationExpander.Visibility = Visibility.Collapsed;
                    NearbyLocationExpander.Visibility = Visibility.Visible;
                }
                else
                    NearbyLocationExpander.Visibility = Visibility.Collapsed;
            };

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                basicMapTile = new MapTileSource(new AzureMapTile(AzureMapsSubscriptionKey).TileSource);
                labelsMapTile = new MapTileSource(new AzureMapTile(AzureMapsSubscriptionKey, AzureMapsLite.MapTileLayer.labels).TileSource);
                satelliteMapTile = new MapTileSource(new AzureMapImageTile(AzureMapsSubscriptionKey).TileSource);
                trafficTile = new MapTileSource(new AzureMapTrafficTile(AzureMapsSubscriptionKey).TileSource);
                trafficIncidentsTile = new MapTileSource(new AzureMapIncidentTile(AzureMapsSubscriptionKey, TrafficIncidentDetailStyle.s1).TileSource);

                MyMap.TileSources.Insert(0, basicMapTile);

                // Start handling Geolocator and network status changes after loading the data 
                // so that the view doesn't get refreshed before there is something to show.
                LocationHelper.Geolocator.StatusChanged += Geolocator_StatusChanged;
                NetworkHelper.Instance.NetworkChanged += Instance_NetworkChanged;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            NetworkHelper.Instance.NetworkChanged -= Instance_NetworkChanged;
            LocationHelper.Geolocator.StatusChanged -= Geolocator_StatusChanged;
        }

        /// <summary>
        /// Handles the Geolocator.StatusChanged event to refresh the map and locations list 
        /// if the Geolocator is available, and to display an error message otherwise.
        /// </summary>
        private async void Geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await GeneralHelpers.CallOnUiThreadAsync(async () =>
            {
                switch (args.Status)
                {
                    case PositionStatus.Ready:
                        UpdateLocationStatus(true);
                        await ResetViewAsync();
                        break;
                    case PositionStatus.Initializing:
                        break;
                    
                    case PositionStatus.NoData:
                    case PositionStatus.Disabled:
                    case PositionStatus.NotInitialized:
                    case PositionStatus.NotAvailable:
                    default:
                        UpdateLocationStatus(false);
                        await ResetViewAsync(false);
                        break;
                }
            });
        }

        /// <summary>
        /// Shows or hides the error message relating to the Geolocator status, depending on the specified value.
        /// </summary>
        /// <param name="isLocationAvailable">true if the Geolocator is available; otherwise, false.</param>
        private void UpdateLocationStatus(bool isLocationAvailable)
        {
            if (isLocationAvailable)
            {
                NoGeolocatorAccess.Dismiss();
                centerMeButton.IsEnabled = true;
            }
            else
            {
                NoGeolocatorAccess.Show(0);
                centerMeButton.IsEnabled = false;
            }
        }

        // Display No Network Notification
        private async void Instance_NetworkChanged(object sender, EventArgs e)
        {
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NoInternetWarningNotification.Show(0);
                });
            else
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NoInternetWarningNotification.Dismiss();
                });
        }

        /// <summary>
        /// Updates the UI to account for the user's current position, if available, 
        /// resetting the MapControl bounds and refreshing the travel info. 
        /// </summary>
        /// <param name="isGeolocatorReady">false if the Geolocator is known to be unavailable; otherwise, true.</param>
        /// <returns></returns>
        private async Task ResetViewAsync(bool isGeolocatorReady = true)
        {
            LocationData currentLocation = null;

            if (isGeolocatorReady) currentLocation = await GetCurrentLocationAsync();

            if (currentLocation != null)
            {
                if (MappedLocations.Count > 0 && MappedLocations[0].IsCurrentLocation)
                {
                    MappedLocations.RemoveAt(0);
                }

                await LocationHelper.TryUpdateMissingLocationInfoAsync(currentLocation, "Current Location");

                MappedLocations.Insert(0, new LocationData { Name = "Current Location", Position = currentLocation.Position, IsCurrentLocation = true });
                RouteWaypoints.Insert(0, currentLocation);
            }

            // Set the current view of the map control. 
            var positions = Locations.Select(loc => loc.Position).ToList();
            if (currentLocation != null) positions.Insert(0, currentLocation.Position);

            if (positions.Count > 0)
            {
                bool isSuccessful = await SetViewBoundsAsync(positions);

                if (isSuccessful && positions.Count < 2) MyMap.ZoomLevel = 15;
                else if (!isSuccessful && positions.Count > 0)
                {
                    MyMap.Center = new Geopoint(positions[0]);
                    MyMap.ZoomLevel = 15;
                }
            }
        }

        private async Task<bool> SetViewBoundsAsync(List<BasicGeoposition> geopositions)
        {
            var bounds = GeoboundingBox.TryCompute(geopositions);
            double viewWidth = ApplicationView.GetForCurrentView().VisibleBounds.Width;
            var margin = new Thickness((viewWidth >= 500 ? 300 : 10), 10, 10, 10);
            return await MyMap.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Default);
        }

        /// <summary>
        /// Gets the current location if the geolocator is available, 
        /// and updates the Geolocator status message depending on the results.
        /// </summary>
        /// <returns>The current location.</returns>
        private async Task<LocationData> GetCurrentLocationAsync()
        {
            var currentLocation = await LocationHelper.GetCurrentLocationAsync();
            UpdateLocationStatus(currentLocation != null);

            return currentLocation;
        }

        /// <summary>
        /// Handles the click event of the CenterMe Button to acquire user current location,
        /// and center the map to user current location.
        /// </summary>
        private async void CenterMeButton_Click(object sender, RoutedEventArgs e)
        {
            await ResetViewAsync(false);
        }

        /// <summary>
        /// Handles the Holding event of the MapControl to add a new location
        /// to the Locations list, using the position indicated by the gesture.
        /// </summary>
        private async void MyMap_MapHolding(MapControl sender, MapInputEventArgs args)
        {
            var location = new LocationData { Position = args.Location.Position };

            // Resolve the address given the geocoordinates. In this case, because the 
            // location is unambiguous, there is no need to pass in the current location.
            await LocationHelper.TryUpdateMissingLocationInfoAsync(location, null);

            EditNewLocation(location);
        }

        /// <summary>
        /// Handles the Right Click event of the MapControl to add a new location
        /// to the Locations list, using the position indicated by the gesture.
        /// </summary>
        private async void MyMap_MapRightTapped(MapControl sender, MapRightTappedEventArgs args)
        {
            var location = new LocationData { Position = args.Location.Position };

            // Resolve the address given the geocoordinates. In this case, because the 
            // location is unambiguous, there is no need to pass in the current location.
            await LocationHelper.TryUpdateMissingLocationInfoAsync(location, null);

            EditNewLocation(location);
        }

        /// <summary>
        /// Adds the specified location to the Locations list and shows the editor flyout.
        /// </summary>
        public void EditNewLocation(LocationData location)
        {
            Locations.Add(location);
            PinnedLocationListView.UpdateLayout();
            //EditLocation(location);
        }

        /// <summary>
        /// Gets the data context of the specified element as a LocationData instance.
        /// </summary>
        /// <param name="element">The element bound to the location.</param>
        /// <returns>The location bound to the element.</returns>
        private LocationData GetLocation(FrameworkElement element) =>
            (element.FindName("Presenter") as FrameworkElement).DataContext as LocationData;

        /// <summary>
        /// Handles delete button clicks to remove the selected
        /// location from the Locations collection. 
        /// </summary>
        private void DeleteLocation_Click(object sender, RoutedEventArgs e)
        {
            var location = GetLocation(sender as Button);
            Locations.Remove(location);
        }

        /// <summary>
        /// Handles clicks to the Show Location button and center the display 
        /// the pinned location to the map. 
        /// </summary>
        private async void ShowLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var location = GetLocation(sender as Button);
            await MyMap.TrySetViewAsync(new Geopoint(location.Position), null, null, null, MapAnimationKind.Linear);
        }

        /// <summary>
        /// Handles clicks to the Show Route button and display 
        /// the route to the selected location from the user's current position. 
        /// </summary>
        private void ShowRouteButton_Click(object sender, RoutedEventArgs e)
        {
            var location = GetLocation(sender as Button);

            RouteWaypoints.Clear();
            RouteWaypoints.Insert(0, MappedLocations[0]);
            RouteWaypoints.Add(location);

            RouteDirectionSplitView.IsPaneOpen = true;
        }

        /// <summary>
        /// Handles clicks to the RouteDirectionGrid button and  
        /// Show/Hide RouteDirectionGrid based on the current status. 
        /// </summary>
        private void directionBtn_Click(object sender, RoutedEventArgs e)
        {
            RouteDirectionSplitView.IsPaneOpen = !RouteDirectionSplitView.IsPaneOpen;
        }

        /// <summary>
        /// Handles clicks to the Close RouteDirection Grid button and 
        /// collapse RouteDirectionGrid. 
        /// </summary>
        private void CloseRouteDirectionGrid_Click(object sender, RoutedEventArgs e)
        {
            TravelTimeOptionGrid.Visibility = Visibility.Collapsed;
            leaveNowButton.IsChecked = true;
            leaveatButton.IsChecked = false;
            arrivebyButton.IsChecked = false;

            timePicker.Time = DateTime.Now.TimeOfDay;
            timePicker.SelectedTime = DateTime.Now.TimeOfDay;

            if (calendarView.SelectedDates.Count > 0)
            {
                calendarView.SelectedDates.Clear();
                calendarView.SelectedDates.Add(DateTime.Now);
            }

            RouteDirectionSplitView.IsPaneOpen = false;
            
            if (RouteDatas.Count > 0)
                RouteDatas.Clear();
        }

        /// <summary>
        /// Handles clicks to the Travel Mode button and 
        /// update selected travel mode. 
        /// </summary>
        private void TravelMode_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;

            if (RouteDatas.Count > 0)
                RouteDatas.Clear();

            driveButton.IsChecked = false;
            busButton.IsChecked = false;
            walkButton.IsChecked = false;

            string selectedMode = btn.Name.ToString();

            switch (selectedMode)
            {
                case "driveButton":
                    travelMode = TravelMode.car;
                    driveButton.IsChecked = true;
                    break;

                case "busButton":
                    travelMode = TravelMode.bus;
                    busButton.IsChecked = true;
                    break;

                case "walkButton":
                    travelMode = TravelMode.pedestrian;
                    walkButton.IsChecked = true;
                    break;

                default:
                    travelMode = TravelMode.car;
                    break;
            }

            GetRouteDirections();
        }

        /// <summary>
        /// Handles clicks to the Show add route waypoing button and 
        /// show add route waypoint grid. 
        /// </summary>
        private void ShowAddRouteWayPointGrid_Click(object sender, RoutedEventArgs e)
        {
            AddRouteWayPointGrid.Visibility = Visibility.Visible;
            AddRouteWayPointButtonGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles clicks to the Close Add Route Waypoint Grid and 
        /// show the Add RouteWayPointButton
        /// </summary>
        private void CloseAddRouteWayPointGrid_Click(object sender, RoutedEventArgs e)
        {
            AddRouteWayPointGrid.Visibility = Visibility.Collapsed;
            AddRouteWayPointButtonGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles clicks to remove waypoint from RouteWaypoints list
        /// </summary>
        private void RemoveWaypointButton_Click(object sender, RoutedEventArgs e)
        {
            var location = ((Button)sender).DataContext as LocationData;
            RouteWaypoints.Remove(location);
        }

        /// <summary>
        /// Handles clicks to set waypoint as departure point
        /// </summary>
        private void SetDepartureWaypoint_Click(object sender, RoutedEventArgs e)
        {
            var location = ((MenuFlyoutItem)sender).DataContext as LocationData;
            
            RouteWaypoints.Insert(0, location);
            RouteDirectionSplitView.IsPaneOpen = true;
        }

        /// <summary>
        /// Handles clicks to set waypoint as destination
        /// </summary>
        private void SetArrivalWaypoint_Click(object sender, RoutedEventArgs e)
        {
            var location = ((MenuFlyoutItem)sender).DataContext as LocationData;
            RouteWaypoints.Add(location);
            RouteDirectionSplitView.IsPaneOpen = true;
        }

        /// <summary>
        /// Handles clicks to open the Main Menu split view panel
        /// </summary>
        private void OpenMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenuSplitView.IsPaneOpen = !MainMenuSplitView.IsPaneOpen;
        }

        #region Main Menu
        /// <summary>
        /// Handles clicks to show basic road or satellite imagery tile
        /// </summary>
        private void ChangeMapTile_Click(object sender, RoutedEventArgs e)
        {
            var btnName = ((ToggleButton)sender).Name;

            BasicMapTileButton.IsChecked = false;
            SatelliteImageTileButton.IsChecked = false;
            MyMap.TileSources.RemoveAt(0);

            if (MyMap.TileSources.Contains(labelsMapTile))
                MyMap.TileSources.Remove(labelsMapTile);

            switch (btnName)
            {
                case "BasicMapTileButton":
                    MyMap.TileSources.Insert(0, basicMapTile);
                    BasicMapTileButton.IsChecked = true;

                    break;

                case "SatelliteImageTileButton":
                    MyMap.TileSources.Insert(0, satelliteMapTile);
                    SatelliteImageTileButton.IsChecked = true;

                    if (showLabelToggleSwitch.IsOn)
                        MyMap.TileSources.Add(labelsMapTile);
                    break;
            }

        }

        /// <summary>
        /// Handles clicks to show current traffic tile
        /// </summary>
        private void TrafficTileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TrafficTileButton.IsChecked.Value)
                MyMap.TileSources.Remove(trafficTile);
            else
                MyMap.TileSources.Insert(1, trafficTile);
        }

        /// <summary>
        /// Handles clicks to show current traffic incidents tile
        /// </summary>
        private void TrafficIncidentTileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TrafficIncidentTileButton.IsChecked.Value)
                MyMap.TileSources.Remove(trafficIncidentsTile);
            else
                MyMap.TileSources.Insert(1, trafficIncidentsTile);
        }

        /// <summary>
        /// Handles clicks to show label tile, it use inconjunction with Satellite imagery tile
        /// </summary>
        private void showLabelToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (showLabelToggleSwitch.IsOn)
                MyMap.TileSources.Add(labelsMapTile);
            else
                MyMap.TileSources.Remove(labelsMapTile);

        }
        #endregion

        #region AutoSuggestion Search Box 
        private async void searchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var autoSuggestBox = (AutoSuggestBox)sender;
            
            if (String.IsNullOrWhiteSpace(autoSuggestBox.Text) || (autoSuggestBox.Text.Trim().Length <= 4))
                return;

            var language = (LanguageComboBox.SelectedItem as ComboBoxItem).Tag.ToString();

            var results = await LocationHelper.FuzzySearchAsync(autoSuggestBox.Text.Trim(), language);

            if (autoSuggestBox.Tag.ToString() == "main")
                mainSearchBox.ItemsSource = results;
            else
                WayPointSearch.ItemsSource = results;
        }

        private void searchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string tag = ((AutoSuggestBox)sender).Tag.ToString();

            var selected = args.SelectedItem as Result;

            LocationData location = new LocationData() {  Position = new BasicGeoposition() { Latitude = selected.position.lat, Longitude = selected.position.lon, Altitude = 50 } };

            location.Name = LocationHelper.ConstructStringFromAddress(selected.address, "name", null);
            location.Address = LocationHelper.ConstructStringFromAddress(selected.address, "address", null);
            location.FormattedAddress = LocationHelper.ConstructStringFromAddress(selected.address, "full", null);

            if (String.IsNullOrWhiteSpace(location.Name))
                location.Name = selected.position.lat.ToString("0.000000, ") + selected.position.lon.ToString("0.000000");

            if (selected.poi != null)
                location.Name = selected.poi.name;

            EditNewLocation(location);

            if (tag == "main")
            {
                mainSearchBox.Text = string.Empty;
            }
            else
            {
                RouteWaypoints.Add(location);
                WayPointSearch.Text = string.Empty;
                AddRouteWayPointGrid.Visibility = Visibility.Collapsed;
                AddRouteWayPointButtonGrid.Visibility = Visibility.Visible;
            }
        }
        #endregion

        /// <summary>
        /// Handles clicks to get route directions
        /// </summary>
        private void GoButton_Click(object sender, RoutedEventArgs e)
        {
            TravelTimeOptionGrid.Visibility = Visibility.Collapsed;
            GetRouteDirections();
        }

        /// <summary>
        /// Update Avoids options for routing
        /// </summary>
        private void SetAvoidOptionFlyout_Closed(object sender, object e)
        {
            GetRouteDirections();
        }

        private void Settings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetRouteDirections();
        }

        /// <summary>
        /// Search routes to selected destination/waypoints
        /// </summary>
        private async void GetRouteDirections()
        {
            if (RouteDatas == null)
                return;

            if (RouteDatas.Count > 0)
                RouteDatas.Clear();

            if (RouteWaypoints.Count < 2)
                return;

            List<BasicGeoposition> pts = RouteWaypoints.Select(loc => loc.Position).ToList();

            DateTime dateTime = DateTime.Now.AddMinutes(1);

            if ((leaveatButton.IsChecked.Value) || (arrivebyButton.IsChecked.Value))
            {
                var dts = calendarView.SelectedDates.ToList();
                dateTime = dts[0].DateTime.Date.AddMinutes(timePicker.SelectedTime.Value.TotalMinutes);
            }

            var avoids = GetAvoids();

            var language = (LanguageComboBox.SelectedItem as ComboBoxItem).Tag.ToString();

            RouteDirectionsResponse results = new RouteDirectionsResponse();

            if (leaveatButton.IsChecked.Value || leaveatButton.IsChecked.Value)
                results = await LocationHelper.GetRouteDirections(pts, language, travelMode, avoids, routeType, RouteOptionsCountComboBox.SelectedIndex, dateTime, null);
            else
                results = await LocationHelper.GetRouteDirections(pts, language, travelMode, avoids, routeType, RouteOptionsCountComboBox.SelectedIndex, null, dateTime);

            if (results == null) return;

            foreach (var route in results.routes)
            {
                var routedata = new RouteData() 
                { 
                    Route = route, 
                    IsLeaveNow = leaveNowButton.IsChecked.Value,
                    IsArriveBy = arrivebyButton.IsChecked.Value,
                    IsLeaveAt = leaveatButton.IsChecked.Value,
                    IsShowed = false, 
                    StartLocationLable = RouteWaypoints[0].Name, 
                    EndLocationLabel = RouteWaypoints[RouteWaypoints.Count - 1].Name 
                };

                RouteDatas.Add(routedata);
            }
        }

        /// <summary>
        /// Process Avoids selection
        /// </summary>
        /// <returns>return a list of avoid selection</returns>
        private List<Avoid> GetAvoids()
        {
            List<Avoid> avoids = new List<Avoid>();

            if (borderCrossingsCB.IsChecked == true)
                avoids.Add(Avoid.borderCrossings);

            if (carpoolsCB.IsChecked == true)
                avoids.Add(Avoid.carpools);

            if (ferriesCB.IsChecked == true)
                avoids.Add(Avoid.ferries);

            if (motorwaysCB.IsChecked == true)
                avoids.Add(Avoid.motorways);

            if (tollRoadsCB.IsChecked == true)
                avoids.Add(Avoid.tollRoads);

            if (unpavedRoadsCB.IsChecked == true)
                avoids.Add(Avoid.unpavedRoads);

            return avoids;
        }

        /// <summary>
        /// Handles clicks to show detail of route direction instructions
        /// </summary>
        private void MoreRouteDirectionDetails_Click(object sender, RoutedEventArgs e)
        {
            var routedata = ((Button)sender).DataContext as RouteData;

            routedata.IsShowed = !routedata.IsShowed;
        }

        /// <summary>
        /// Plot polyline on MapControls for selected route directions
        /// </summary>
        private async void PlotRouteDirectionsAsyc(RouteData routeData)
        {
            MyMap.MapElements.Clear();

            if (routeData == null)
                return;

            int legcount = 0;
            List<BasicGeoposition> positions = new List<BasicGeoposition>();

            foreach (var leg in routeData.Route.legs)
            {
                List<BasicGeoposition> pts = new List<BasicGeoposition>();

                foreach (var point in leg.points)
                {
                    BasicGeoposition pt = new BasicGeoposition() { Latitude = point.latitude, Longitude = point.longitude, Altitude = 50 };
                    pts.Add(pt);
                }

                positions.AddRange(pts);
                Color color = Color.FromArgb(255, 0, 88, 161);

                switch (legcount)
                {
                    case 0:
                        color = Color.FromArgb(255, 0, 88, 161);
                        break;

                    case 1:
                        color = Color.FromArgb(255, 30, 116, 189);
                        break;

                    case 2:
                        color = Color.FromArgb(255, 11, 171, 209);
                        break;

                    default:
                        color = Color.FromArgb(255, 11, 171, 209);
                        break;
                }

                legcount++;

                Geopath path = new Geopath(pts);
                MapPolyline polyline = new MapPolyline() { Path = path, StrokeThickness = 5, StrokeColor = color };
                MyMap.MapElements.Add(polyline);
            }

            bool isSuccessful = await SetViewBoundsAsync(positions);
        }

        /// <summary>
        /// Set current time for TimePicker and CalendarView
        /// Set Minimum & Maximum DateTime for CalendarView
        /// </summary>
        private void ChangeTravelTime_Click(object sender, RoutedEventArgs e)
        {
            leaveNowButton.IsChecked = false;
            leaveatButton.IsChecked = false;
            arrivebyButton.IsChecked = false;

            timePicker.Time = DateTime.Now.TimeOfDay;
            timePicker.SelectedTime = DateTime.Now.TimeOfDay;

            calendarView.MinDate = DateTime.Now;
            calendarView.MaxDate = DateTime.Now.AddDays(14);

            if (calendarView.SelectedDates.Count <= 0)
                calendarView.SelectedDates.Add(DateTime.Now);

            string btnName = ((ToggleButton)sender).Name;

            switch (btnName)
            {
                case "leaveNowButton":
                    leaveNowButton.IsChecked = true;
                    datetimeSelectionGrid.Visibility = Visibility.Collapsed;
                    break;

                case "leaveatButton":
                    leaveatButton.IsChecked = true;
                    datetimeSelectionGrid.Visibility = Visibility.Visible;
                    break;

                case "arrivebyButton":
                    arrivebyButton.IsChecked = true;
                    datetimeSelectionGrid.Visibility = Visibility.Visible;
                    break;

                default:
                    break;
            }

        }

        /// <summary>
        /// Handles clicks to show time picker and calendar view
        /// </summary>
        private void SetTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (TravelTimeOptionGrid.Visibility == Visibility.Collapsed)
            {
                TravelTimeOptionGrid.Visibility = Visibility.Visible;
                datetimeSelectionGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                TravelTimeOptionGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles change of time by timepicker
        /// Update the display accordingly by calling UpdateTravelTimeStatusLabel
        /// </summary>
        private void timePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if ((e.OldTime.Hours == e.NewTime.Hours) && (e.OldTime.Minutes == e.NewTime.Minutes))
                return;

            travelTime = DateTime.Now.Date.AddMinutes(timePicker.SelectedTime.Value.TotalMinutes);
            UpdateTravelTimeStatusLabel();
        }

        /// <summary>
        /// Handles change of date by calendarview
        /// Update the display accordingly by calling UpdateTravelTimeStatusLabel
        /// </summary>
        private void calendarView_SelectedDatesChanged(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs args)
        {
            var dts = calendarView.SelectedDates.ToList();
            if (dts.Count == 0)
                return;

            travelTime = dts[0].DateTime.Date.AddMinutes(timePicker.SelectedTime.Value.TotalMinutes);
            UpdateTravelTimeStatusLabel();
        }

        /// <summary>
        /// Update travel time accordingly and call GetRouteDirection
        /// </summary>
        private void UpdateTravelTimeStatusLabel()
        {
            if (leaveNowButton.IsChecked.Value)
                SetTimeStatusTextBox.Text = "Leave now";

            if (leaveatButton.IsChecked.Value)
                SetTimeStatusTextBox.Text = "Leave at " + travelTime.ToShortTimeString() + "\n" + travelTime.ToLongDateString();

            if (arrivebyButton.IsChecked.Value)
                SetTimeStatusTextBox.Text = "Arrive by " + travelTime.ToShortTimeString() + "\n" + travelTime.ToLongDateString();

            GetRouteDirections();
        }

        /// <summary>
        /// Search nearby POIs based on selected category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchByCategory_Click(object sender, RoutedEventArgs e)
        {
            NearbyLocations.Clear();
            PinnedLocationListView.UpdateLayout();

            var selected = (MenuFlyoutItem)sender;
            var selectedlocation = selected.DataContext as LocationData;
            var searchCategory = selected.Text;
            var language = (LanguageComboBox.SelectedItem as ComboBoxItem).Tag.ToString();

            var results = await LocationHelper.SearchPOIsByCategory(searchCategory, selectedlocation.Position, language, selectedlocation.CountryCode);

            foreach (var result in results)
            {
                LocationData location = new LocationData() { Position = new BasicGeoposition() { Latitude = result.position.lat, Longitude = result.position.lon, Altitude = 50 } };

                location.Name = LocationHelper.ConstructStringFromAddress(result.address, "name", null);
                location.Address = LocationHelper.ConstructStringFromAddress(result.address, "address", null);
                location.FormattedAddress = LocationHelper.ConstructStringFromAddress(result.address, "full", null);

                if (String.IsNullOrWhiteSpace(location.Name))
                    location.Name = result.position.lat.ToString("0.000000, ") + result.position.lon.ToString("0.000000");

                if (result.poi != null)
                    location.Name = result.poi.name;

                NearbyLocations.Add(location);
            }
        }

        /// <summary>
        /// Clear all Nearby POIs list, this will clear the same entry at Locations list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearNearbyLocation_Click(object sender, RoutedEventArgs e)
        {
            int count = NearbyLocations.Count;

            for (int i = 0; i < count; i++)
            {
                NearbyLocations.RemoveAt(0);
            }
        }

    }
}
