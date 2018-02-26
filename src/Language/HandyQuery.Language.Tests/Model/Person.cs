using System;

namespace HandyQuery.Language.Tests.Model
{
    public enum Gender
    {
        Unknown,
        Male,
        Female
    }
    
    public class Person
    {
        public long Id { get; set; }
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public short Age { get; set; }
        
        public Gender Gender { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        
        public decimal Salary { get; set; }
        public bool IsEmployed { get; set; }
        
        public int LuckyNumber { get; set; }
    }
}