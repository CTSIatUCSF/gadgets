using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Salesforce;
using ChatterService.Model;

namespace ChatterService
{

    public class ChatterService : IChatterService
    {
        public const string TEST_SERVICE_URL = "https://login.ucsf--ctsi.cs10.my.salesforce.com/services/Soap/c/22.0";

        private SalesforceService _service;
        public string Url { get; set; }

        public ChatterService(string url)
        {
            Url = url;
        }

        public bool Login(string username, string password, string token)
        {
            _service = new SalesforceService();
            _service.Url = Url;

            LoginResult result;
            try
            {
                result = _service.login(username, password + token);

                _service.Url = result.serverUrl;
                _service.SessionHeaderValue = new SessionHeader();
                _service.SessionHeaderValue.sessionId = result.sessionId;
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void CreateActivity(string userId, string message, DateTime timestamp)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            var news = new FeedItem
            {
                ParentId = userId,
                CreatedById = userId,
                Type = "TextPost",
                Body = message,
                CreatedDate = timestamp

            };

            var result = _service.upsert("Id", new sObject[] { news });
        }

        public List<Activity> GetActivities(string userId)
        {
            QueryResult qr = _service.query
                ("SELECT Id, Type, CreatedDate, CreatedBy.name, Body " +
                 "FROM NewsFeed WHERE Type='UserStatus' AND CreatedById = '" + userId + "' " +
                 "ORDER BY CreatedDate DESC, ID DESC LIMIT 15");

            var feeds = new List<Activity>();
            if (qr.size <= 0)
                return feeds;

            feeds.AddRange(from NewsFeed record in qr.records
                           select new Activity
                           {
                               ActivityId = record.Id,
                               Message = record.Body
                           });
            return feeds;
        }

        public string GetUserId(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }

            QueryResult qr = _service.query
                ("SELECT Id " +
                 "FROM User WHERE ucsf_id__c = '" + employeeId + "' " +
                 "ORDER BY Id DESC, ID DESC LIMIT 15");
            if (qr.records == null)
            {
                throw new Exception("Cannot find user with employeeId=" + employeeId);
            }
            var user = qr.records.FirstOrDefault();
            return user.Id;
        }
    }
}
