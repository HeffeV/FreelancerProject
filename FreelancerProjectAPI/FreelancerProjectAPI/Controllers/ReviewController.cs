using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreelancerProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ReviewController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Review
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.Include(r=>r.User).Include(r=>r.Company).ToListAsync();
        }

        [Authorize]
        [HttpGet]
        [Route("checkifuserreviewedcompany/{companyid}")]
        public Boolean checkIfUserReviewedCompany(long companyid)
        {
            var userid = long.Parse(this.User.Claims.First(i => i.Type == "UserID").Value);
            //long userid = 2;
            User user = _context.Users.Find(userid);
            var company = _context.Companies
                .Include(c => c.Reviews).ThenInclude(r=> r.User)
                .FirstOrDefault(c => c.CompanyID == companyid);
            var testobject = company.Reviews.Any(r => r.User.UserID == userid);
            if (testobject)
            {
                return true;
            } else
            {
                return false;
            }
        }

        // GET: api/Review/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Review>> GetReview(long id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            return review;
        }

        // PUT: api/Review/5
        [HttpPut]
        public async Task<IActionResult> PutReview(Review review)
        {

            if (review != null)
            {
                Review tmpReview = _context.Reviews.Find(review.ReviewID);
                if (tmpReview != null)
                {
                    tmpReview.Title = review.Title;
                    tmpReview.Description = review.Description;

                    _context.Entry(review).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPost]
        [Route("addreviewtocompany")]
        public ActionResult<Review> PostReview(Review review)
        {
            var userid = long.Parse(this.User.Claims.First(i => i.Type == "UserID").Value);
            //long userid = 1;
            User user = _context.Users.Find(userid);
            var company = _context.Companies.FirstOrDefault(c => c.CompanyID == review.Company.CompanyID);
            review.User = user;
            review.Company = company;
            _context.Reviews.Add(review);
            _context.SaveChanges();
            company.Reviews.Add(review);
            _context.SaveChanges();
            return CreatedAtAction("GetReview", new { id = review.ReviewID }, review);
        }

        // DELETE: api/Review/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Review>> DeleteReview(long id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool ReviewExists(long id)
        {
            return _context.Reviews.Any(e => e.ReviewID == id);
        }
    }
}
