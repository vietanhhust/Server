using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerAPI.Controllers.Services
{
    public class PasswordService
    {
        public PasswordService()
        {

        }

        // Hash MD5 password
        public string PasswordHash(string input)
        {
            input = input.ToLower(); 
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        // So sánh mật khẩu
        public bool PasswordCheck(string input1, string input2)
        {
            return input1 == input2;
        }
    }

    public class PasswordModel{
        public string Password { get; set; }
    }
}
