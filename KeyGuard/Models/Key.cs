using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyGuard.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Key : ObservableValidator
    {
        [JsonProperty]
        [ObservableProperty]
        private string _id;

        [JsonProperty]
        [Required(ErrorMessage = "Service name is required.")]
        [MaxLength(100, ErrorMessage = "Service name must be 100 characters at most.")]
        [ObservableProperty]
        private string _service;

        [JsonProperty]
        [MaxLength(400, ErrorMessage = "Link to service must be 400 characters at most.")]
        [ObservableProperty]
        private string _serviceLink;

        [JsonProperty]
        [MaxLength(100, ErrorMessage = "Username must be 100 characters at most.")]
        [ObservableProperty]
        private string _username;

        [JsonProperty]
        [EmailAddress(ErrorMessage = "Email address is invalid.")]
        [ObservableProperty]
        private string _email;

        [JsonProperty]
        [MaxLength(512, ErrorMessage = "Password must be 512 characters at most.")]
        [ObservableProperty]
        private string _password;

        [JsonProperty]
        [MaxLength(400, ErrorMessage = "Security question must be 400 characters at most.")]
        [ObservableProperty]
        private string _securityQuestion;

        [JsonProperty]
        [MaxLength(2000, ErrorMessage = "Other information must be 2000 characters at most.")]
        [ObservableProperty]
        private string _other;

        public ValidationResult Validate()
        {
            ValidateAllProperties();

            return HasErrors ? new ValidationResult(GetErrors().First().ToString()) : ValidationResult.Success;
        }
    }
}
