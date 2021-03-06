﻿using System;
using System.Web.Http;
using System.Web.Http.Results;
using CSM.ParkingData.Controllers;
using CSM.ParkingData.Models;
using CSM.ParkingData.ViewModels;
using NUnit.Framework;

namespace CSM.ParkingData.Tests.Lifetime
{
    [TestFixture]
    public class ControllerTests : ControllerTestsBase
    {
        private LifetimeController _controller;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _controller = new LifetimeController(_mockSensorEventsService.Object) {
                Request = _requestStub,
                RequestContext = _requestContextStub
            };
        }

        [Test]
        [Category("SensorEventsLifetime")]
        public void Max_ReturnsSensorEventMaxLifetime()
        {
            IHttpActionResult actionResult = _controller.Max();
            var contentResult = actionResult as OkNegotiatedContentResult<SensorEventLifetime>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(_lifetimeStub.Length, contentResult.Content.Length);
            Assert.AreEqual(DateTimeKind.Utc, contentResult.Content.Since.Kind);
            Assert.AreEqual(_lifetimeStub.Since, contentResult.Content.Since);
            Assert.AreEqual(_lifetimeStub.Units, contentResult.Content.Units);
        }

        [Test]
        [Category("SensorEventsLifetime")]
        public void Default_ReturnsSensorEventMaxLifetime_WhenDefaultIsNull()
        {
            SensorEventLifetime notConfigured = null;

            _mockSensorEventsService
                .Setup(m => m.GetDefaultLifetime())
                .Returns(notConfigured);

            IHttpActionResult actionResult = _controller.Default();
            var contentResult = actionResult as OkNegotiatedContentResult<SensorEventLifetime>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(_lifetimeStub.Length, contentResult.Content.Length);
            Assert.AreEqual(DateTimeKind.Utc, contentResult.Content.Since.Kind);
            Assert.AreEqual(_lifetimeStub.Since, contentResult.Content.Since);
            Assert.AreEqual(_lifetimeStub.Units, contentResult.Content.Units);
        }

        [Test]
        [Category("SensorEventsLifetime")]
        public void Default_ReturnsSensorEventDefaultLifetime()
        {
            var mockDefaultLifetime = new SensorEventLifetime {
                Length = 1,
                Units = TimeSpanUnits.Hours,
                Since = new DateTime(1, DateTimeKind.Utc)
            };

            _mockSensorEventsService
                .Setup(m => m.GetDefaultLifetime())
                .Returns(mockDefaultLifetime);

            IHttpActionResult actionResult = _controller.Default();
            var contentResult = actionResult as OkNegotiatedContentResult<SensorEventLifetime>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(mockDefaultLifetime.Length, contentResult.Content.Length);
            Assert.AreEqual(DateTimeKind.Utc, contentResult.Content.Since.Kind);
            Assert.AreEqual(mockDefaultLifetime.Since, contentResult.Content.Since);
            Assert.AreEqual(mockDefaultLifetime.Units, contentResult.Content.Units);
        }
    }
}
