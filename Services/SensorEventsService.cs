﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSM.ParkingData.Models;
using CSM.ParkingData.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Services;
using Orchard.Settings;

namespace CSM.ParkingData.Services
{
    public class SensorEventsService : ISensorEventsService
    {
        private readonly IClock _clock;
        private readonly IMeteredSpacesService _meteredSpacesService;
        private readonly IRepository<SensorEvent> _sensorEventsRepo;
        private readonly ISiteService _siteService;

        public SensorEventsService(
            IClock clock,
            IMeteredSpacesService meteredSpacesService,
            IRepository<SensorEvent> sensorEventsRepo,
            ISiteService siteService)
        {
            _clock = clock;
            _meteredSpacesService = meteredSpacesService;
            _sensorEventsRepo = sensorEventsRepo;
            _siteService = siteService;
        }

        public SensorEventLifetime GetMaxLifetime()
        {
            var siteSettings = _siteService.GetSiteSettings();
            var sensorEventsSettings = siteSettings.As<SensorEventsSettings>();

            if (sensorEventsSettings == null)
            {
                sensorEventsSettings = new SensorEventsSettings();
                sensorEventsSettings.ContentItem = siteSettings.ContentItem;
            }

            var lifetime = new SensorEventLifetime() {
                Length = sensorEventsSettings.MaxLifetimeLength,
                Units = sensorEventsSettings.MaxLifetimeUnits,
                Since = getLifetimeSince(sensorEventsSettings.MaxLifetimeLength, sensorEventsSettings.MaxLifetimeUnits)
            };

            return lifetime;
        }

        public SensorEventLifetime GetDefaultLifetime()
        {
            var siteSettings = _siteService.GetSiteSettings();
            var sensorEventsSettings = siteSettings.As<SensorEventsSettings>();

            if (sensorEventsSettings == null)
            {
                sensorEventsSettings = new SensorEventsSettings();
                sensorEventsSettings.ContentItem = siteSettings.ContentItem;
            }

            var lifetime = new SensorEventLifetime() {
                Length = sensorEventsSettings.DefaultLifetimeLength,
                Units = sensorEventsSettings.DefaultLifetimeUnits,
                Since = getLifetimeSince(sensorEventsSettings.DefaultLifetimeLength, sensorEventsSettings.DefaultLifetimeUnits)
            };

            return lifetime;
        }

        public SensorEvent AddOrUpdate(SensorEventPOST viewModel)
        {
            var meteredSpace = _meteredSpacesService.AddOrUpdate(viewModel.MeteredSpace);

            var posted = new SensorEvent() {
                ClientId = viewModel.ClientID,
                EventTime = DateTime.Parse(viewModel.EventTime),
                EventType = viewModel.EventType,
                MeteredSpace = meteredSpace,
                ReceivedTime = _clock.UtcNow,
                SessionId = long.Parse(viewModel.MeteredSpace.SessionID),
                TransmissionId = long.Parse(viewModel.TransmissionID),
                TransmissionTime = DateTime.Parse(viewModel.TransmissionDateTime)
            };

            var existing = _sensorEventsRepo.Get(x => x.TransmissionId == posted.TransmissionId);

            if (existing == null)
            {
                _sensorEventsRepo.Create(posted);
            }
            else
            {
                posted.Id = existing.Id;
                _sensorEventsRepo.Update(posted);
            }

            return posted;
        }

        public SensorEventGET GetViewModel(SensorEvent entity)
        {
            return new SensorEventGET() {
                EventId = entity.TransmissionId,
                EventTime = entity.EventTime,
                EventType = entity.EventType,
                MeterId = entity.MeteredSpace.MeterId,
                SessionId = entity.SessionId,
                SequenceNumber = entity.Id,
            };
        }

        public IQueryable<SensorEvent> Query()
        {
            return _sensorEventsRepo
                    .Table
                    //taking advantage of the clustered index on Id
                    //since we normally want the most recent events (which are at the end)
                    .OrderByDescending(s => s.Id);
        }

        public IEnumerable<SensorEventGET> GetViewModelsSince(DateTime datetime)
        {
            return GetViewModelsSince(datetime, null);
        }

        public IEnumerable<SensorEventGET> GetViewModelsSince(DateTime datetime, string meterId)
        {
            var query = Query().Where(s => s.EventTime >= datetime);

            if (!String.IsNullOrEmpty(meterId))
                query = query.Where(s => s.MeteredSpace.MeterId == meterId);

            return query.Select(GetViewModel).OrderByDescending(vm => vm.EventTime);
        }

        public IEnumerable<SensorEventGET> GetViewModelsSince(long sequenceNumber)
        {
            return GetViewModelsSince(sequenceNumber, null);
        }

        public IEnumerable<SensorEventGET> GetViewModelsSince(long sequenceNumber, string meterId)
        {
            var maxLifetime = GetMaxLifetime();

            var query = Query().Where(s => s.Id >= sequenceNumber && s.EventTime >= maxLifetime.Since);

            if (!String.IsNullOrEmpty(meterId))
                query = query.Where(s => s.MeteredSpace.MeterId == meterId);

            return query.Select(GetViewModel).OrderByDescending(vm => vm.SequenceNumber);
        }

        private DateTime getLifetimeSince(double length, LifetimeUnits units)
        {
            DateTime since = DateTime.MaxValue;
            double lengthModifier = -1 * length;

            switch (units)
            {
                case LifetimeUnits.Hours:
                    since = _clock.UtcNow.AddHours(lengthModifier);
                    break;
                case LifetimeUnits.Minutes:
                    since = _clock.UtcNow.AddMinutes(lengthModifier);
                    break;
                case LifetimeUnits.Seconds:
                    since = _clock.UtcNow.AddSeconds(lengthModifier);
                    break;
            }

            return since;
        }
    }
}
