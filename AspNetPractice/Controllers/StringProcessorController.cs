using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AspNetPractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class StringProcessorController : ControllerBase
    {
        [HttpPost]
        public IActionResult ProcessString([FromBody] string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return BadRequest("Input string cannot be empty");
            }

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

            return Ok(result);
        }
    }
}
