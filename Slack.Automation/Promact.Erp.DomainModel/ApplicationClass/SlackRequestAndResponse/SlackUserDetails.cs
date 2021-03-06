﻿using Newtonsoft.Json;
using Promact.Erp.DomainModel.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Promact.Erp.DomainModel.ApplicationClass.SlackRequestAndResponse
{
    public class SlackUserDetails : ModelBase
    {
        /// <summary>
        /// User Id of Slack for user
        /// </summary>
        [JsonProperty("id")]
        public string UserId { get; set; }

        /// <summary>
        /// Team Id of Slack user
        /// </summary>
        [JsonProperty("team_id")]
        public string TeamId { get; set; }

        /// <summary>
        /// Name of the slack user
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Bool true for they exist in team or not
        /// </summary>
        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        /// <summary>
        /// Status of the slack user
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Real Name of the slack user
        /// </summary>
        [JsonProperty("real_name")]
        public string RealName { get; set; }

        /// <summary>
        /// Timezone label of the slack user
        /// </summary>
        [JsonProperty("tz_label")]
        public string TimeZoneLabel { get; set; }

        /// <summary>
        /// Timezone offset of the slack user
        /// </summary>
        [JsonProperty("tz_offset")]
        public string TimeZoneOffset { get; set; }

        /// <summary>
        /// True if it is Admin
        /// </summary>
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// True if it is Owner
        /// </summary>
        [JsonProperty("is_owner")]
        public bool IsOwner { get; set; }

        /// <summary>
        /// True if it is Primary Owner
        /// </summary>
        [JsonProperty("is_primary_owner")]
        public bool IsPrimaryOwner { get; set; }

        /// <summary>
        /// True if it is Restricted User
        /// </summary>
        [JsonProperty("is_restricted")]
        public bool IsRestrictedUser { get; set; }

        /// <summary>
        /// True if it is Ultra Restricted User
        /// </summary>
        [JsonProperty("is_ultra_restricted")]
        public bool IsUltraRestrictedUser { get; set; }

        /// <summary>
        /// True if it is Bot
        /// </summary>
        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [NotMapped]
        [JsonProperty("profile")]
        public SlackProfile Profile { get; set; }

        /// <summary>
        /// Title of the slack user
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }


        /// <summary>
        /// Contact No. of the slack user
        /// </summary>
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Skype of the slack user
        /// </summary>
        [JsonProperty("skype")]
        public string Skype { get; set; }

        /// <summary>
        /// First Name of the slack user
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name of the slack user
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Email of the slack user
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

    
    }
}
