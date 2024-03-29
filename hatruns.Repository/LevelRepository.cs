﻿using HatCommunityWebsite.DB;
using Microsoft.EntityFrameworkCore;

namespace HatCommunityWebsite.Repo
{
    public interface ILevelRepository
    {
        Task<List<Level>> GetLevelsByGameIdIncludeAll(int gameId);

        Task<Level> GetLevelById(int id);

        Task<Level> GetLevelByIdIncludeCategories(int id);

        Task SaveLevel(Level level);

        Task UpdateLevels(List<Level> levels);

        Task SaveLevels(List<Level> levels);

        Task UpdateLevel(Level level);

        Task<bool> LevelExistsByName(string name);

        Task<bool> LevelExistsByNameExluceId(string name, int id);
    }

    public class LevelRepository : ILevelRepository
    {
        private readonly AppDbContext _context;

        public LevelRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Level> GetLevelById(int id)
        {
            return await _context.Levels.FindAsync(id);
        }

        public async Task<Level> GetLevelByIdIncludeCategories(int id)
        {
            return await _context.Levels
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Level>> GetLevelsByGameIdIncludeAll(int gameId)
        {
            return await _context.Levels
                .Include(x => x.Categories)
                .ThenInclude(x => x.Subcategories)
                .Where(x => x.GameId == gameId)
                .ToListAsync();
        }

        public async Task<bool> LevelExistsByName(string name)
        {
            return await _context.Levels.AnyAsync(x => x.Name == name);
        }

        public async Task<bool> LevelExistsByNameExluceId(string name, int id)
        {
            return await _context.Levels.AnyAsync(x => x.Name == name && x.Id != id);
        }

        public async Task SaveLevel(Level level)
        {
            _context.Levels.Add(level);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLevel(Level level)
        {
            _context.Levels.Update(level);
            await _context.SaveChangesAsync();
        }

        public async Task SaveLevels(List<Level> levels)
        {
            foreach (var level in levels)
                _context.Levels.Add(level);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateLevels(List<Level> levels)
        {
            foreach (var level in levels)
                _context.Levels.Update(level);

            await _context.SaveChangesAsync();
        }
    }
}