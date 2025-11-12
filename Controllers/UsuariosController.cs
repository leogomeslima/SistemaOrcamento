using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaOrcamento.Api.DTOs;
using SistemaOrcamento.Api.Models;

namespace SistemaOrcamento.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        
        private readonly SistemaOrcamentoContext _context;

       
        public UsuariosController(SistemaOrcamentoContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUsuario(UsuarioCreateDto usuarioDto)
        {
           
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioDto.Email))
            {
                return BadRequest("Este email já está cadastrado.");
            }

            
            if (usuarioDto.Role != "Colaborador" && usuarioDto.Role != "Gestor" && usuarioDto.Role != "Financeiro")
            {
                return BadRequest("O Role deve ser 'Colaborador', 'Gestor' ou 'Financeiro'.");
            }

            
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Senha);

           
            var novoUsuario = new Usuario
            {
                Nome = usuarioDto.Nome,
                Email = usuarioDto.Email,
                SenhaHash = senhaHash, 
                Role = usuarioDto.Role
            };

          
            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            var usuarioParaRetorno = new UsuarioReadDto
            {
                UsuarioID = novoUsuario.UsuarioID,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email,
                Role = novoUsuario.Role
            };
            
            return CreatedAtAction(nameof(GetUsuarioById), new { id = novoUsuario.UsuarioID }, novoUsuario);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioReadDto>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioReadDto
                {
                    UsuarioID = u.UsuarioID,
                    Nome = u.Nome,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
            return Ok(usuarios);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioReadDto>> GetUsuarioById(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(); 
            }

            var usuarioParaRetorno = new UsuarioReadDto
            {
                UsuarioID = usuario.UsuarioID,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Role = usuario.Role
            };

            return Ok(usuarioParaRetorno); // Retorna 200 OK com o DTO
        }
    }
}