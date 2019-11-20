using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace AzureMapsUWP.Helpers
{
    public class LocationData : BindableBase
    {
        private string name;
        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private string address;
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public string Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }

        private string countrycode;
        public string CountryCode { get { return countrycode; } set { SetProperty(ref countrycode, value); } }

        private string formattedaddress;
        /// <summary>
        /// Gets or sets the address of the location.
        /// </summary>
        public string FormattedAddress
        {
            get { return formattedaddress; }
            set { SetProperty(ref formattedaddress, value); }
        }

        private BasicGeoposition position;
        /// <summary>
        /// Gets the geographic position of the location.
        /// </summary>
        public BasicGeoposition Position
        {
            get { return position; }
            set
            {
                SetProperty(ref position, value);
                OnPropertyChanged(nameof(Geopoint));
            }
        }

        /// <summary>
        /// Gets a Geopoint representation of the current location for use with the map service APIs.
        /// </summary>
        public Geopoint Geopoint => new Geopoint(Position);

        private bool isCurrentLocation;
        /// <summary>
        /// Gets or sets a value that indicates whether the location represents the user's current location.
        /// </summary>
        public bool IsCurrentLocation
        {
            get { return isCurrentLocation; }
            set
            {
                SetProperty(ref isCurrentLocation, value);
                OnPropertyChanged(nameof(NormalizedAnchorPoint));
            }
        }

        private bool isSelected;
        /// <summary>
        /// Gets or sets a value that indicates whether the location is 
        /// the currently selected one in the list of saved locations.
        /// </summary>
        [IgnoreDataMember]
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
                OnPropertyChanged(nameof(PinColor));
            }
        }

        /// <summary>
        /// Gets a path to an image to use as a map pin, reflecting the IsSelected property value. 
        /// </summary>
        public string PinColor => IsSelected ? "Black" : "Orange";

        private Point centerpoint = new Point(0.5, 0.5);
        private Point pinpoint = new Point(0.5, 0.5);
        /// <summary>
        /// Gets a value for the MapControl.NormalizedAnchorPoint attached property
        /// to reflect the different map icon used for the user's current location. 
        /// </summary>
        public Point NormalizedAnchorPoint => IsCurrentLocation ? centerpoint : pinpoint;

        private bool isSaved;
        /// <summary>
        /// Gets or sets a value that indicates whether this location is 
        /// being saved for for future access.
        /// </summary>
        public bool IsSaved
        {
            get { return isSaved; }
            set { SetProperty(ref isSaved, value); }
        }

    }
}
