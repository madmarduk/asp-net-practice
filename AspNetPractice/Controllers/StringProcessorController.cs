﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;

namespace AspNetPractice.Controllers
{
    public class TreeNode
    {
        public TreeNode(char data)
        {
            Data = data;
        }

        public char Data { get; set; }
        public TreeNode Left { get; set; }
        public TreeNode Right { get; set; }

        public void Insert(TreeNode node)
        {
            if (node.Data < Data)
            {
                if (Left == null)
                {
                    Left = node;
                }
                else
                {
                    Left.Insert(node);
                }
            }
            else
            {
                if (Right == null)
                {
                    Right = node;
                }
                else
                {
                    Right.Insert(node);
                }
            }            
        }
        
        public char[] Transform(List<char> elements = null)
        {
            if (elements == null)
            {
                elements = new List<char>();
            }

            if (Left != null)
            {
                Left.Transform(elements);
            }

            elements.Add(Data);

            if (Right != null)
            {
                Right.Transform(elements);
            }

            return elements.ToArray();
        }
    }


    [Route("api/[controller]")]
    [ApiController]   
    public class StringProcessorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static SemaphoreSlim _semaphore;
        public StringProcessorController(IConfiguration configuration)
        {
            _configuration = configuration;
            int parallelLimit = _configuration.GetValue<int>("Settings:ParallelLimit");
            if (_semaphore == null)
            {
                _semaphore = new SemaphoreSlim(parallelLimit, parallelLimit);
            }
        }
        


        [HttpGet]
        public async Task<IActionResult> ProcessString([FromQuery] string input = "string", [FromQuery] string sortAlgorithm = "quick")
        {
            if (!await _semaphore.WaitAsync(0))
            {
                return StatusCode(503, "Service Unavaliable");

            }


            char[] QuickSort(char[] input, int startIndex, int endIndex)
            {
                int i = startIndex;
                int j = endIndex;
                var pivot = input[startIndex];

                while (i <= j)
                {
                    while (input[i] < pivot)
                    {
                        i++;
                    }

                    while (input[j] > pivot)
                    {
                        j--;
                    }

                    if (i <= j)
                    {
                        (input[j], input[i]) = (input[i], input[j]);
                        i++;
                        j--;
                    }
                }

                if (startIndex < j) QuickSort(input, startIndex, j);
                if (i < endIndex) QuickSort(input, i, endIndex);

                return input;
            }


            char[] TreeSort(char[] input)
            {
                var treeNode = new TreeNode(input[0]);
                for (int i = 1; i < input.Length; i++)
                {
                    treeNode.Insert(new TreeNode(input[i]));
                }

                return treeNode.Transform();
            }

            try
            {
                if (string.IsNullOrEmpty(input))
                {
                    return BadRequest("Input string cannot be empty");
                }

                string[] blackList = _configuration.GetSection("Settings:BlackList").Get<string[]>();
                // var RandomAPI = _configuration.GetValue<string>("RandomAPI");

                foreach (string word in blackList)
                {
                    if (input == word)
                    {
                        return BadRequest("Blacklisted word");
                    }

                }

                // Symbols check
                char[] alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
                string badSymbols = string.Empty;
                for (int i = 0; i < input.Length; i++)
                {
                    if (!alphabet.Contains(input[i]))
                    {
                        badSymbols += input[i];
                    }
                }

                if (!string.IsNullOrEmpty(badSymbols))
                {
                    return BadRequest($"Input string should only contain lower case latin letters. Error in next symbols: {badSymbols}");
                }

                // String processing
                string result;
                // Если строка будет иметь чётное количество символов, то программа должна разделить её на
                // две подстроки, каждую подстроку перевернуть и соединять обратно обе подстроки в одну строку.
                if (input.Length % 2 == 0)
                {
                    result = new string(input.Substring(0, input.Length / 2).Reverse().ToArray());
                    result += new string(input.Substring(input.Length / 2).Reverse().ToArray());
                }
                // Если входная строка будет иметь нечётное количество символов, то программа должна перевернуть
                // эту строку и к ней добавить изначальную строку, которую ввёл пользователь.
                else
                {
                    result = new string(input.Reverse().ToArray()) + input;
                }

                // Самая длинная подстрока начинающаяся и заканчивающаяся на гласную
                List<int> vowelIndexes = new List<int>(2);

                for (int i = 0; i < result.Length; i++)
                {
                    if ("aeiouy".Contains(result[i]))
                    {
                        vowelIndexes.Add(i);
                        break;
                    }
                }

                for (int i = result.Length - 1; i >= 0; i--)
                {
                    if ("aeiouy".Contains(result[i]))
                    {
                        vowelIndexes.Add(i);
                        break;
                    }

                }

                var vowelsSubstring = result.Substring(vowelIndexes[0], vowelIndexes[1] - vowelIndexes[0] + 1);

                // Информация о том, сколько раз входил в обработанную строку каждый символ
                var symbolsDictionary = new Dictionary<char, int>();

                foreach (char x in result)
                {
                    if (!symbolsDictionary.ContainsKey(x))
                    {
                        symbolsDictionary.Add(x, 1);
                    }
                    else
                    {
                        symbolsDictionary[x]++;
                    }

                }

                string symbolsInfoString = string.Empty;

                foreach (var x in symbolsDictionary)
                {
                    symbolsInfoString += $"Symbol: {x.Key}, Times: {x.Value}\n";
                }


                string sortedResult;

                if (sortAlgorithm.ToLower() == "quick")
                {
                    sortedResult = new string(QuickSort(result.ToCharArray(), 0, result.Length - 1));
                }
                else if (sortAlgorithm.ToLower() == "tree")
                {
                    sortedResult = new string(TreeSort(result.ToCharArray()));
                }
                else
                {
                    return BadRequest("Invalid sort algorithm. Use 'quick' or 'tree'");
                }

                // random number
                int stringLen = result.Length - 1;
                int randomNumber;
                var RandomAPI = _configuration.GetValue<string>("RandomAPI");
                using (HttpClient client = new HttpClient())
                {

                    HttpResponseMessage response = await client.GetAsync($"{RandomAPI}{stringLen}");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        int[] randomNumbers = System.Text.Json.JsonSerializer.Deserialize<int[]>(responseBody);
                        randomNumber = randomNumbers[0];
                    }
                    else
                    {
                        Random rnd = new Random();
                        randomNumber = rnd.Next(0, stringLen);
                    }
                }

                string cutResult = result.Remove(randomNumber, 1);

                var displayResult = new
                {
                    ProccessedString = result,
                    SymbolCounts = symbolsDictionary.Select(kvp => new { Symbol = kvp.Key, Count = kvp.Value }),
                    VowelsSubstring = vowelsSubstring,
                    SortedString = sortedResult,
                    CutString = cutResult

                };
                //var displayResult = $"{result} \n{symbolsInfoString}Подстрока с началом и концом из гласной: {vowelsSubstring}\nОтсортированная строка: {sortedResult}\nУрезанная обработанная строка: {cutResult}".ToArray();


                return Ok(displayResult);
            }

            finally
            {
                _semaphore.Release();
            }


            

        }       
        
    }
}
