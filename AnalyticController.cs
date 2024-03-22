using Ba7rApp.Api.Dto;
using Ba7rApp.Domain.Enum;
using Ba7rApp.Domain.Models;
using Ba7rApp.Repository.IRepository;
using Ba7rApp.Repository.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using System.Linq;

namespace Ba7rApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IHomeSeaRepository homeSeaRepository;

        public AnalyticController(IUnitOfWork unitOfWork , IHomeSeaRepository homeSeaRepository)
        {
            this.unitOfWork = unitOfWork;
            this.homeSeaRepository = homeSeaRepository;

        }

        [HttpPut]
        [Route("IncreaseVisit")]
        public async Task<IActionResult> IncreaseVisit()
        {
            var analtics = await unitOfWork.Repository<Analytics>().GetById(1);
            if (analtics == null)
                return BadRequest(new { message = "Analtyics Not Found" });


            analtics.TotalVist = analtics.TotalVist + 1;

            var result = await unitOfWork.Repository<Analytics>().Update(analtics);
            if (!result)
                return BadRequest(new { message = false });

            await unitOfWork.Complete();
            return Ok(new { message = true });
        }

        [HttpPut]
        [Route("IncreaseSession")]
        public async Task<IActionResult> IncreaseSession()
        {
            var analtics = await unitOfWork.Repository<Analytics>().GetById(1);
            if (analtics == null)
                return BadRequest(new { message = "Analtyics Not Found" });


            analtics.OnlineSession = analtics.OnlineSession + 1;

            var result = await unitOfWork.Repository<Analytics>().Update(analtics);
            if (!result)
                return BadRequest(new { message = false });

            await unitOfWork.Complete();
            return Ok(new { message = true });
        }

        [HttpPut]
        [Route("DecreaseSession")]
        public async Task<IActionResult> DecreaseSession()
        {
            var analtics = await unitOfWork.Repository<Analytics>().GetById(1);
            if (analtics == null)
                return BadRequest(new { message = "Analtyics Not Found" });

            if (analtics.OnlineSession < 1)
                return BadRequest(new { message = "Online Session Less Than 1" });

            analtics.OnlineSession = analtics.OnlineSession - 1;

            var result = await unitOfWork.Repository<Analytics>().Update(analtics);
            if (!result)
                return BadRequest(new { message = false });

            await unitOfWork.Complete();
            return Ok(new { message = true });
        }

        [HttpGet]
        [Route("TotalVisit")]
        public async Task<IActionResult> TotalVisit()
        {
            var analtics = await unitOfWork.Repository<Analytics>().GetById(1);
            if (analtics == null)
                return BadRequest(new { message = "Analtyics Not Found" });

            return Ok(new { TotalVisit = analtics.TotalVist });
        }

        [HttpGet]
        [Route("OnlineSessions")]
        public async Task<IActionResult> OnlineSessions()
        {
            var analtics = await unitOfWork.Repository<Analytics>().GetById(1);
            if (analtics == null)
                return BadRequest(new { message = "Analtyics Not Found" });

            return Ok(new { OnlineSessions = analtics.OnlineSession });
        }

        [HttpGet]
        [Route("TotalMoney")]
        public IActionResult TotalMoney()
        {
            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>().GetAll();

            var productPackage = packageUserList
                .GroupBy(pu => pu.PackageId)
                .Select(group => new
                {
                    PackageId = group.Key,
                    TotalPrice = group.Sum(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                }).Sum(group => group.TotalPrice);

            double totalMoney = training + trip + watersport + package + productPackage +
                safari + camping + eventLand  + recreationalLand + recreationalSea;

            return Ok(new { TotalMoney = totalMoney.ToString() });
        }

        [HttpGet]
        [Route("TotalOrder")]
        public IActionResult TotalOrder()
        {
            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Count();

            
            double totalOrder = training + trip + watersport + package  +
                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalOrder = totalOrder.ToString() });
        }

        [HttpGet]
        [Route("TotalUsers")]
        public IActionResult TotalUsers()
        {
            double users = unitOfWork.Repository<User>()
                .GetAll()
                .Count();

            return Ok(new { TotalUsers = users.ToString() });
        }

        [HttpGet]
        [Route("TotalUsersMonth")]
        public IActionResult TotalUsersMonth()
        {
            DateTime thirtyDaysAgo = DateTime.Today.AddDays(-30).ToLocalTime();
            DateTime today = DateTime.Today.AddDays(1).ToLocalTime();

            double users = unitOfWork.Repository<User>()
               .GetAll()
               .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
               .Count();


            return Ok(new { TotalUsers = users });
        }

        [HttpGet]
        [Route("TotalBusniessAccount")]
        public IActionResult TotalBusniessAccount()
        {
            double users = unitOfWork.Repository<BusinessAccount>()
                .GetAll()
                .Where(bu=>bu.AvailabilityStatus == AvailabilityStatus.Available)
                .Count();

            return Ok(new { TotalUsers = users });
        }

        [HttpGet]
        [Route("TotalBusniessAccountMonth")]
        public IActionResult TotalBusniessAccountMonth()
        {
            DateTime thirtyDaysAgo = DateTime.Today.AddDays(-30).ToLocalTime();
            DateTime today = DateTime.Today.AddDays(1).ToLocalTime();

            double users = unitOfWork.Repository<BusinessAccount>()
               .GetAll()
               .Where(bu => bu.AvailabilityStatus == AvailabilityStatus.Available && bu.CreationTime >= thirtyDaysAgo && bu.CreationTime < today)
               .Count();


            return Ok(new { TotalUsers = users });
        }


        [HttpGet]
        [Route("TotalMoneyLast12Hours")]
        public IActionResult TotalMoneyLast12Hours()
        {
            DateTime twelveHoursAgo = DateTime.Now.AddHours(-12).ToLocalTime();
            DateTime now = DateTime.Now.ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>()
                .GetAll()
                .Where(pu => pu.CreationTime >= twelveHoursAgo && pu.CreationTime <= now);

            var productPackageTotalPrice = packageUserList
                .Select(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                .Sum();

            double totalMoney = training + trip + watersport + package + productPackageTotalPrice +
                                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalMoney = totalMoney });
        }

        [HttpGet]
        [Route("TotalOrderLast12Hours")]
        public IActionResult TotalOrderLast12Hours()
        {
            DateTime twelveHoursAgo = DateTime.Now.AddHours(-12).ToLocalTime();
            DateTime now = DateTime.Now.ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= twelveHoursAgo && t.CreationTime <= now)
                .Count();


            double totalOrder = training + trip + watersport + package +
                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalOrder = totalOrder.ToString() });
        }

        [HttpGet]
        [Route("TotalMoneyToday")]
        public IActionResult TotalMoneyToday()
        {
            DateTime today = DateTime.Today.ToLocalTime();
            DateTime tomorrow = today.AddDays(1).ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>()
                .GetAll()
                .Where(pu => pu.CreationTime >= today && pu.CreationTime < tomorrow);

            var productPackageTotalPrice = packageUserList
                .Select(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                .Sum();

            double totalMoney = training + trip + watersport + package + productPackageTotalPrice +
                                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalMoney = totalMoney });
        }

        [HttpGet]
        [Route("TotalOrderToday")]
        public IActionResult TotalOrderToday()
        {
            DateTime today = DateTime.Today.ToLocalTime();
            DateTime tomorrow = today.AddDays(1).ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= today && t.CreationTime < tomorrow)
                .Count();


            double totalOrder = training + trip + watersport + package +
                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalOrder = totalOrder.ToString() });
        }

        [HttpGet]
        [Route("TotalMoneyYasterday")]
        public IActionResult TotalMoneyYasterday()
        {
            DateTime yesterday = DateTime.Today.AddDays(-1).ToLocalTime();
            DateTime today = DateTime.Today.ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>()
                .GetAll()
                .Where(pu => pu.CreationTime >= yesterday && pu.CreationTime < today);

            var productPackageTotalPrice = packageUserList
                .Select(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                .Sum();

            double totalMoney = training + trip + watersport + package + productPackageTotalPrice +
                                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalMoney = totalMoney });
        }

        [HttpGet]
        [Route("TotalOrderYasterday")]
        public IActionResult TotalOrderYasterday()
        {
            DateTime yesterday = DateTime.Today.AddDays(-1).ToLocalTime();
            DateTime today = DateTime.Today.ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= yesterday && t.CreationTime < today)
                .Count();


            double totalOrder = training + trip + watersport + package +
                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalOrder = totalOrder.ToString() });
        }

        [HttpGet]
        [Route("TotalMoneyMonth")]
        public IActionResult TotalMoneyMonth()
        {
            DateTime thirtyDaysAgo = DateTime.Today.AddDays(-30).ToLocalTime();
            DateTime today = DateTime.Today.AddDays(1).ToLocalTime(); 

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>()
                .GetAll()
                .Where(pu => pu.CreationTime >= thirtyDaysAgo && pu.CreationTime < today);

            var productPackageTotalPrice = packageUserList
                .Select(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                .Sum();

            double totalMoney = training + trip + watersport + package + productPackageTotalPrice +
                                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalMoney = totalMoney });
        }

        [HttpGet]
        [Route("TotalOrderMonth")]
        public IActionResult TotalOrderMonth()
        {
            DateTime thirtyDaysAgo = DateTime.Today.AddDays(-30).ToLocalTime();
            DateTime today = DateTime.Today.AddDays(1).ToLocalTime();

            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= thirtyDaysAgo && t.CreationTime < today)
                .Count();


            double totalOrder = training + trip + watersport + package +
                safari + camping + eventLand + recreationalLand + recreationalSea;

            return Ok(new { TotalOrder = totalOrder.ToString() });
        }

        [HttpGet]
        [Route("TotalDataLast7Days")]
        public IActionResult TotalDataLast7Days()
        {
            DateTime currentDay = DateTime.UtcNow.ToLocalTime();
            DateTime startOfWeek = currentDay.AddDays(-7).ToLocalTime();
            DateTime endOfWeek = startOfWeek.AddDays(7).ToLocalTime();
            

            double totalMoney = CalculateTotalMoneyForDay(startOfWeek, endOfWeek);
            double totalOrder = CalculateTotalOrderForDay(startOfWeek, endOfWeek);

            return Ok(new { TotalMoney = totalMoney , TotalOrder = totalOrder  });
        }

        [HttpGet]
        [Route("TotalDataLast7DaysDaily")]
        public IActionResult TotalDataLast7DaysDaily()
        {
            DateTime currentDate = DateTime.Today.ToLocalTime(); // Current date

            List<object> dailyData = new List<object>(); // To store data for each day

            for (int i = 0; i < 7; i++)
            {
                // Calculate start and end dates for the current day
                DateTime currentDateMinusDays = currentDate.AddDays(-i).ToLocalTime();
                DateTime nextDateMinusDays = currentDateMinusDays.AddDays(1).ToLocalTime();

                // Calculate total money earned and total order for current day
                double totalMoney = CalculateTotalMoneyForDay(currentDateMinusDays, nextDateMinusDays);
                double totalOrder = CalculateTotalOrderForDay(currentDateMinusDays, nextDateMinusDays);

                // Create a data object for current day
                var dayData = new
                {
                    Day = i + 1,
                    Date = currentDateMinusDays.ToString("yyyy-MM-dd"),
                    TotalMoney = totalMoney,
                    TotalOrder = totalOrder
                };

                // Add the data object for current day to the list
                dailyData.Add(dayData);
            }

            // Return the data for the last 7 days
            return Ok(dailyData);
        }

        private double CalculateTotalMoneyForDay(DateTime fromDate, DateTime toDate)
        {
            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Sum(t => t.TotalPrice);

            var packageUserList = unitOfWork.Repository<PackageUser>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate);

            var productPackageTotalPrice = packageUserList
                .Select(pu => unitOfWork.Repository<ProductPackage>().GetById(pu.PackageId).Result.Price)
                .Sum();



            double totalMoney = training + trip + watersport + package
                + safari + camping + eventLand + recreationalLand + recreationalSea + productPackageTotalPrice; // Add totals for other booking types...

            return totalMoney;
        }
        private double CalculateTotalOrderForDay(DateTime fromDate, DateTime toDate)
        {
            double training = unitOfWork.Repository<BookingTraining>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double trip = unitOfWork.Repository<BookingTrip>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double watersport = unitOfWork.Repository<BookingWaterSport>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double package = unitOfWork.Repository<BookingPackage>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double safari = unitOfWork.Repository<BookingSafariSport>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double camping = unitOfWork.Repository<BookingCamping>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double eventLand = unitOfWork.Repository<BookingEvent>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double recreationalLand = unitOfWork.Repository<BookingRecreationalLand>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();

            double recreationalSea = unitOfWork.Repository<BookingRecreationalSea>()
                .GetAll()
                .Where(t => t.CreationTime >= fromDate && t.CreationTime < toDate)
                .Count();





            double totalOrder = training + trip + watersport + package
                + safari + camping + eventLand + recreationalLand + recreationalSea; // Add totals for other booking types...

            return totalOrder;
        }

        [HttpGet]
        [Route("RecentBooking")]
        public IActionResult RecentBooking()
        {
            var training =  unitOfWork.Repository<BookingTraining>().GetAll()
                .Select(b => new { 
                    Id = b.Id,
                    Image = homeSeaRepository.Path()+"images/" + unitOfWork.Repository<TrainingPhoto>().GetAll().Where(photo=>photo.TrainingId == b.TrainingId).Select(photo=>photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<Training>().GetById(b.TrainingId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Training" ,
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();

            var trip = unitOfWork.Repository<BookingTrip>().GetAll()
                .Select(b => new {
                Id = b.Id,
                Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<TripPhoto>().GetAll().Where(photo => photo.TripId == b.TripId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                Title = unitOfWork.Repository<Trip>().GetById(b.TripId).Result.Title,
                Price = b.TotalPrice,
                CreatedAt = b.CreationTime,
                Type = "Trip",
                UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
            })
                .ToList();

            var watersport = unitOfWork.Repository<BookingWaterSport>().GetAll().Select(b => new {
                Id = b.Id,
                Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<WaterSportPhoto>().GetAll().Where(photo => photo.WaterSportId == b.WaterSportId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                Title = unitOfWork.Repository<WaterSport>().GetById(b.WaterSportId).Result.Title,
                Price = b.TotalPrice,
                CreatedAt = b.CreationTime,
                Type = "Water Sport",
                UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
            })
                .ToList();

            var package = unitOfWork.Repository<BookingPackage>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<PackagePhoto>().GetAll().Where(photo => photo.PackageId == b.packageId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<Package>().GetById(b.packageId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Package",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();
            var safari = unitOfWork.Repository<BookingSafariSport>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<SafariSportPhoto>().GetAll().Where(photo => photo.SafariSportId == b.SafariSportId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<SafariSport>().GetById(b.SafariSportId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Safai Sport",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();

            var camping = unitOfWork.Repository<BookingCamping>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<CampingPhoto>().GetAll().Where(photo => photo.CampingId == b.CampingId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<Camping>().GetById(b.CampingId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Camping",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();
            var eventLand = unitOfWork.Repository<BookingEvent>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<EventPhoto>().GetAll().Where(photo => photo.EventId == b.EventId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<Event>().GetById(b.EventId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Event",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();
            var recreationalLand = unitOfWork.Repository<BookingRecreationalLand>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<RecreationalLandPhoto>().GetAll().Where(photo => photo.RecreationalId == b.RecreationalId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<RecreationalLand>().GetById(b.RecreationalId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Recreational Land",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();
            var recreationalSea = unitOfWork.Repository<BookingRecreationalSea>().GetAll()
                .Select(b => new {
                    Id = b.Id,
                    Image = homeSeaRepository.Path() + "images/" + unitOfWork.Repository<RecreationalSeaPhoto>().GetAll().Where(photo => photo.RecreationalId == b.RecreationalId).Select(photo => photo.PhotoPath).FirstOrDefault(),
                    Title = unitOfWork.Repository<RecreationalSea>().GetById(b.RecreationalId).Result.Title,
                    Price = b.TotalPrice,
                    CreatedAt = b.CreationTime,
                    Type = "Recreational Sea",
                    UserName = unitOfWork.Repository<User>().GetById(b.UserId).Result.FirstName + " " + unitOfWork.Repository<User>().GetById(b.UserId).Result.LastName,
                    CouponValue = b.CouponValue == null ? 0 : b.CouponValue.Value,
                })
                .ToList();


            var allBookings = training.Concat(trip)
                                      .Concat(watersport)
                                      .Concat(package)
                                      .Concat(safari)
                                      .Concat(camping)
                                      .Concat(eventLand)
                                      .Concat(recreationalLand)
                                      .Concat(recreationalSea);

             var orderedBookings = allBookings.OrderByDescending(b => b.CreatedAt).ToList();

            return Ok(orderedBookings);
        }


    }
}
