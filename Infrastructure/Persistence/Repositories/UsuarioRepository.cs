using Domain.Entities;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }


        public async Task<Usuario> GetByIdAsync(Guid id)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<Usuario> GetByEmail(string email)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == email);
        }

        public async Task<Usuario> GetByTokenRecuperacion(string token)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == token);
        }
    }
}
