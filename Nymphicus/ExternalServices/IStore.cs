using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nymphicus.Model;
using Nymphicus.API;

namespace Nymphicus.ExternalServices
{
    interface IStore
    {
        string Name { get; }
        string Description { get; }
        string Homepage { get; }
        bool UsesOAuth { get;}
        string Username { get; set; }
        string Password { get; set; }
        bool PasswordVerified { get; set; }
        string LastError { get; }
        string ServiceIcon { get; }
        string ServiceIconRelativePath { get; }

        bool CanVerifyCredentials { get; }
        bool VerifyCredentials();
        bool SendNow(IItem item, string url);

    }

   
}
