using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatRESTfullAPI.Data;
using ChatRESTfullAPI.Models;

namespace ChatRESTfullAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly ChatContext _context;

        public ChatsController(ChatContext context)
        {
            _context = context;
        }

        // GET: api/Chats
        [HttpGet]
        public IEnumerable<Chat> GetChats()
        {
            return _context.Chats;
        }

        // GET: api/Chats/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChat([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var chat = await _context.Chats.FindAsync(id);

            if (chat == null)
            {
                return NotFound();
            }

            return Ok(chat);
        }

        // PUT: api/Chats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat([FromRoute] int id, [FromBody] Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != chat.ChatId)
            {
                return BadRequest();
            }

            _context.Entry(chat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatExists(id))
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

        // POST: api/Chats
        [HttpPost]
        public async Task<IActionResult> PostChat([FromBody] Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetChat", new { id = chat.ChatId }, chat);
        }

        // DELETE: api/Chats/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
            {
                return NotFound();
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return Ok(chat);
        }

        private bool ChatExists(int id)
        {
            return _context.Chats.Any(e => e.ChatId == id);
        }
    }
}