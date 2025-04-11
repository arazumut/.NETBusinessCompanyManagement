using System;
using Microsoft.AspNetCore.Identity;

namespace CompanyManagementSystem.Models
{
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Employee = "Employee";

        public static readonly string[] AllRoles = new[] { Admin, Manager, Employee };
    }
} 