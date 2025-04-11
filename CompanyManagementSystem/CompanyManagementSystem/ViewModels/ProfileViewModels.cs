using System;
using System.Collections.Generic;

namespace CompanyManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class EmployeeProfileViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsActive { get; set; }
        public IList<string> Roles { get; set; }
    }
} 