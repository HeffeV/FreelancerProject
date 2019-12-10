using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;

namespace FreelancerProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CompanyController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Company
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await _context.Companies.ToListAsync();
        }

        // GET: api/Company/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(long id)
        {
            var company = await _context.Companies
                .Include(c=> c.Assignments)
                .Include(c=> c.ContactInfo)
                .Include(c=> c.Location)
                .Include(c=> c.Reviews)
                .Include(c=> c.UserCompanies).ThenInclude(uc=> uc.User)
                .FirstOrDefaultAsync(c=>c.CompanyID == id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        // PUT: api/Company/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(long id, Company company)
        {
            if (id != company.CompanyID)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Company
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            var userid = long.Parse(this.User.Claims.First(i => i.Type == "UserID").Value);
            User user = _context.Users.Find(userid);

            UserCompany com = new UserCompany()
            {
                Company = company,
                User = user
            };

            company.UserCompanies = new List<UserCompany>();
            company.UserCompanies.Add(com);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCompany", new { id = company.CompanyID }, company);
        }

        // DELETE: api/Company/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(long id)
        {
            var company = await _context.Companies
               .Include(c => c.Assignments)
                .Include(c => c.ContactInfo)
                .Include(c => c.Location)
                .Include(c => c.Reviews)
                .Include(c => c.UserCompanies).ThenInclude(uc => uc.User)
                .FirstOrDefaultAsync(c => c.CompanyID == id);

            if (company == null)
            {
                return NotFound();
            }

            _context.Companies
                .Remove(company);
            await _context.SaveChangesAsync();

            return company;
        }

        private bool CompanyExists(long id)
        {
            return _context.Companies.Any(e => e.CompanyID == id);
        }

		[HttpGet("ByUser")]
		public ActionResult<IEnumerable<Company>> FindCompanyByUser(int userID)
		{
			var usercompanies = _context.UserCompanies.Include(u => u.Company).Where(u => u.User.UserID == userID);
			List<Company> companies = new List<Company>();
			foreach (var uc in usercompanies)
			{
				var found = _context.Companies.FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID);
				if (found != null)
				{
					companies.Add(_context.Companies.FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
				}
			}
			return companies;

		}

        [HttpGet]
        [Route("getRandoms")]
        public async Task<ActionResult<IEnumerable<Company>>> GetRandomCompanies()
        {
            var allCompanies = await _context.Companies.ToListAsync();
            List<Company> companies = new List<Company>();
            Random r = new Random();
            if (allCompanies.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    companies.Add(allCompanies[r.Next(0, allCompanies.Count)]);
                }
            }
            else
            {
                NotFound();
            }

            return companies;
        }
    }
}
