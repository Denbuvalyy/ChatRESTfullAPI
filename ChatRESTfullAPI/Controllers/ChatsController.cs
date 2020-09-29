using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatRESTfullAPI.Data;
using ChatRESTfullAPI.Models;
using Microsoft.AspNetCore.Cors;

namespace ChatRESTfullAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("HealthPolicy")]
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


        // GET: api/Chats/5/messages
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetChatMessages([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var chat = await _context.Chats.Include(m => m.ChatMessages)
                .FirstOrDefaultAsync(i => i.ChatId == id);

            if (chat == null)
            {
                return NotFound();
            }

            return Ok(chat.ChatMessages);
        }

        // GET: api/Chats/5/users
        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetChatUsers([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userWithChats = await _context.Users.Include(u=>u.UserChats ).Include(m=>m.UserMessages).ToListAsync();
             
            if (userWithChats == null)
            {
                return NotFound();
            }
            List<User> users = new List<User>();
            foreach(var user in userWithChats)
            {
                var chat = user.UserChats.FirstOrDefault(h => h.ChatId == id);
                if (chat != null)
                {
                    user.UserChats = null;
                    users.Add(user);
                }
            }

            return Ok(users.OrderBy(n=>n.UserName));
        }

        // PUT: api/Chats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChat([FromRoute] int id, [FromBody] Message message)//[FromBody] Chat chat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _context.Users.FindAsync(message.UserId);
            var chat = await _context.Chats.Include(m => m.ChatMessages).Include(p => p.ChatUsers)
                .FirstOrDefaultAsync(i => i.ChatId == id);           

            if (chat == null)
            {
                return NotFound();
            }

            if (user == null)
            {
                return NotFound();
            }

            if (!chat.ChatUsers.Where(n => n.UserId == message.UserId).Any())
            {
                ChatUser chatUser = new ChatUser();               
                chatUser.Chat = chat;
                chatUser.User = user;
                _context.ChatsUsers.Add(chatUser);
                await _context.SaveChangesAsync();
            }
            chat.ChatMessages.Add(message);

            //if (id != chat.ChatId)
            //{
            //    return BadRequest();
            //}

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
  
            List<ChatUser> tempUsers = chat.ChatUsers.ToList();
            chat.ChatUsers = null;

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            //var tempchat= await _context.Chats.Include(m => m.ChatMessages).Include(p => p.ChatUsers)
            //    .FirstOrDefaultAsync(i => i.ChatId == chat.ChatId);
            //tempchat.ChatUsers = tempUsers;

            //try
            //{


            for (int i = 0; i < tempUsers.Count; i++)
            {
                ChatUser chatUser = new ChatUser();
                chatUser.Chat = chat;
                //chatUser.ChatId = chat.ChatId;
                var user = _context.Users.Find(tempUsers[i].UserId);
                chatUser.User = user;
                // chatUser.UserId = tempUsers[i].UserId;
                _context.ChatsUsers.Add(chatUser);
                _context.SaveChanges();
            }

            _context.Entry(chat).State = EntityState.Modified;
            _context.SaveChanges();
            //try
            //{
                
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!ChatExists(chat.ChatId))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //}
            //catch(Exception ex)
            //{

            //}
            //_context.Entry(chat).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!ChatExists(chat.ChatId))
            //    {
            //        return NotFound();
            //    }

            //    else
            //    {/        throw;
            //    }
            //}
            //var tempchat = _context.Chats.Find(chat.ChatId);
            chat.ChatUsers = null;

            return Ok(chat);  
            //return CreatedAtAction("GetChat", new { id = chat.ChatId }, chat);
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