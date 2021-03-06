﻿using System;
using System.Collections.Generic;
using System.Net;
using com.esendex.sdk.http;
using com.esendex.sdk.rest;
using com.esendex.sdk.rest.resources;
using com.esendex.sdk.sent;
using com.esendex.sdk.utilities;
using Moq;
using NUnit.Framework;

namespace com.esendex.sdk.test.sent
{
    [TestFixture]
    public class SentServiceTests
    {
        private SentService service;

        private Mock<ISerialiser> mockSerialiser;
        private Mock<IRestClient> mockRestClient;

        [SetUp]
        public void TestInitialize()
        {
            mockSerialiser = new Mock<ISerialiser>();
            mockRestClient = new Mock<IRestClient>();

            service = new SentService(mockRestClient.Object, mockSerialiser.Object);
        }

        [Test]
        public void DefaultConstructor()
        {
            // Arrange
            var credentials = new EsendexCredentials("username", "password");

            // Act
            var serviceInstance = new SentService(credentials);

            // Assert
            Assert.That(serviceInstance.RestClient, Is.InstanceOf<RestClient>());
            Assert.That(serviceInstance.Serialiser, Is.InstanceOf<XmlSerialiser>());
        }

        [Test]
        public void DefaultDIConstructor()
        {
            // Arrange
            var uri = new Uri("http://tempuri.org");
            var credentials = new EsendexCredentials("username", "password");
            IHttpRequestHelper httpRequestHelper = new HttpRequestHelper();
            IHttpResponseHelper httpResponseHelper = new HttpResponseHelper();
            IHttpClient httpClient = new HttpClient(credentials, uri, httpRequestHelper, httpResponseHelper);

            IRestClient restClient = new RestClient(httpClient);
            ISerialiser serialiser = new XmlSerialiser();

            // Act
            var serviceInstance = new SentService(restClient, serialiser);

            // Assert
            Assert.That(serviceInstance.RestClient, Is.InstanceOf<RestClient>());
            Assert.That(serviceInstance.Serialiser, Is.InstanceOf<XmlSerialiser>());
        }

        [Test]
        public void GetBatchMessages_WithId_ReturnsSentBatchMessages()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 15;
            var id = Guid.NewGuid();

            RestResource resource = new MessageBatchesResource(id);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var expectedResult = new SentMessageCollection();

            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessageCollection>(response.Content))
                .Returns(expectedResult);

            // Act
            var actualResult = service.GetBatchMessages(id);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetMessage_WithId_ReturnsSentMessage()
        {
            // Arrange
            var id = Guid.NewGuid();

            RestResource resource = new MessageHeadersResource(id);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var expectedResult = new SentMessage();

            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessage>(response.Content))
                .Returns(expectedResult);

            // Act
            var actualResult = service.GetMessage(id);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        public void GetMessages_WithPageNumberAndPageSize_ReturnsSentMessages()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 15;

            RestResource resource = new MessageHeadersResource(pageNumber, pageSize);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var expectedResult = new SentMessageCollection
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessageCollection>(response.Content))
                .Returns(expectedResult);

            // Act
            var actualResult = service.GetMessages(pageNumber, pageSize);

            // Assert
            Assert.AreEqual(pageNumber, actualResult.PageNumber);
            Assert.AreEqual(pageSize, actualResult.PageSize);
        }

        [Test]
        public void GetMessages_WithPageNumberAndPageSizeAndAccountReference_ReturnsSentMessages()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 15;
            var accountReference = "accountReference";

            RestResource resource = new MessageHeadersResource(accountReference, pageNumber, pageSize);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var expectedResult = new SentMessageCollection
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessageCollection>(response.Content))
                .Returns(expectedResult);

            // Act
            var actualResult = service.GetMessages(accountReference, pageNumber, pageSize);

            // Assert
            Assert.AreEqual(pageNumber, actualResult.PageNumber);
            Assert.AreEqual(pageSize, actualResult.PageSize);
        }

        [Test]
        public void GetMessages_WithFailedMessage_ReturnsSentMessagesWithFailureReason()
        {
            // Arrange
            var pageNumber = 1;
            var pageSize = 15;
            var accountReference = "accountReference";

            RestResource resource = new MessageHeadersResource(accountReference, pageNumber, pageSize);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var sentMessage = new SentMessage {FailureReason = new FailureReason {Code = 80, Description = "yolo", PermanentFailure = true}};

            var expectedResult = new SentMessageCollection
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Messages = new List<SentMessage> {sentMessage}
            };

            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessageCollection>(response.Content))
                .Returns(expectedResult);

            // Act
            var actualResult = service.GetMessages(accountReference, pageNumber, pageSize);

            // Assert
            Assert.AreEqual(pageNumber, actualResult.PageNumber);
            Assert.AreEqual(pageSize, actualResult.PageSize);

            Assert.That(actualResult.Messages[0].FailureReason.Code, Is.EqualTo(sentMessage.FailureReason.Code));
            Assert.That(actualResult.Messages[0].FailureReason.Description, Is.EqualTo(sentMessage.FailureReason.Description));
            Assert.That(actualResult.Messages[0].FailureReason.PermanentFailure, Is.EqualTo(sentMessage.FailureReason.PermanentFailure));
        }

        [Test]
        public void GetMessage_WithFailedMessage_ReturnsSentMessagesWithFailureReason()
        {

            var messageId = Guid.NewGuid();

            RestResource resource = new MessageHeadersResource(messageId);

            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = "serialisedItem"
            };

            var sentMessage = new SentMessage { Id = messageId, FailureReason = new FailureReason { Code = 80, Description = "yolo", PermanentFailure = true } };


            mockRestClient
                .Setup(rc => rc.Get(resource))
                .Returns(response);

            mockSerialiser
                .Setup(s => s.Deserialise<SentMessage>(response.Content))
                .Returns(sentMessage);


            // Act
            var result = service.GetMessage(sentMessage.Id);

            // Assert
            Assert.That(result.FailureReason.Code, Is.EqualTo(sentMessage.FailureReason.Code));
            Assert.That(result.FailureReason.Description, Is.EqualTo(sentMessage.FailureReason.Description));
            Assert.That(result.FailureReason.PermanentFailure, Is.EqualTo(sentMessage.FailureReason.PermanentFailure));
        }
    }
}