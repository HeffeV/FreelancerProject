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
                User = user,
                Accepted = true,
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
            List<UserCompany> userCompanies = new List<UserCompany>();
            userCompanies = _context.UserCompanies.Include(uc => uc.Company)
                                                  .Where(uc => uc.Company.CompanyID == id).ToList();

            if (company == null)
            {
                return NotFound();
            }

            foreach(UserCompany userCompany in userCompanies)
            {
                _context.UserCompanies.Remove(userCompany);
            }

            _context.Companies.Remove(company);
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
            var usercompanies = _context.UserCompanies.Include(u => u.Company).Where(u => u.User.UserID == userID && u.Accepted == true) ;
            List<Company> companies = new List<Company>();
            foreach (var uc in usercompanies)
            {
                var found = _context.Companies.FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID);
                if (found != null)
                {
                    companies.Add(_context.Companies.Include(c => c.Assignments).ThenInclude(a => a.Status).Include(c => c.Assignments).ThenInclude(a => a.UserAssignments).ThenInclude(u => u.User).FirstOrDefault(c => c.CompanyID == uc.Company.CompanyID));
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
			var userCompanies = _context.UserCompanies.Include(uc => uc.User).Include(uc => uc.Company).Where(uc => uc.User.UserID == userID && uc.Accepted == true);
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

		[Authorize]
		//POST: api/Company/InviteRecruiter?companyID=0&recruiterEmail=0
		[HttpPost("InviteRecruiter")]
		//invite a recuiter to a company
		//return OK if succesful
		public async Task<IActionResult> InviteRecruiterToCompany(int companyID, string recruiterEmail)
		{

			User user = _context.Users.FirstOrDefault(u => u.Email == recruiterEmail);
			if (user == null)
			{
				return NotFound();
			}
			else
			{
				User recruiter = _context.Users.Include(u => u.UserType).FirstOrDefault(u => u.Email == recruiterEmail);
				Company company = _context.Companies.Include(c => c.UserCompanies).FirstOrDefault(c => c.CompanyID == companyID);

				if (recruiter.UserType.Type == "recruiter")
				{
					UserCompany userCompany = new UserCompany(); 
					userCompany.User = recruiter;
					userCompany.Company = company;
					userCompany.Accepted = false;

					_context.UserCompanies.Add(userCompany);
					await _context.SaveChangesAsync();
					return Ok();
				}
				else
				{
					return BadRequest();
				}
			}
		}

		[Authorize]
		//PUT: api/Company/AcceptInviteCompany?companyID=0&recruiterID=0
		[HttpPut("AcceptInviteCompany")]
		//recruiter accepts the invite to the company
		//return OK if succesful
		public async Task<IActionResult> AcceptInviteToCompany(int companyID, int recruiterID)
		{
			UserCompany userCompany = _context.UserCompanies.Include(uc => uc.Company).Include(uc => uc.User).FirstOrDefault(uc => uc.Company.CompanyID == companyID && uc.User.UserID == recruiterID);

			userCompany.Accepted = true;

			_context.Entry(userCompany).State = EntityState.Modified;
			await _context.SaveChangesAsync();
			return Ok();
		}
		[Authorize]
		//PUT: api/Company/DeclineInviteCompany?companyID=0&recruiterID=0
		[HttpDelete("DeclineInviteCompany")]
		//recruiter declines the invite to the company
		//return OK if succesful
		public async Task<IActionResult> DeclineInviteToCompany(int companyID, int recruiterID)
		{
			UserCompany userCompany = _context.UserCompanies.Include(uc => uc.Company).Include(uc => uc.User).FirstOrDefault(uc => uc.Company.CompanyID == companyID && uc.User.UserID == recruiterID);
			_context.UserCompanies.Remove(userCompany);
			await _context.SaveChangesAsync();
			return Ok();
		}

		[Authorize]
		//DELETE:  api/Company/LeaveCompany?companyID=0&recruiterID=0
		[HttpDelete("LeaveCompany")]
		//recruiter leaves company
		//if he is the last recruiter of the company - delete the company
		//return OK if succesful
		public async Task<IActionResult> LeaveCompany(int companyID, int recruiterID)
		{

			List<UserCompany> userCompanies = _context.UserCompanies.Include(uc => uc.Company).Include(uc => uc.User).ThenInclude(u => u.UserType).Where(uc => uc.Company.CompanyID == companyID && uc.User.UserType.Type == "recruiter").ToList();

			if (userCompanies.Count == 1)
			{
				Company company = _context.Companies.Include(c => c.UserCompanies).FirstOrDefault(c => c.CompanyID == companyID);
				DeleteCompany(company.CompanyID);
			}
			else
			{
				UserCompany userCompany = _context.UserCompanies.Include(uc => uc.Company).Include(uc => uc.User).FirstOrDefault(uc => uc.Company.CompanyID == companyID && uc.User.UserID == recruiterID);
				_context.UserCompanies.Remove(userCompany);
			}

			await _context.SaveChangesAsync();
			return Ok();
		}

		[Authorize]
		//GET:  api/Company/GetCompanyInvites?userID=0
		[HttpGet("GetCompanyInvites")]
		//returns list of invites from companies tot this user
		public async Task<ActionResult<IEnumerable<UserCompany>>> GetCompanyInvites(int userID)
		{
			List<UserCompany> invites = await _context.UserCompanies.Include(uc => uc.Company).Include(uc => uc.User).Where(uc => uc.User.UserID == userID && uc.Accepted == false).ToListAsync();
			return invites;
		}

	}

}
