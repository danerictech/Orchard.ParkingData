﻿using CSM.ParkingData.ViewModels;
using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace CSM.ParkingData.Services
{
    public class ParkingLotsService : IParkingLotsService
    {
        public IEnumerable<ParkingLot> Get()
        {
            string lotDataUrl = CloudConfigurationManager.GetSetting("ParkingLotDataUrl");
            XDocument xdocument;

            try
            {
                xdocument = XDocument.Load(lotDataUrl);
                return Get(xdocument);
            }
            catch
            {
                return Enumerable.Empty<ParkingLot>();
            }
        }

        public IEnumerable<ParkingLot> Get(XDocument lotXml)
        {
            var parkingLots = new List<ParkingLot>();
            DateTime lastUpdateUtc = ParseLastUpdateUtc(lotXml.Root);

            foreach (var lot in lotXml.Root.Elements("lot"))
            {
                var parkingLot = ParseFromXml(lot);
                parkingLot.LastUpdate = lastUpdateUtc;
                parkingLots.Add(parkingLot);
            }

            return parkingLots;
        }

        public ParkingLot ParseFromXml(XElement xml)
        {
            return new ParkingLot()
            {
                AvailableSpaces = Convert.ToInt32(xml.Element("available").Value),
                Description = xml.Element("description").Value,
                Latitude = Convert.ToDecimal(xml.Element("latitude").Value),
                Longitude = Convert.ToDecimal(xml.Element("longitude").Value),
                Name = xml.Element("name").Value,
                StreetAddress = xml.Element("address").Value,
                ZipCode = Convert.ToInt32(xml.Element("zip").Value)
            };
        }

        public DateTime ParseLastUpdateUtc(XElement xml)
        {
            string dateString = xml.Element("date").Value;
            string timeString = xml.Element("time").Value;
            string tzString = xml.Element("timezone").Value;
            
            DateTime local = DateTime.ParseExact(dateString + " " + timeString, "yyyy-MM-dd HH:mm", null);
            TimeZoneInfo sourceTz = TimeZoneInfo.FindSystemTimeZoneById(tzString);

            return TimeZoneInfo.ConvertTimeToUtc(local, sourceTz);
        }
    }
}
