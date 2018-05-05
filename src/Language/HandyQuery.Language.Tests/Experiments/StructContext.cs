using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;
using NUnit.Framework;

namespace HandyQuery.Language.Tests.Experiments
{
    public unsafe class StructContext
    {
        /// <summary>
        /// Provides stack memory to be used.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        internal ref struct ContextMemory
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            [FieldOffset(0)] private fixed byte _bytes[8];
        }
        
        [StructLayout(LayoutKind.Explicit)]
        internal ref struct SomeContext
        {
            [FieldOffset(0)] public int FirstItem;
            [FieldOffset(4)] public int SecondItem;
        }
        
        [Test]
        public void T()
        {
            var contextMemory = new ContextMemory();

            var context = (SomeContext*)&contextMemory;
            context->FirstItem = 10;
            context->SecondItem = 20;

            context->FirstItem.Should().Be(10);
            context->SecondItem.Should().Be(20);
            (*context).FirstItem.Should().Be(10);
            (*context).SecondItem.Should().Be(20);
            ((SomeContext*) &contextMemory)->FirstItem.Should().Be(10);
            ((SomeContext*) &contextMemory)->SecondItem.Should().Be(20);
        }
    }
}