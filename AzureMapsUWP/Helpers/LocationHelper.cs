using AzureMapsLite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace AzureMapsUWP.Helpers
{
    public static class LocationHelper
    {
        private static string CountryCode = string.Empty;
        private static BasicGeoposition CurrentLocation = new BasicGeoposition();

        /// <summary>
        /// Gets the Geolocator singleton used by the LocationHelper.
        /// </summary>
        public static Geolocator Geolocator { get; } = new Geolocator();

        /// <summary>
        /// Gets or sets the CancellationTokenSource used to enable Geolocator.GetGeopositionAsync cancellation.
        /// </summary>
        private static CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Gets the current location if the geolocator is available.
        /// </summary>
        /// <returns>The current location.</returns>
        public static async Task<LocationData> GetCurrentLocationAsync()
        {
            try
            {
                // Request permission to access the user's location.
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:

                        CancellationTokenSource = new CancellationTokenSource();
                        var token = CancellationTokenSource.Token;

                        Geoposition position = await Geolocator.GetGeopositionAsync().AsTask(token);
                        return new LocationData { Position = position.Coordinate.Point.Position };

                    case GeolocationAccessStatus.Denied:
                    case GeolocationAccessStatus.Unspecified:
                    default:
                        return null;
                }
            }
            catch (TaskCanceledException)
            {
                // Do nothing.
            }
            finally
            {
                LocationHelper.CancellationTokenSource = null;
            }
            return null;
        }

        /// <summary>
        /// Cancels any waiting GetGeopositionAsync call.
        /// </summary>
        public static void CancelGetCurrentLocation()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Attempts to update either the address or the coordinates of the specified location 
        /// if the other value is missing, using the specified current location to provide 
        /// context for prioritizing multiple locations returned for an address.  
        /// </summary>
        /// <param name="location">The location to update.</param>
        public static async Task<bool> TryUpdateMissingLocationInfoAsync(LocationData location, string label)
        {
            bool hasNoAddress = String.IsNullOrEmpty(location.Address);
            if (hasNoAddress && location.Position.Latitude == 0 && location.Position.Longitude == 0) return true;

            var rawresult = await MainPage.azureMaps.SearchAddressReverseAsync(location.Position);
            var result = rawresult as SearchAddressReverse;
            Address address = result.addresses[0].address;


            try
            {
                location.Name = ConstructStringFromAddress(address, "name", label);
                location.Address = ConstructStringFromAddress(address, "address", label);
                location.FormattedAddress = ConstructStringFromAddress(address, "full", label);
                location.CountryCode = address.countryCode;

                if (String.IsNullOrWhiteSpace(location.Name))
                    location.Name = location.Position.Latitude.ToString("0.000000, ") + location.Position.Longitude.ToString("0.000000");

                if ((label != null) && (label == "Current Location"))
                {
                    CurrentLocation = location.Position;
                    CountryCode = address.countryCode;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ConstructStringFromAddress(Address address, string returnType, string label)
        {
            string result = "";

            string no = string.Empty;
            string street = string.Empty;
            string municipalitySubdivision = string.Empty;
            string municipality = string.Empty;
            string postalcode = string.Empty;
            string countrySecondarySubDivision = string.Empty;
            string countrySubDivision = string.Empty;
            string country = string.Empty;

            if (address.buildingNumber != null)
                no = address.buildingNumber + " ";

            if (address.streetNumber != null)
                no = address.streetNumber + " ";

            if (address.street != null)
                street = address.street;

            if (address.municipality != null)
                municipality = address.municipality + ", ";

            if (address.municipalitySubdivision != null)
                municipalitySubdivision = address.municipalitySubdivision + ", ";

            if (address.postalCode != null)
                postalcode = address.postalCode + ", ";

            if (address.countrySecondarySubdivision != null)
                countrySecondarySubDivision = address.countrySecondarySubdivision + ", ";

            if (address.countrySubdivision != null)
                countrySubDivision = address.countrySubdivision + ", ";
            
            if (address.country != null)
                country = address.country;

            switch (returnType)
            {
                case "name":
                    if (label != null)
                        result = label;
                    else
                        result = (no + street).Trim();
                    break;

                case "address":
                    result = (municipalitySubdivision + municipality + postalcode + countrySecondarySubDivision + countrySubDivision + country).Trim();
                    break;

                case "full":
                    result = (no + street + ", " + municipalitySubdivision + municipality + postalcode + countrySecondarySubDivision + countrySubDivision + country).Trim();
                    break;

                default:
                    break;
            }

            return result;
        }


        public static async Task<List<Result>> FuzzySearchAsync(string text, string language)
        {

            var rawresult = await MainPage.azureMaps.SearchFuzzyAsync(text, null, null, null, null, null, CurrentLocation, 10000000, null, null, language, null, CountryCode, null);

            var result = rawresult as SearchAddressResponse;

            return result.results;
        }

        public static async Task<RouteDirectionsResponse> GetRouteDirections(List<BasicGeoposition> waypoints, string language, TravelMode travelMode, List<Avoid> avoids,
            RouteType routeType, int alternativeRouteCount, DateTime? departureTime, DateTime? arrivalTime)
        {
           
            var rawresult = await MainPage.azureMaps.GetRouteDirectionsAsync(waypoints, language, true, null, routeType, null, null, travelMode, avoids, 
                null, null, null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null, alternativeRouteCount, 
                null, null, arrivalTime, departureTime, null, RouteInstructionsType.text, null, ComputeTravelTimeFor.all, null, null, null);

            try
            {
                return rawresult as RouteDirectionsResponse;
            }
            catch
            { return null; }
        }
    
        public static async Task<List<Result>> SearchPOIsByCategory(string category, BasicGeoposition location, string language, string countryCode)
        {
            var rawresult = await MainPage.azureMaps.SearchPOICategoryAsync(category, null, null, null, location, null, null, null, null, language, null, countryCode);
            var result = rawresult as SearchAddressResponse;

            return result.results;
        }
    
    }
}
