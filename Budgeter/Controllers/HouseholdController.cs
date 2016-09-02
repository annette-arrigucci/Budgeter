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
                        Email = member.Email
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

        //Post
        public ActionResult InviteUser()
        {
            return View();
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