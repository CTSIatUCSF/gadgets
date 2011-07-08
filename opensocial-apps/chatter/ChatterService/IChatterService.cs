using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChatterService.Model;


namespace ChatterService
{

    public interface IChatterService
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
    void CreateActivity(string userId, string message, DateTime timestamp);
    void CreateActivityUsingApex(string userId, string message, DateTime timestamp);
        
    /// <summary>
    /// Return all activities.
    /// </summary>
    /// <returns>Collection of activity models.</returns>
    List<Activity> GetActivities();

    /// <summary>
    /// Return all activities for a user.
    /// </summary>
    /// <returns>Collection of activity models.</returns>
    List<Activity> GetActivities(string userId);


    /// <summary>
    /// Return correct UserId which corresponds for CustomObject ucsf_id__c.
    /// </summary>
    /// <param name="customObject_ucsf_id__c"></param>
    /// <returns></returns>
    string GetUserId(string employId);
    }
}
