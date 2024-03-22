using Ba7rApp.Api.Dto;
using Ba7rApp.Domain.Models;
using Ba7rApp.Repository.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;

namespace Ba7rApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountAdminController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHomeSeaRepository homeSeaRepository;
        public AccountAdminController(IWebHostEnvironment hostingEnvironment, IUnitOfWork unitOfWork, IHomeSeaRepository homeSeaRepository)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.unitOfWork = unitOfWork;
            this.homeSeaRepository = homeSeaRepository;
        }
        private string ProcessUploadFile(IFormFile Photo)
        {
            string uniqueFileName = null;
            if (Photo != null)
            {
                string uploadFile = Path.Combine(hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Photo.FileName;
                string filePath = Path.Combine(uploadFile, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        private static string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            // Create the hash value
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine the salt and hash
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Convert the combined salt and hash to a string
            string hashedPassword = Convert.ToBase64String(hashBytes);

            return hashedPassword;
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            // Extract the bytes from the hashed password
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            // Get the salt from the hashBytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute the hash of the provided password
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Compare the computed hash with the stored hash
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            int verificationCode = random.Next(100000, 999999);
            return verificationCode.ToString();
        }

        private void SendVerificationCode(string email, string verificationCode)
        {
            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("bar377344@gmail.com", "hpsqjmcoejganhgh");

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("bar377344@gmail.com"),
                    Subject = "Password Reset Verification Code",
                    Body = $"Your verification code is: {verificationCode}"
                };
                mailMessage.To.Add(email);

                client.Send(mailMessage);
            }
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUpDto([FromForm] AdminDto model)
        {
            string uniqueFileName = ProcessUploadFile(model.Photo);
            bool emailExist = await unitOfWork.Repository<Admin>().AnyAsync(u => u.Email == model.Email);
            if (emailExist)
                return BadRequest(new { message = "Email already exists" });


            var admin = new Admin
            {
                PhotoPath = uniqueFileName,
                Name = model.Name,
                Email = model.Email,
                HashPassword = HashPassword(model.Password),

            };

            if (await unitOfWork.Repository<Admin>().Add(admin))
            {
                await unitOfWork.Complete();
                return StatusCode(200, new { message = "Admin Added Successfully", Id = admin.Id });
            }
            else
            {
                return StatusCode(400, new { message = "Admin Not Added " });
            }
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login([FromForm]LoginDto model)
        {
            var admin = homeSeaRepository.GetAdminByEmail(model.Email);
            if (admin == null || !VerifyPassword(model.Password, admin.HashPassword))
                return BadRequest(new { message = "Invalid email or password" });

            return Ok(new { message = "Admin logged in successfully", Id = admin.Id });
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public IActionResult ForgotPassword([FromForm]CheckDto model)
        {
            
            var admin = homeSeaRepository.GetAdminByEmail(model.Value);
            if (admin == null)
                return BadRequest(new { message = "No Admin Found" });
            
            
            string verificationCode = GenerateVerificationCode();

            //admin.VerificationCode = verificationCode;
            //await unitOfWork.Repository<Admin>().Update(admin);
            //await unitOfWork.Complete();
 
            SendVerificationCode(model.Value, verificationCode);

            return Ok(new { message = "Verification code sent" , Code = verificationCode});
        }

        //[HttpPost("CheckCode")]
        //public async Task<IActionResult> CheckCode(int AdminId,string Code)
        //{
           
        //    var user = await unitOfWork.Repository<Admin>().GetById(AdminId);
        //    if (user == null)
        //        return BadRequest(new { message = "No Admin Found" });


        //    if (user.VerificationCode != Code)
        //        return BadRequest(new { message = "Invalid Code" });


        //    return Ok(new { message = "Valid Code" });
        //}



        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm]ResetPasswordDto model, int AdminId )
        {
            var admin = await unitOfWork.Repository<Admin>().GetById(AdminId);
            if (admin == null)
                return BadRequest(new { message = "No Admin Found" });


            admin.HashPassword = HashPassword(model.Password);
            //admin.VerificationCode = null;
            await unitOfWork.Repository<Admin>().Update(admin);
            await unitOfWork.Complete();

            return Ok(new { message = "Password Reset Successfully" });
        }

    }
}
