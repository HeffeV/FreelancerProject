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
    public class CompanyController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public CompanyController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Company
        [Authorize]
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
                .Include(c => c.Assignments).ThenInclude(a => a.TagAssignments).ThenInclude(a => a.Tag)
                .Include(c => c.ContactInfo)
                .Include(c => c.Location)
                .Include(c => c.Reviews).ThenInclude(c=>c.User)
                .Include(c => c.UserCompanies).ThenInclude(uc => uc.User)
                .Include(c => c.TagCompanies).ThenInclude(tc => tc.Tag)
                .FirstOrDefaultAsync(c => c.CompanyID == id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        [Authorize]
        [HttpPut("updateimage")]
        public async Task<IActionResult> PutImage(Company company)
        {

            _context.Entry(company).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // PUT: api/Company/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutCompany(Company company)
        {

            Company tmpCompany;

            tmpCompany = await _context.Companies
                .Include(c => c.Assignments)
                .Include(c => c.ContactInfo)
                .Include(c => c.Location)
                .Include(c => c.Reviews)
                .Include(c => c.UserCompanies).ThenInclude(uc => uc.User)
                .Include(c => c.TagCompanies).ThenInclude(tc => tc.Tag).FirstOrDefaultAsync(c => c.CompanyID == company.CompanyID);
            tmpCompany.CompanyName = company.CompanyName;
            tmpCompany.Location.Country = company.Location.Country;
            tmpCompany.Location.Address = company.Location.Address;
            tmpCompany.Location.Postcode = company.Location.Postcode;
            tmpCompany.About = company.About;
            tmpCompany.ContactInfo.LinkedIn = company.ContactInfo.LinkedIn;
            tmpCompany.ContactInfo.MobileNumber = company.ContactInfo.MobileNumber;


            foreach (TagCompany tc in company.TagCompanies)
            {
                TagCompany tmpTagCompany = _context.TagCompanies.Include(t => t.Tag).SingleOrDefault(t => t.Tag.TagName == tc.Tag.TagName && t.TagCompanyID == tc.TagCompanyID);
                if (tmpTagCompany == null || tmpTagCompany.Equals(null))
                {
                    //tag is nog niet toegevoegd aan assignment
                    Tag tmpTag = _context.Tags.SingleOrDefault(t => t.TagName == tc.Tag.TagName);
                    if (tmpTag == null || tmpTag.Equals(null))
                    {
                        //tag bestaat niet
                        tmpCompany.TagCompanies.Add(new TagCompany() { Tag = new Tag() { TagName = tc.Tag.TagName }, Company = tmpCompany });
                    }
                    else
                    {
                        //tag bestaat niet
                        tmpCompany.TagCompanies.Add(new TagCompany() { Tag = tmpTag, Company = tmpCompany });
                    }
                }
            }

            _context.Entry(tmpCompany).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();

        }

        // POST: api/Company
        [Authorize]
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
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Company>> DeleteCompany(long id)
        {
            var company = await _context.Companies
                .Include(c => c.Assignments).ThenInclude(a=>a.UserAssignments)
                .Include(c => c.Assignments).ThenInclude(a => a.TagAssignments)
                .Include(c => c.ContactInfo)
                .Include(c => c.Location)
                .Include(c => c.Reviews)
                .Include(c => c.UserCompanies)
                .Include(c=>c.TagCompanies)
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

        [Authorize]
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
                    companies.Add(_context.Companies.Include(c => c.Assignments).ThenInclude(a => a.Status).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
                }
            }
            return companies;

        }


        [HttpGet]
        [Route("getRandoms")]
        public async Task<ActionResult<IEnumerable<Company>>> GetRandomCompanies()
        {
            var allCompanies = await _context.Companies.Include(a => a.TagCompanies).ThenInclude(a => a.Tag).ToListAsync();
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

		[HttpGet("CheckIfOwnCompany")]
		public Boolean CheckIfOwnCompany(int companyID, int userID)
		{
			var userCompanies = _context.UserCompanies.Include(uc => uc.User).Include(uc => uc.Company).Where(uc => uc.User.UserID == userID);
			List<Company> companies = new List<Company>();
			List<Assignment> assignments = new List<Assignment>();

			foreach (var uc in userCompanies)
			{
				companies.Add(_context.Companies.Include(c => c.Assignments).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
			}
			foreach (var company in companies)
			{
				
					if (company.CompanyID == companyID)
					{
						return true;
					}
				
			}
			return false;

		}

        [Authorize]
        [HttpPost("filteredCompanies")]
        public async Task<ActionResult<IEnumerable<Company>>> GetFilteredCompanies(FilterCompanyModel filterCompanyModel)
        {
            List<Company> companies = await _context.Companies
                .Include(c => c.ContactInfo)
                .Include(c => c.Location)
                .Include(c => c.UserCompanies).ThenInclude(uc => uc.User)
                .Include(c => c.TagCompanies).ThenInclude(tc => tc.Tag)
                .ToListAsync();

            if (filterCompanyModel.CompanyName != null && filterCompanyModel.CompanyName != "" && filterCompanyModel.CompanyName != " ")
            {
                companies = companies.Where(c => c.CompanyName.ToLower().Contains(filterCompanyModel.CompanyName.ToLower())).ToList();
            }
            if (filterCompanyModel.Country != null && filterCompanyModel.Country != "" && filterCompanyModel.Country != " ")
            {
                companies = companies.Where(c => c.Location.Country.ToLower().Contains(filterCompanyModel.Country.ToLower())).ToList();
            }
            if (filterCompanyModel.Postcode != null && filterCompanyModel.Postcode != "")
            {
                companies = companies.Where(c => c.Location.Postcode.ToLower().Contains(filterCompanyModel.Postcode.ToLower())).ToList();
            }

            return companies;
        }
    }
}
