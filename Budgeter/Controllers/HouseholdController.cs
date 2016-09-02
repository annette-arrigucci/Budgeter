using Budgeter.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Budgeter.Controllers
{
    public class HouseholdController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        

        // GET: Households
        [Authorize]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var householdMembersInfo = new List<UserInfoViewModel>();
            if (user.HouseholdId != null) { 
                var householdId = user.HouseholdId;
                var householdMembers = db.Users.Where(x => x.HouseholdId == householdId).ToList();
                foreach(var member in householdMembers)
                {
                    var uinfo = new UserInfoViewModel
                    {
                        DisplayName = member.FirstName + " " + member.LastName,
                        Email = member.Email,
                        UserId = member.Id
                    };
                    householdMembersInfo.Add(uinfo);
                }
            }          
            return View(householdMembersInfo);
        }

        //GET: Households/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        //POST: Households/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name")]Household model)
        {
            if (ModelState.IsValid)
            {
                //create the household in the database
                model.Code = GetRandomString();
                db.Households.Add(model);
                db.SaveChanges();

                //find the current user in the database
                //get the HouseholdId from the household that was added
                var uId = User.Identity.GetUserId();
                
                var household = db.Households.First(x => x.Name.Equals(model.Name));
                var hId = household.Id;
                AssignUserToHousehold(uId, hId);

                return RedirectToAction("Dashboard", "Home");
            }
            return View(model);
        }

        //assign the user the HouseholdId - need to use UserManager for this
        //this will overwrite any other HouseholdId that the user has been assigned to
        public async Task AssignUserToHousehold(string userId, int householdId)
        {       
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            ApplicationUser user = userManager.FindById(userId);
            user.HouseholdId = (int)householdId;
            IdentityResult result = await userManager.UpdateAsync(user);
        }

        public string GetRandomString()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 5)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        //POST: Households/InviteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InviteUser(string email, int householdId)
        {
            if (ModelState.IsValid)
            {
                Household hhold = db.Households.Find(householdId);
                if (hhold == null)
                {
                    return RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
                }
                if (string.IsNullOrEmpty(email))
                {
                    return RedirectToAction("Index", "Errors", "Email address empty");
                }
                else
                {
                    //use utilities to see if email is valid
                    var emailChecker = new RegexUtilities();
                    if (!emailChecker.IsValidEmail(email))
                    {
                        return RedirectToAction("Index", "Errors", "Email address not valid");
                    }
                    //if the email address is valid, send the email
                    SendEmailInvitation(email, householdId);
                    return RedirectToAction("Dashboard","Home");

                }
            }
            return View();
        }

        public async Task SendEmailInvitation(string email, int householdId)
        {
            Household hhold = db.Households.Find(householdId);
            if (hhold == null)
            {
                RedirectToAction("Index", "Error", new { errorMessage = "Not Found" });
            }
            var callbackUrl = "http://aarrigucci-budgeter.azurewebsites.net/Household/Join/" + householdId;
            string subject = "Invitation to join Budgeter household";
            var emailCode = hhold.Code;
            string message = "You have been invited to join the " + hhold.Name + " household on the Budgeter application. Click <a href=\"" + callbackUrl + "\" target=\"_blank\">here</a> to join. If you don’t have an account, you will be prompted to create one. Enter the code " + emailCode + ".";
            
            var es = new EmailService();
            es.SendAsync(new IdentityMessage
            {
                Destination = email,
                Subject = subject,
                Body = message
            });
        }

        //Post
        public ActionResult Join()
        {
            return View();
        }

        //Post
        public ActionResult Leave()
        {
            return View();
        }
    }
}