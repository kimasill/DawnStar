using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        public AccountController(AppDbContext db)
        {
            _context = db;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRes CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRes res = new CreateAccountPacketRes();
            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName)
                .FirstOrDefault();

            if (account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    AccountName = req.AccountName,
                    Password = req.Password
                });
                bool success = _context.SaveChangesEx();
                res.CreateSuccess = success;
            }
            else
            {
                res.CreateSuccess = false;
            }
            return res;   
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRes LoginAccount([FromBody]LoginAccountPacketReq req)
        {
            LoginAccountPacketRes res = new LoginAccountPacketRes();
            
            AccountDb account = _context.Accounts
                .AsNoTracking()
                .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                .FirstOrDefault();

            if (account == null)
            {
                res.LoginSuccess = false;
            }
            else 
            {
                res.LoginSuccess = true;

                res.ServerList = new List<ServerInfo>()
                {
                    new ServerInfo(){Name = "카단", IP = "127.0.0.1", CrowdedLevel = 0 },
                    new ServerInfo(){Name = "루페온", IP = "127.0.0.1", CrowdedLevel = 3 }
                };
            }
            return res;
        }
    }
}
