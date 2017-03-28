namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

    public class SimpleAuthorizationContext : AuthorizationContext
    {
        private List<ClaimSet> claimSets;
        private DateTime expirationTime;
        private string id;
        private Dictionary<string, object> properties;

        public SimpleAuthorizationContext()
        {
            this.id = Guid.NewGuid().ToString();
            this.expirationTime = DateTime.Now.AddDays(1);
            this.claimSets = new List<ClaimSet>();
            this.properties = new Dictionary<string, object>();
        }

        public override ReadOnlyCollection<ClaimSet> ClaimSets
        {
            get
            {
                return new ReadOnlyCollection<ClaimSet>(this.claimSets);
            }
        }

        public override DateTime ExpirationTime
        {
            get
            {
                return this.expirationTime;
            }
        }

        public override string Id
        {
            get
            {
                return this.id;
            }
        }

        public override IDictionary<string, object> Properties
        {
            get
            {
                return this.properties;
            }
        }

        internal void AddIdentity(IIdentity genericIdentity)
        {
            List<IIdentity> identities = null;
            if (this.properties.ContainsKey("Identities"))
            {
                identities = (List<IIdentity>)this.properties["Identities"];
            }
            else
            {
                identities = new List<IIdentity>();
                this.properties["Identities"] = identities;
            }

            identities.Add(genericIdentity);
            this.claimSets.Add(new DefaultClaimSet(
                new Claim(ClaimTypes.Authentication, genericIdentity, Rights.Identity)));
        }
    }
}
