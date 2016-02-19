using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Manager for communicating with the OSM web services.
    /// </summary>
    public class Manager
    {
        private readonly Lazy<BadgeManager> badge;
        private readonly CacheSettings cacheSettings = new CacheSettings();
        private readonly Lazy<GroupManager> group;
        private readonly Lazy<MemberManager> member;
        private readonly Lazy<TermManager> term;
        private Connection connection;

        /// <summary>
        /// Initialise an unauthorised <see cref="Manager"/> instance.
        /// </summary>
        /// <param name="apiId">The application's API ID.</param>
        /// <param name="token">The application token.</param>
        public Manager(string apiId, string token)
            : this()
        {
            this.connection = new Connection(apiId, token, null, null);
        }

        /// <summary>
        /// Initialise an authorised <see cref="Manager"/> instance.
        /// </summary>
        /// <param name="apiId">The application's API ID.</param>
        /// <param name="token">The application token.</param>
        /// <param name="userId">The authorised user ID.</param>
        /// <param name="secret">The secret for the authorised user.</param>
        public Manager(string apiId, string token, string userId, string secret)
            : this()
        {
            this.connection = new Connection(apiId, token, userId, secret);
        }

        private Manager()
        {
            this.badge = new Lazy<BadgeManager>(() => new BadgeManager(this.connection, this.cacheSettings));
            this.group = new Lazy<GroupManager>(() => new GroupManager(this.connection));
            this.member = new Lazy<MemberManager>(() => new MemberManager(this.connection, this.cacheSettings));
            this.term = new Lazy<TermManager>(() => new TermManager(this.connection, this.cacheSettings));
        }

        /// <summary>
        /// The authorisation details.
        /// </summary>
        public IAuthorisation Authorisation
        {
            get { return this.connection; }
        }

        /// <summary>
        /// Operations for working with badges.
        /// </summary>
        public BadgeManager Badge
        {
            get
            {
                this.EnsureAuthorised();
                return this.badge.Value;
            }
        }

        /// <summary>
        /// The settings for controlling the caches.
        /// </summary>
        public CacheSettings CacheSettings
        {
            get { return this.cacheSettings; }
        }

        /// <summary>
        /// Operations for working with groups and sections.
        /// </summary>
        public GroupManager Group
        {
            get
            {
                this.EnsureAuthorised();
                return this.group.Value;
            }
        }

        /// <summary>
        /// Operations for working with members.
        /// </summary>
        public MemberManager Member
        {
            get
            {
                this.EnsureAuthorised();
                return this.member.Value;
            }
        }

        /// <summary>
        /// Operations for working with terms.
        /// </summary>
        public TermManager Term
        {
            get
            {
                this.EnsureAuthorised();
                return this.term.Value;
            }
        }

        /// <summary>
        /// First time call to authorise a user and initialise the secret.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns><c>true</c> if the user is authorised; <c>false</c> otherwise.</returns>
        public async Task<bool> Authorise(string email, string password)
        {
            var loginValues = new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password
            };
            var response = await this.connection.PostAsync("users.php?action=authorise", loginValues);
            var result = JsonConvert.DeserializeObject<AuthoriseResult>(response);
            if (result == null) return false;

            this.connection = this.connection.AddAuthorisation(result.userid, result.secret);
            return this.connection.IsAuthorised;
        }

        private void EnsureAuthorised()
        {
            if (!this.connection.IsAuthorised) throw new Exception("Not authorised");
        }

        private class AuthoriseResult
        {
            public string secret { get; set; }
            public string userid { get; set; }
        }
    }
}