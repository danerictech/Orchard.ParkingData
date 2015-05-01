﻿using System;
using System.Runtime.Serialization;
using CSM.ParkingData.Models;

namespace CSM.ParkingData.ViewModels
{
    [DataContract(Name = "lifetime", Namespace = "")]
    public class SensorEventLifetime
    {
        public LifetimeUnits Units { get; set; }

        [DataMember(Name = "length")]
        public double Length { get; set; }

        [DataMember(Name = "units")]
        public string UnitsString
        {
            get { return Units.ToString().ToLower(); }
            private set { ; }
        }

        [DataMember(Name = "since")]
        public DateTime Since { get; set; }
    }
}
