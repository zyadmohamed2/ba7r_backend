
using Ba7rApp.Api.Dto;
using Ba7rApp.Domain.Enum;
using Ba7rApp.Domain.Models;
using Ba7rApp.Repository;
using Ba7rApp.Repository.IRepository;
using Ba7rApp.Services.CategoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.OpenApi.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Ba7rApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountUserController : ControllerBase
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IHomeSeaRepository homeSeaRepository;
        public AccountUserController(IWebHostEnvironment hostingEnvironment, IUnitOfWork unitOfWork,IHomeSeaRepository homeSeaRepository)
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


        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUp([FromForm] SignUpDto model)
        {
            string uniqueFileName = ProcessUploadFile(model.Photo);

            bool emailExists = await unitOfWork.Repository<User>().AnyAsync(u => u.Email == model.Email);
            bool emailExists1 = await unitOfWork.Repository<BusinessAccount>().AnyAsync(u => u.Email == model.Email);
            if (emailExists || emailExists1)
                return BadRequest(new { message = "Email already exists" });


            bool phoneExists = await unitOfWork.Repository<User>().AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
            bool phoneExists1 = await unitOfWork.Repository<BusinessAccount>().AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (phoneExists || phoneExists1)
                return BadRequest(new { message = "Phone Number already exists" });

            var user = new User
            {
                PhotoPath = uniqueFileName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                Address = model.Address,
                Categories = homeSeaRepository.GetCategoriesByName(model.Categories),
                Points = 0,
                Code = homeSeaRepository.generateuniquecode(),
                CreationTime = DateTime.UtcNow.ToLocalTime()
            };

            if (await unitOfWork.Repository<User>().Add(user))
            {
                await unitOfWork.Complete();
                return StatusCode(200, new { message = "User Added Successfully", Id = user.Id });
            }
            else
            {
                return StatusCode(400, new { message = "User Not Added " });
            }
        }

        [HttpGet]
        [Route("CheckEmail")]
        public async Task<IActionResult> CheckEmail([FromQuery] CheckDto model)
        {

            bool emailExists = await unitOfWork.Repository<User>().AnyAsync(u => u.Email == model.Value);
            bool emailExists1 = await unitOfWork.Repository<BusinessAccount>().AnyAsync(u => u.Email == model.Value);
            if (emailExists || emailExists1)
                return Ok(new { message = true });

            return BadRequest(new { message = false });

        }

        [HttpGet]
        [Route("CheckPhone")]
        public async Task<IActionResult> CheckPhone([FromQuery] CheckDto model)
        {

            bool phoneExists = await unitOfWork.Repository<User>().AnyAsync(u => u.PhoneNumber == model.Value);
            bool phoneExists1 = await unitOfWork.Repository<BusinessAccount>().AnyAsync(u => u.PhoneNumber == model.Value);
            if (phoneExists || phoneExists1)
                return Ok(new { message = true });

            return BadRequest(new { message = false });

        }

        [HttpGet]
        [Route("GetProfile")]
        public async Task<IActionResult> GetProfile(int UserId) 
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if (user == null)
                return BadRequest(new { message = "User Not Found" });

            var userDto = new ProfileDto
            {
                PhotoPath = homeSeaRepository.Path().ToString() + "images/"+user.PhotoPath,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };
            return Ok(userDto);
        }


        [HttpPut]
        [Route("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto model,int UserId)
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if (user == null)
                return BadRequest(new { message = "User Not Found" });


                bool emailExists = await unitOfWork.Repository<User>().AnyAsync(u => u.Email == model.Email && u.Id != UserId);
                bool emailExistsInBusinessAccount = await unitOfWork.Repository<BusinessAccount>().AnyAsync(u => u.Email == model.Email);
                if (emailExists || emailExistsInBusinessAccount)
                    return BadRequest(new { message = "Email already exists" });

            user.FirstName = model.FirstName;
            user.Email = model.Email;
            user.LastName = model.LastName;
            user.Gender = (Gender)model.Gender;
            if (!string.IsNullOrWhiteSpace(model.Email))
                user.Email = model.Email;

            user.PhoneNumber = user.PhoneNumber;

            if(model.Photo != null)
            {
                string FilePath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.Photo.ToString());
                System.IO.File.Delete(FilePath);
            }
            user.PhotoPath = ProcessUploadFile(model.Photo);

             var result = await  unitOfWork.Repository<User>().Update(user);
            if (!result)
                return BadRequest(new { message = "Profile Can't Updated " });

            await unitOfWork.Complete();
            return Ok(new { message ="Profile Updated Successfully"});
        }


        [HttpDelete]
        [Route("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount(int UserId)
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if (user == null)
                return BadRequest(new { message = "User Not Found" });


            await unitOfWork.Repository<User>().Delete(user);
            await unitOfWork.Complete();
            return StatusCode(200, new { message = "User Deleted Successfully" });


        }

        [HttpGet]
        [Route("GetUserPoints")]
        public async Task<IActionResult> GetUserPoints (int UserId)
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if (user == null)
                return BadRequest(new { message = "User Not Found" });

            string userStatus;

            if (user.Points < 600)
            {
                userStatus = "Bronze";
            }
            else if (user.Points < 1000)
            {
                userStatus = "Silver";
            }
            else
            {
                userStatus = "Gold";
            }

            string userImage = homeSeaRepository.Path().ToString() + "images/" + user.PhotoPath;
            string userFullName = user.FirstName + " " + user.LastName;

            return Ok(new { Points = user.Points , Status = userStatus , UserImage = userImage , UserName = userFullName });
        }

        [HttpGet]
        [Route("GetTopUser")]
        public IActionResult GetTopUser()
        {
            var topUsers = unitOfWork.Repository<User>().GetAll().OrderByDescending(user => user.Points).Take(3) 
                .Select(user => new
        {
           Image = homeSeaRepository.Path() + "images/" + user.PhotoPath,
           UserName = user.FirstName +" "+user.LastName,
           Status = homeSeaRepository.GetUserStatus(user.Points)
        })
        .ToList();

            if (topUsers.Count() == 0)
                return NotFound(new { message = "No users found" });

            return Ok(topUsers);
           
        }

        [HttpGet]
        [Route("GetAllCoupons")]
        public IActionResult GetAllCoupons()
        {
            var coupons =  unitOfWork.Repository<Coupon>().GetAll()
                .Where(c => c.IsAvailable == true &&  c.ValidTime > DateTime.UtcNow.ToLocalTime());

            var result = coupons.Select(coupon => new
            {
                Id = coupon.Id,
                Image = homeSeaRepository.Path() + "iamges/" + coupon.PhotoPath,
                Type = coupon.CouponType.GetDisplayName(),
                Title = coupon.Value + "%" + " " + "OFF",
                Name = coupon.Name,
                TimeString = "Valid until " + coupon.ValidTime.ToShortDateString(),
            });

            return Ok(result);
        }


        [HttpGet]
        [Route("GetCoupon")]
        public async Task<IActionResult> GetCouponTrip(int CouponId)
        {

            var coupon = await unitOfWork.Repository<Coupon>().GetById(CouponId);
            if (coupon != null && coupon.IsAvailable == true && coupon.ValidTime > DateTime.UtcNow.ToLocalTime())
            {
                var couponData = new 
                {

                    Title1 = coupon.Value + "%" + " " + "OFF",
                    Image = homeSeaRepository.Path() + "iamges/" + coupon.PhotoPath,
                    Name = coupon.Name,
                    Type = coupon.CouponType.GetDisplayName(),
                    Title2 = $"Get {coupon.Value}% at your next {coupon.Name} Registeration ",
                    Descriptions = homeSeaRepository.GetCouponDescriptions(CouponId).Select(a => a.Description).ToList(),
                    TimeString = "Valid untill " + coupon.ValidTime.ToShortDateString(),
                    Code = coupon.Code,
                };

                return Ok(couponData);
            }

            return BadRequest(new { message = "An error occur" });

        }

        [HttpPost]
        [Route("BuyCoupon")]
        public async Task<IActionResult> BuyCoupon(int UserId, int CouponId)
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if (user == null)
                return BadRequest(new { message = "User Not Exist" });

            var coupon = await unitOfWork.Repository<Coupon>().GetById(CouponId);
            if (coupon == null)
                return BadRequest(new { message = "Coupon Not Exist" });

            if (user.Points >= coupon.PointsRequired && coupon.IsAvailable == true )
            {
                user.Points = user.Points - coupon.PointsRequired;
                var couponUser = new CouponUser
                {
                    UserId = UserId,
                    CouponId  = coupon.Id,

                };
                var result = await unitOfWork.Repository<CouponUser>().Add(couponUser);
                if (!result)
                    return BadRequest(new { message = "An error occur" });

                await unitOfWork.Repository<User>().Update(user);
                await unitOfWork.Complete();
                return Ok(new { message = "You buy this coupon successfully" });
            }

            return BadRequest(new { message = "Your can't buy this coupon" });
        }

        [HttpGet]
        [Route("ViewCoupon")]
        public async Task<IActionResult> ViewCoupon(int CouponId)
        {

            var coupon = await unitOfWork.Repository<Coupon>().GetById(CouponId);
            if (coupon == null)
                return BadRequest(new { message = "Coupon Not Exist" });

            return Ok(new { code = coupon.Code });
        }


        [HttpGet]
        [Route("ViewCode")]
        public IActionResult UseCode(int UserId)
        {
            var user = unitOfWork.Repository<User>().GetById(UserId).Result;
            if (user == null)
                return BadRequest(new { message = "This User Not Found" });

            return Ok(new { Code = user.Code });
        }


        [HttpPut]
        [Route("UseCode")]
        public async Task<IActionResult> UseCode(int UserId , string Code)
        {
            var user = await unitOfWork.Repository<User>().GetById(UserId);
            if(user == null)
                return BadRequest(new { message = "This User Not Found" });


            var userCode = homeSeaRepository.GetUserByCode(Code);
            if (userCode == null)
                return BadRequest(new { message = "This Code Not Found" });

            userCode.Points += 25;
            user.Points += 25;
            await unitOfWork.Repository<User>().Update(user);
            await unitOfWork.Repository<User>().Update(userCode);
            await unitOfWork.Complete();
            return Ok(new {message ="You Used This Code Successfully"});
        }

    }
}