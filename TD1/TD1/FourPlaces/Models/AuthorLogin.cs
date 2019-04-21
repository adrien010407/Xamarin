using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TD1.FourPlaces.Models
{
    class AuthorLogin
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        public AuthorLogin(string email, string pass)
        {
            Email = email;
            Password = pass;
        }

        public AuthorLogin()
        {

        }
    }
}
