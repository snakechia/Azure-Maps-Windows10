using AzureMapsLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureMapsUWP.Helpers
{
    public class RouteData : BindableBase
    {
        private bool isSelected;
        public bool IsSelected { get { return isSelected; } set { SetProperty(ref isSelected, value); } }

        private bool isShowed;
        public bool IsShowed { get { return isShowed; } set { SetProperty(ref isShowed, value); OnPropertyChanged(nameof(ShowButtonLabel)); } }

        private string startLocationLable;
        public string StartLocationLable { get { return startLocationLable; } set { SetProperty(ref startLocationLable, value); } }

        private string endLocationLabel;
        public string EndLocationLabel { get { return endLocationLabel; } set { SetProperty(ref endLocationLabel, value); } }

        public string ShowButtonLabel => !IsShowed ? "More details" : "Less details";

        private bool isLeaveNow;
        public bool IsLeaveNow { get { return isLeaveNow; } set { SetProperty(ref isLeaveNow, value); } }


        private bool isArriveBy;
        public bool IsArriveBy { get { return isArriveBy; } set { SetProperty(ref isArriveBy, value); OnPropertyChanged(nameof(ArriveByMessage)); } }
        public string ArriveByMessage => IsArriveBy ? "Leave around " + Route.summary.departureTime.ToShortTimeString() : "";


        private bool isLeaveAt;
        public bool IsLeaveAt { get { return isLeaveAt; } set { SetProperty(ref isLeaveAt, value); OnPropertyChanged(nameof(LeaveAtMessage)); } }
        public string LeaveAtMessage => IsLeaveAt ? "Arrive around " + Route.summary.arrivalTime.ToShortTimeString()  : "";


        /// <summary>
        /// Gets or sets the Route data return from Azure Maps REST APIs
        /// </summary>
        private Route route;
        public Route Route { get { return route; } set { SetProperty(ref route, value); } }


    }
}
