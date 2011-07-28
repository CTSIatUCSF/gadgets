using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatterService.Model;

namespace ChatterService
{

    public class ChatterService : IChatterService
    {
        public const string TEST_SERVICE_URL = "https://login.ucsf--ctsi.cs10.my.salesforce.com/services/Soap/c/22.0";

        private Salesforce.SforceService _service;
        public string Url { get; set; }

        public ChatterService(string url)
        {
            Url = url;
        }

        public bool Login(string username, string password, string token)
        {
            _service = new Salesforce.SforceService();
            _service.Url = Url;

            Salesforce.LoginResult result;
            try
            {
                result = _service.login(username, password + token);

                _service.Url = result.serverUrl;
                _service.SessionHeaderValue = new Salesforce.SessionHeader();
                _service.SessionHeaderValue.sessionId = result.sessionId;
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void CreateActivity(string userId, string code, string message, DateTime timestamp)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            var news = new Salesforce.FeedItem
            {
                ParentId = userId,
                CreatedById = userId,
                Type = "TextPost",
                Title = code,
                Body = message,
                CreatedDate = timestamp,
                LastModifiedDate = timestamp,

            };

            var descr = _service.describeSObject("FeedItem");
            var result = _service.create(new Salesforce.sObject[] { news });
//            var result = _service.upsert("Id", new Salesforce.sObject[] { news });
        }

        public void CreateActivityUsingApex(string userId, string code, string message, DateTime timestamp)
        {
            if (_service == null)
                throw new Exception("Service is null. You need to login first!");

            if (string.IsNullOrEmpty(message))
                throw new Exception("Invalid argument!");

            Salesforce.apex.ApexService service = new Salesforce.apex.ApexService();
            int idx1 = _service.Url.IndexOf(@"/services/");
            int idx2 = service.Url.IndexOf(@"/services/");
            service.Url = _service.Url.Substring(0, idx1) + service.Url.Substring(idx1);

            service.SessionHeaderValue = new Salesforce.apex.SessionHeader();
            service.SessionHeaderValue.sessionId = _service.SessionHeaderValue.sessionId;

            DateTime utc = timestamp.ToUniversalTime();
            String apex = "FeedItem post = new FeedItem(); \n" +
                    " post.ParentId = '" + userId  + "'; \n" +
                    " post.Title = '" + EncodeString(code) + "'; \n" +
                    " post.Body = '" + EncodeString(message) + "'; \n" +
                    " post.CreatedById = '" + userId + "'; \n" +
                    " post.CreatedDate = Datetime.newInstanceGmt(" + utc.Year + "," + utc.Month + "," + utc.Day + "," + utc.Hour + "," + utc.Minute + "," + utc.Second + "); \n" +
                    " insert post;";

            Salesforce.apex.ExecuteAnonymousResult result = service.executeAnonymous(apex);
            if (!result.success)
            {
                throw new Exception("Cannot create FeedItem:" + result.exceptionMessage + "\n" + result.exceptionStackTrace);
            }
        }

        protected String EncodeString(string str)
        {
            return str.Replace("'", "\\'");
        }

        public List<Activity> GetActivities(string userId)
        {
            Salesforce.QueryResult qr = _service.query
                    ("SELECT Id, Type, CreatedDate, CreatedById, Body, ParentId " +
                     "FROM UserProfileFeed WITH UserId = '" + userId + "'" +
                     "ORDER BY CreatedDate DESC, ID DESC LIMIT 100");

            var feeds = new List<Activity>();
            if (qr.size <= 0)
            {
                return feeds;
            }

            feeds.AddRange(from Salesforce.UserProfileFeed record in qr.records
                           select new Activity
                           {
                               Id = record.Id,
                               Message = record.Body,
                               CreatedDT = record.CreatedDate,
                               CreatedById = record.CreatedById
                           });

            return feeds;
        }

        public List<Activity> GetActivities(int count)
        {
            var feeds = new List<Activity>();
            Salesforce.QueryResult qr = _service.query
                   ("Select u.Type,  u.ParentId, u.Id, u.CreatedDate, u.CreatedById, Body," +
                    "       Parent.Name, Parent.FirstName, Parent.LastName, Parent.UCSF_ID__c, " +  
                    " (SELECT ID, FieldName, OldValue, NewValue FROM FeedTrackedChanges ORDER BY ID DESC) " +
                    " From UserFeed u " +
                    " Where (Type='TextPost' or Type='UserStatus') and u.IsDeleted = false" +
                    " ORDER BY CreatedDate DESC, ID DESC LIMIT " + count);

            feeds.AddRange(from Salesforce.UserFeed record in qr.records
                           select new Activity
                           {
                               Id = record.Id,
                               Message = record.Body,
                               CreatedDT = record.CreatedDate,
                               CreatedById = record.CreatedById,
                               Type = ActivityType.TextPost,
                               Parent = new User 
                               {
                                   Id = record.ParentId,
                                   Name = record.Parent.Name,
                                   FirstName = record.Parent.FirstName,
                                   LastName = record.Parent.LastName,
                                   EmployeeId = record.Parent.UCSF_ID__c
                               }
 
                           });

            return feeds;
        }

        public string GetUserId(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                throw new Exception("Employee Id is required");
            }

            Salesforce.QueryResult qr = _service.query
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
