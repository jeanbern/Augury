using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Augury
{
    public static class Tokenizer
    {
        public static IEnumerable<string> ToLowerCase(this IEnumerable<string> words)
        {
            return words.Select(word => word.ToLowerInvariant());
        }

        //TODO: we should be able to do this without this much work.
        //Why is it even done this way? It's no faster than GetSentences.Last...
        public static List<string> GetLastSentence(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string> { "" };
            }

            if (IsSentenceEnder(text[text.Length - 1]))
            {
                return new List<string> { "" };
            }
            
            var res = GetSentences(text.Reverse());
            var enumerator = res.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return new List<string> { "" };
            }

            var result = enumerator.Current;
            if (result.Count == 0)
            {
                return new List<string> { "" };
            }

            result.Reverse();
            for (var x = 0; x < result.Count; x++)
            {
                result[x] = result[x].Reverse();
            }

            if (MayAsWellBeWhitespace(text[text.Length - 1]))
            {
                result.Add("");
            }

            return result;
        }

        //TODO: Making this use a stream instead of a giant string would be nice
        public static IEnumerable<List<string>> GetSentences(string text)
        {
            var builder = new StringBuilder();
            var inWord = false;
            var inSentence = false;
            var sentence = new List<string>();
            var apostropheHit = false;

            var previous = ' ';

            foreach (var character in text)
            {
                if (!inWord)
                {
                    if (IsWordCharacter(character))
                    {
                        inWord = true;
                        inSentence = true;
                        goto addCurrentCharacter;
                    }

                    if (IsSentenceEnder(character))
                    {
                        if (inSentence)
                        {
                            goto endSentence;
                        }
                    }

                    goto next;
                }

                if (character == '-')
                {
                    if (previous == '-')
                    {
                        goto endSentence;
                    }

                    if (apostropheHit)
                    {
                        goto endWord;
                    }

                    goto addCurrentCharacter;
                }

                if (character == '\'')
                {
                    if (apostropheHit)
                    {
                        goto endSentence;
                    }

                    apostropheHit = true;
                    goto addCurrentCharacter;
                }

                if (MayAsWellBeWhitespace(character))
                {
                    goto endWord;
                }

                if (IsWordCharacter(character))
                {
                    goto addCurrentCharacter;
                }

                if (IsSentenceEnder(character))
                {
                    goto endSentence;
                }

            next:
                {
                    previous = character;
                    continue;
                }

            endWord:
                {
                    while (builder.Length > 0 && IsSpecialCharacterThatCanBePartOfWord(builder[builder.Length - 1]))
                    {
                        builder.Remove(builder.Length - 1, 1);
                    }

                    sentence.Add(builder.ToString());
                    builder.Clear();
                    inWord = false;
                    apostropheHit = false;
                    goto next;
                }

            addCurrentCharacter:
                {
                    builder.Append(character);
                    goto next;
                }

            endSentence:
                {
                    while (builder.Length > 0 && IsSpecialCharacterThatCanBePartOfWord(builder[builder.Length - 1]))
                    {
                        builder.Remove(builder.Length - 1, 1);
                    }

                    if (builder.Length > 0)
                    {
                        sentence.Add(builder.ToString());
                    }

                    builder.Clear();
                    yield return sentence;
                    sentence = new List<string>();
                    inWord = false;
                    inSentence = false;
                    apostropheHit = false;
                    goto next;
                }
            }

            if (builder.Length > 0)
            {
                var word = builder.ToString();
                sentence.Add(word);
            }

            if (sentence.Count > 0)
            {
                yield return sentence;
            }
        }
        
        public static void ForwardTokenizeTogether(IEnumerable<string> sentence, IDictionary<string, uint> oneGrams, IDictionary<string[], uint> twoGrams, IDictionary<string[], uint> threeGrams)
        {
            var enumerator = sentence.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return;
            }

            var first = enumerator.Current;
            uint count;
            if (oneGrams.TryGetValue(first, out count))
            {
                oneGrams[first] = count + 1;
            }
            else
            {
                oneGrams[first] = 1;
            }

            if (!enumerator.MoveNext())
            {
                return;
            }

            var second = enumerator.Current;
            var twoGram = new[] { first, second };
            if (twoGrams.TryGetValue(twoGram, out count))
            {
                twoGrams[twoGram] = count + 1;
            }
            else
            {
                twoGrams[twoGram] = 1;
            }

            if (oneGrams.TryGetValue(second, out count))
            {
                oneGrams[second] = count + 1;
            }
            else
            {
                oneGrams[second] = 1;
            }

            var third = second;
            second = first;
            while (enumerator.MoveNext())
            {
                first = second;
                second = third;
                third = enumerator.Current;

                var threeGram = new[] { first, second, third };
                if (threeGrams.TryGetValue(threeGram, out count))
                {
                    threeGrams[threeGram] = count + 1;
                }
                else
                {
                    threeGrams[threeGram] = 1;
                }

                twoGram = new[] { second, third };
                if (twoGrams.TryGetValue(twoGram, out count))
                {
                    twoGrams[twoGram] = count + 1;
                }
                else
                {
                    twoGrams[twoGram] = 1;
                }

                if (oneGrams.TryGetValue(third, out count))
                {
                    oneGrams[third] = count + 1;
                }
                else
                {
                    oneGrams[third] = 1;
                }
            }
        }

        public static string CapitalizeFromTemplate(string template, string target, bool capitalizeFirst = false)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return string.Empty;
            }

            if (target.ToLowerInvariant() == "i")
            {
                return "I";
            }

            if (string.IsNullOrWhiteSpace(template))
            {
                return capitalizeFirst ? CapitalizeFirst(target) : target.ToLowerInvariant();
            }
            
            var start = template[0];
            var end = template.Skip(1).ToList();
            
            if (!char.IsUpper(start))
            {
                return capitalizeFirst ? CapitalizeFirst(target) : ToLowercase(target);
            }

            if (IsUpper(end) && !IsLower(end))
            {
                return ToUppercase(target);
            }

            if (capitalizeFirst || !char.IsLower(start))
            {
                return CapitalizeFirst(target);
            }

            return ToLowercase(target);
        }

        private static string CapitalizeFirst(string target)
        {
            return char.ToUpperInvariant(target[0]) + ToLowercase(target.Skip(1));
        }

        public static bool IsUpper(IEnumerable<char> input)
        {
            return input.All(char.IsUpper);
        }

        public static bool IsLower(IEnumerable<char> input)
        {
            return input.All(char.IsLower);
        }

        public static string ToUppercase(IEnumerable<char> input)
        {
            return new string(input.Select(char.ToUpperInvariant).ToArray());
        }

        public static string ToLowercase(IEnumerable<char> input)
        {
            return new string(input.Select(char.ToLowerInvariant).ToArray());
        }

        #region private methods

        internal static string Reverse(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // allocate a buffer to hold the output
            var output = new char[input.Length];
            for (int outputIndex = 0, inputIndex = input.Length - 1; outputIndex < input.Length; outputIndex++, inputIndex--)
            {
                // check for surrogate pair
                if (input[inputIndex] >= 0xDC00 && input[inputIndex] <= 0xDFFF &&
                    inputIndex > 0 && input[inputIndex - 1] >= 0xD800 && input[inputIndex - 1] <= 0xDBFF)
                {
                    // preserve the order of the surrogate pair code units
                    output[outputIndex + 1] = input[inputIndex];
                    output[outputIndex] = input[inputIndex - 1];
                    outputIndex++;
                    inputIndex--;
                }
                else
                {
                    output[outputIndex] = input[inputIndex];
                }
            }

            return new string(output);
        }

        internal static bool MayAsWellBeWhitespace(char character)
        {
            return char.IsWhiteSpace(character) || character == ',';
        }

        internal static bool IsWordCharacter(char character)
        {
            return Char.IsLetterOrDigit(character);
        }

        internal static bool IsSpecialCharacterThatCanBePartOfWord(char character)
        {
            return character == '\'' || character == '-';
        }

        private static bool IsSentenceEnder(char character)
        {
            return !(IsWordCharacter(character) || MayAsWellBeWhitespace(character));
        }

        #endregion
    }
}
