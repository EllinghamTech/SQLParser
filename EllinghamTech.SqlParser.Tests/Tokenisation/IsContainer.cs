using EllinghamTech.SqlParser.Internal;
using EllinghamTech.SqlParser.Tests.Extensions;
using NUnit.Framework;

namespace EllinghamTech.SqlParser.Tests.Tokenisation
{
    /// <summary>
    /// These tests are designed to test the private method IsContainer within the tokeniser itself.
    /// </summary>
    public class IsContainer
    {
        [TestCase('\'')]
        [TestCase('"')]
        [TestCase('(')]
        [TestCase('`')]
        public void FalseOnContainerCharacterWhenEscaped(char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_isEscaped", true);
            tokeniser.Send("_curChar", curChar);

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsFalse(isContainer);
        }

        [TestCase('\'')]
        [TestCase('"')]
        [TestCase('(')]
        [TestCase('`')]
        public void TrueOnContainerCharacter(char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_curChar", curChar);

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsTrue(isContainer);
        }

        [TestCase('\'', '\'')]
        [TestCase('"', '"')]
        [TestCase('(', ')')]
        [TestCase('`', '`')]
        public void TrueOnEndContainerCharacterWhenInContainer(char containerChar, char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_curChar", curChar);
            tokeniser.Send("_curContainer", containerChar);

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsTrue(isContainer);
        }

        [TestCase('a')]
        [TestCase('+')]
        [TestCase('-')]
        [TestCase('_')]
        [TestCase('<')]
        [TestCase('>')]
        [TestCase('@')]
        [TestCase('\0')]
        [TestCase('\n')]
        [TestCase('\t')]
        public void FalseOnCharacterThatIsNotAContainerCharacter(char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_curChar", curChar);

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsFalse(isContainer);
        }

        [TestCase('a')]
        [TestCase('+')]
        [TestCase('-')]
        [TestCase('_')]
        [TestCase('<')]
        [TestCase('>')]
        [TestCase('@')]
        [TestCase('\0')]
        [TestCase('\n')]
        [TestCase('\t')]
        public void FalseOnCharacterThatIsNotAContainerWhenInContainerCharacter(char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_curChar", curChar);
            tokeniser.Send("_curContainer", '"');

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsFalse(isContainer);
        }

        /// <summary>
        /// Note: this test only tests the case when the container has a different end character
        /// to the start character.
        /// </summary>
        /// <param name="curChar"></param>
        [TestCase(')')]
        [TestCase(']')]
        [TestCase('}')]
        public void FalseOnEndContainerCharacterWhenNotInContainer(char curChar)
        {
            Tokeniser tokeniser = new Tokeniser("");
            tokeniser.Send("_curChar", curChar);

            object isContainerRaw = tokeniser.Send("IsContainerCharacter");
            bool? isContainer = isContainerRaw as bool?;

            Assert.IsFalse(isContainer);
        }
    }
}
