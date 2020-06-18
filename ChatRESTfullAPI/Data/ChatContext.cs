using ChatRESTfullAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatRESTfullAPI.Data
{
    public class ChatContext : DbContext
    {
        public ChatContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
           // base.OnModelCreating(builder);
            builder.Entity<ChatUser>()
                .HasKey(cu =>  new { cu.UserId, cu.ChatId });
            //builder.Entity<ChatUser>()
            //    .HasOne(cu => cu.User)
            //    .WithMany(u => u.ChatUsers)
            //    .HasForeignKey(cu => cu.UserId);
            //builder.Entity<ChatUser>()
            //    .HasOne(cu => cu.Chat)
            //    .WithMany(c => c.ChatUsers)
            //    .HasForeignKey(cu => cu.ChatId);
           
        }
        public DbSet<Chat>Chats { get; set; }
        public DbSet<Message>Messages { get; set; }
        public DbSet<User>Users { get; set; }
        public DbSet<ChatUser>ChatsUsers { get; set; }
    }
}
