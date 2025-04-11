using System;
using System.Collections.Generic;

namespace CompanyManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required IList<string> Roles { get; set; } = new List<string>();
    }

    public class EmployeeProfileViewModel
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string FullName { get; set; }
        public required string IdentityNumber { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public required string Department { get; set; }
        public required string Position { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
        public required IList<string> Roles { get; set; } = new List<string>();
    }
} 