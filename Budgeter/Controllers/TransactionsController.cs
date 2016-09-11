﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Budgeter.Models;
using Microsoft.AspNet.Identity;

namespace Budgeter.Controllers
{
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        [Authorize]
        [AuthorizeHouseholdRequired]
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Account not found" });
            }
            //check if the user is authorized to view this account
            var helper = new AccountUserHelper();
            var user = User.Identity.GetUserId();
            if (helper.CanUserAccessAccount(user, (int)id) == false)
            {
                return RedirectToAction("Index", "Errors", new { errorMessage = "Not authorized" });
            }
            //find the transactions for this account
            var transactions = db.Transactions.Where(x => x.AccountId == id).ToList();
            //get the account name and put it in the ViewBag
            ViewBag.AccountName = account.Name;
            //get the account balance
            var accountBalance = GetAccountBalance(transactions);
            //get the reconciled balance
            var accountRecBalance = GetReconciledAccountBalance(transactions);
            //pass the balances in the ViewBag
            ViewBag.Balance = accountBalance;
            ViewBag.Reconciled = accountRecBalance;

            //transform the transactions so we can show them in the index page
            var transactionsToShow = new List<TransactionsIndexViewModel>();
            foreach(var t in transactions)
            {
                var transToShow = new TransactionsIndexViewModel(t);
                transactionsToShow.Add(transToShow);
            }
            
            return View(transactionsToShow);
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,AccountId,Description,DateEntered,DateSpent,Amount,Type,CategoryId,EnteredById,SpentById,IsReconciled,ReconciledAmount")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,AccountId,Description,DateEntered,DateSpent,Amount,Type,CategoryId,EnteredById,SpentById,IsReconciled,ReconciledAmount")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public Decimal GetAccountBalance(List<Transaction> transactions)
        {
            Decimal total = 0;
            foreach (var t in transactions)
            {
                total += t.Amount;
            }
            return total;
        }

        public Decimal GetReconciledAccountBalance(List<Transaction> transactions)
        {
            Decimal recTotal = 0;
            foreach (var t in transactions)
            {
                recTotal += t.ReconciledAmount;
            }
            return recTotal;
        }
    }
}