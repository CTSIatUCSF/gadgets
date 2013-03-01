using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatterService.Model;
using System.Web.Caching;

namespace ChatterService
{
    public enum ActivitySource
    {
        UserFeed = 1,
        ResearchPropfile = 2
    }

    public interface IChatterSoapService
    {
        /// <summary>
        /// Login to salesforce.com
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        bool Login(string username, string password, string token);

        /// <summary>
        /// Create new activity
        /// </summary>
        /// <param name="message"></param>
        /// <param name="timestamp"></param>
        void CreateActivity(string userId, string code, string message, DateTime timestamp);
        void CreateActivityUsingApex(string userId, string code, string message, DateTime timestamp);
        void CreateProfileActivity(string employeeId, string code, string message, DateTime timestamp);
            
        /// <summary>
        /// Return all activities.
        /// </summary>
        /// <returns>Collection of activity models.</returns>
        List<Activity> GetProfileActivities(string userId, int count);
        List<Activity> GetProfileActivities(Activity lastActivity, int count);

        /// <summary>
        /// Return all activities for a user.
        /// </summary>
        /// <returns>Collection of activity models.</returns>
        List<Activity> GetActivities(string userId, int personID, bool includeUserActivities, int count);
        List<Activity> GetUserActivities(string userId, int personId, int count);


        /// <summary>
        /// Return correct UserId which corresponds for CustomObject ucsf_id__c.
        /// </summary>
        /// <param name="customObject_ucsf_id__c"></param>
        /// <returns></returns>
        string GetUserId(string employeeId);

        /// <summary>
        /// Creates Reseach Profile
        /// </summary>
        /// <param name="customObject_ucsf_id__c"></param>
        /// <returns></returns>
        void CreateResearchProfile(string employeeId);

        void AllowUntrustedConnection();

        string CreateGroup(String name, String description, string ownerEmployeeId);
        void AddUsersToGroup(String groupId, string[] users);
        void CreateExternalMessage(string url, string title, string body, string employeeId);
    }
}
