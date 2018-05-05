using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using HandyQuery.Language.Lexing;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Lexing
{
    public class SearchTrieTests
    {
        [TestCaseSource(nameof(TestCases))]
        [TestCaseSource(nameof(TestCasesWithOffset))]
        public void Should_work_with_basic_test_cases(TestCase testCase)
        {
            var trie = SearchTrie<Person>.Create(testCase.CaseSensitive, testCase.Map);
            var reader = new LexerStringReader(testCase.Query, testCase.StartPosition);

            var found = trie.TryFind(reader, out var person, out var readLength);

            found.Should().Be(testCase.ShouldBeFound);
            person.Should().Be(testCase.ExpectedValue);
            readLength.Should().Be(testCase.ExpectedReadLength);
        }

        private static IEnumerable<TestCase> TestCasesWithOffset
        {
            get
            {
                foreach (var testCase in TestCases)
                {
                    const string offsetValue = "OFFSET ";

                    yield return new TestCase()
                    {
                        CaseSensitive = testCase.CaseSensitive,
                        Map = testCase.Map,
                        Query = offsetValue + testCase.Query,
                        StartPosition = testCase.StartPosition + offsetValue.Length,
                        ShouldBeFound = testCase.ShouldBeFound,
                        ExpectedValue = testCase.ExpectedValue,
                        ExpectedReadLength = testCase.ExpectedReadLength
                    };
                }
            }
        }

        private static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["test"] = "John"
                    },
                    Query = "test",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "test".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["test"] = "John"
                    },
                    Query = "tes",
                    ShouldBeFound = false,
                    ExpectedReadLength = 0
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["test"] = "John"
                    },
                    Query = "testt and even more stuff",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "test".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foobar"] = "John",
                    },
                    Query = "foobar",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "foobar".Length
                };
                
                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foo bar"] = "John",
                    },
                    Query = "foo bar",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "foo bar".Length
                };
                
                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foo bar"] = "John",
                    },
                    Query = "foo \tbar",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "foo \tbar".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foobar"] = "John",
                    },
                    Query = "foo bar",
                    ShouldBeFound = true,
                    ExpectedValue = "Jane",
                    ExpectedReadLength = "foo".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foo bar"] = "John",
                    },
                    Query = "foo bar",
                    ShouldBeFound = true,
                    ExpectedValue = "John",
                    ExpectedReadLength = "foo bar".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foobar"] = "John",
                    },
                    Query = "foo bar and even more stuff",
                    ShouldBeFound = true,
                    ExpectedValue = "Jane",
                    ExpectedReadLength = "foo".Length
                };

                yield return new TestCase
                {
                    CaseSensitive = true,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foobar"] = "John",
                    },
                    Query = "Foo and even more stuff",
                    ShouldBeFound = false,
                    ExpectedReadLength = 0
                };

                yield return new TestCase
                {
                    CaseSensitive = false,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foobar"] = "John",
                    },
                    Query = "Foo and even more stuff",
                    ShouldBeFound = true,
                    ExpectedReadLength = "foo".Length,
                    ExpectedValue = "Jane"
                };

                yield return new TestCase
                {
                    CaseSensitive = false,
                    Map = new Dictionary<string, Person>
                    {
                        ["foo"] = "Jane",
                        ["bar"] = "Jack",
                        ["foo bar baaaaaaz"] = "John",
                    },
                    Query = "foo bar baz",
                    ShouldBeFound = true,
                    ExpectedReadLength = "foo".Length,
                    ExpectedValue = "Jane"
                };
            }
        }

        public class TestCase
        {
            public bool CaseSensitive { get; set; }
            public IReadOnlyDictionary<string, Person> Map { get; set; }
            public string Query { get; set; }
            public int StartPosition { get; set; }

            public bool ShouldBeFound { get; set; }
            public Person ExpectedValue { get; set; }
            public int ExpectedReadLength { get; set; }

            public override string ToString()
            {
                return $"Query: \"{Query}\", " +
                       $"Map: [{string.Join(';', Map.Select(x => $"\"{x.Key}\"=>\"{x.Value}\""))}], " +
                       $"StartPosition: \"{StartPosition}\", " +
                       $"CaseSensitive: {CaseSensitive}, " +
                       $"ShouldBeFound: {ShouldBeFound}, " +
                       $"ExpectedValue: \"{ExpectedValue}\", " +
                       $"ExpectedReadLength: {ExpectedReadLength}";
            }
        }

        public class Person
        {
            public string Name { get; private set; }

            public static implicit operator Person(string name)
            {
                return new Person
                {
                    Name = name
                };
            }

            public override string ToString()
            {
                return Name;
            }

            private bool Equals(Person other)
            {
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Person) obj);
            }

            public override int GetHashCode()
            {
                return (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}