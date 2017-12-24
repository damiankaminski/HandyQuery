using System;

namespace HandyQuery.Language.Tests.Model
{
    public class Employee
    {
        public decimal? Salary { get; set; }
        public Person Person { get; set; }
        public DateTime? Created { get; set; }
        public int FunLevel { get; set; }
        public int TestCase { get; set; }
        public int TestCase2 { get; set; }
        public bool HasPersonInfo => Person != null;
        public bool? IsAwesome { get; set; }
        public string Foo { get; set; }

        public string FirstName => Person?.FirstName;

        public string LastName => Person?.LastName;
    }
}
