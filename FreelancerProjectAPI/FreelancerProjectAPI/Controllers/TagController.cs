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
    public class TagController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public TagController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Tag
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            List<Tag> tags=  await _context.Tags.ToListAsync();
            tags = tags.OrderBy(t => t.TagName).ToList();
            return tags;
        }

        // GET: api/Tag/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTag(long id)
        {
            var tag = await _context.Tags.FindAsync(id);

            if (tag == null)
            {
                return NotFound();
            }

            return tag;
        }

        // PUT: api/Tag/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutTag(Tag tag)
        {
            Tag tmpTag = _context.Tags.FirstOrDefault(t => t.TagID == tag.TagID);
            tmpTag.TagName = tag.TagName;
            _context.Entry(tmpTag).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/Tag
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Tag>> PostTag(Tag tag)
        {
            if (_context.Tags.FirstOrDefault(t => t.TagName == tag.TagName) == null)
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetTag", new { id = tag.TagID }, tag);
            }
            else
            {
                return NotFound();
            }

        }

        // DELETE: api/Tag/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tag>> DeleteTag(long id)
        {
            var assignmenttags = await _context.TagAssignments.Where(t => t.Tag.TagID == id).ToListAsync();
            if (assignmenttags != null)
            {
                foreach (TagAssignment ta in assignmenttags)
                {
                    _context.TagAssignments.Remove(ta);
                }
            }
            var companytags = await _context.TagCompanies.Where(t => t.Tag.TagID == id).ToListAsync();
            if (companytags != null)
            {
                foreach (TagCompany ta in companytags)
                {
                    _context.TagCompanies.Remove(ta);
                }
            }
            var usertags = await _context.TagUsers.Where(t => t.Tag.TagID == id).ToListAsync();
            if (usertags != null)
            {
                foreach (TagUser ta in usertags)
                {
                    _context.TagUsers.Remove(ta);
                }
            }

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return tag;
        }

        private bool TagExists(long id)
        {
            return _context.Tags.Any(e => e.TagID == id);
        }

        [Authorize]
        [HttpDelete("tagAssignment/{id}")]
		public async Task<ActionResult<TagAssignment>> DeleteTagAssignment(long id)
		{
			var tagAssignment = await _context.TagAssignments.FindAsync(id);
			if (tagAssignment == null)
			{
				return NotFound();
			}

			_context.TagAssignments.Remove(tagAssignment);
			await _context.SaveChangesAsync();

			return tagAssignment;
		}

        [Authorize]
        [HttpDelete("tagUser/{id}")]
        public async Task<ActionResult<TagUser>> DeleteTagUser(long id)
        {
            var tagUser = await _context.TagUsers.FindAsync(id);
            if (tagUser == null)
            {
                return NotFound();
            }

            _context.TagUsers.Remove(tagUser);
            await _context.SaveChangesAsync();

            return tagUser;
        }

        [Authorize]
        [HttpPost("tagAssignment")]
		public async Task<ActionResult<TagAssignment>> PostTagAssignment(TagAssignment ta)
		{
			_context.TagAssignments.Add(ta);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetTag", new { id = ta.TagAssignmentID }, ta);
		}
	}
}
