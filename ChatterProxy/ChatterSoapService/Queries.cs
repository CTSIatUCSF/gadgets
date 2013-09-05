using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatterService
{
    class Queries
    {
        public const string SOQL_GET_RESEACH_PROFILE = @"
            Select r.Id, r.User__r.Id, r.User__r.UCSF_ID__c, r.User__r.Name, r.User__r.FirstName, r.User__r.LastName, 
            r.User__r.Username, r.User__c, r.Research_Profile_Name__c, r.Name, r.Owner.Id, r.Owner.Name 
            From Research_Profile__c r
            WHERE r.User__r.UCSF_ID__c = '{0}'
            ORDER BY r.Id DESC LIMIT 1";

        public const string SOQL_GET_USER = @"
            SELECT u.Username, u.UCSF_ID__c, u.Name, u.LastName, u.Id, u.FirstName 
            FROM User u WHERE u.ucsf_id__c = '{0}' 
            ORDER BY u.Id DESC LIMIT 1";

        public const string SOQL_GET_GROUP = @"
            SELECT g.Name, g.Description, g.OwnerId, g.CollaborationType 
            FROM CollaborationGroup g WHERE g.Id = '{0}' 
            ORDER BY g.Id DESC LIMIT 1";

        public const string SOQL_GET_PROFILE_ACTIVITIES = @"
            Select u.Type,  u.ParentId, u.Id, u.CreatedDate, u.CreatedById, Body, LinkUrl, Title,
            Parent.User__r.Name, Parent.User__r.FirstName, Parent.User__r.LastName, Parent.User__r.UCSF_ID__c 
            From Research_Profile__Feed u 
            Where (Type='TextPost' or Type='LinkPost') and u.IsDeleted = false 
            ORDER BY CreatedDate DESC, ID DESC LIMIT {0}";

        public const string SOQL_GET_PROFILE_ACTIVITIES_AFTER = @"
            Select u.Type,  u.ParentId, u.Id, u.CreatedDate, u.CreatedById, Body, LinkUrl, Title,
            Parent.User__r.Name, Parent.User__r.FirstName, Parent.User__r.LastName, Parent.User__r.UCSF_ID__c 
            From Research_Profile__Feed u 
            Where (Type='TextPost' or Type='LinkPost') and u.IsDeleted = false and u.CreatedDate > {0}Z 
            ORDER BY CreatedDate DESC, ID DESC LIMIT {1}";

        public const string SOQL_GET_PROFILE_ACTIVITIES_BY_USER = @"
            Select u.Type,  u.ParentId, u.Id, u.CreatedDate, u.CreatedById, Body, LinkUrl, Title,
            Parent.User__r.Name, Parent.User__r.FirstName, Parent.User__r.LastName, Parent.User__r.UCSF_ID__c 
            From Research_Profile__Feed u 
            Where (Type='TextPost' or Type='LinkPost') and u.IsDeleted = false and Parent.User__r.id = '{0}'
            ORDER BY CreatedDate DESC, ID DESC LIMIT {1}";

        public const string SOQL_GET_ALL_USER_ACTIVITIES = @"
            Select u.Type,  u.ParentId, u.Id, u.CreatedDate, u.CreatedById, Body, LinkUrl, Title,
            Parent.Name, Parent.FirstName, Parent.LastName, Parent.UCSF_ID__c, 
            (SELECT ID, FieldName, OldValue, NewValue FROM FeedTrackedChanges ORDER BY ID DESC) 
            From UserFeed u 
            Where (Type='TextPost' or Type='LinkPost' or Type='UserStatus') and u.IsDeleted = false
            ORDER BY CreatedDate DESC, ID DESC LIMIT {0}";

        public const string SOQL_GET_USER_ACTIVITIES = @"
            SELECT Id, Type, CreatedDate, CreatedById, Body, ParentId,
            Parent.Name, Parent.FirstName, Parent.LastName, Parent.Type 
            FROM UserProfileFeed 
            Where Type = 'UserStatus' and Parent.Type = 'User'
            WITH UserId = '{0}'
            ORDER BY CreatedDate DESC, ID DESC LIMIT {1}";
    }
}
