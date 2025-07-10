// Repositories/MessageRepository.cs
using AvitoClone.Models;
using AvitoClone.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AvitoClone.Repositories
{
    public interface IMessageRepository
    {
        Task<List<Message>> GetMessagesForAdAsync(int adId);
        Task AddMessageAsync(Message message);
    }

    public class MessageRepository : IMessageRepository
    {
        private readonly AppDbContext _context;

        public MessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> GetMessagesForAdAsync(int adId)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Where(m => m.AdId == adId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}