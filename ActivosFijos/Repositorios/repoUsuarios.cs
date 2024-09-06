using ActivosFijos.Clases;
using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using ActivosFijos.Models.Vistas;
using Microsoft.EntityFrameworkCore;

namespace ActivosFijos.Repositorios
{
    public class repoUsuarios
    {
        private readonly aftContext _aftContext;
        private readonly iUtiles _utiles;

        public repoUsuarios(aftContext aftContext, iUtiles utiles)
        {
            _aftContext = aftContext;
            _utiles = utiles;
        }

        public async Task<IDictionary<string, string>> AutenticarUsuarioPassword(mdLogin signInViewModel, string? returnUrl)
        {
            var dictionaryErrors = new Dictionary<string, string>();
            
            if (String.IsNullOrEmpty(signInViewModel.Password))
            {
                _utiles.AgregarClaveValorIDictionary(dictionaryErrors, nameof(mdLogin.Password), "Debe introducir la Contraseña.");
                return dictionaryErrors;
            }
            
            Usuario personal = await ObtenerDatosPersonal(signInViewModel.nombreUsuario);
            if (personal != null)
            {
                string cifra = clCipherUtility.Cifrar2(signInViewModel.Password);
                if (personal.Contrasenna == cifra)
                {
                    if ((await ObtenerUsuariosConPermisos(signInViewModel.nombreUsuario)).Count <= 0)
                        _utiles.AgregarClaveValorIDictionary(dictionaryErrors, nameof(mdLogin.nombreUsuario), $"No tiene permiso para acceder a la aplicación.");
                    
                }
                else
                    _utiles.AgregarClaveValorIDictionary(dictionaryErrors, nameof(mdLogin.Password), "Las credenciales son inválidas.");
            }
            else
                _utiles.AgregarClaveValorIDictionary(dictionaryErrors, nameof(mdLogin.nombreUsuario), "El usuario no existe o no tiene permiso para esta aplicación.");
            return dictionaryErrors;
        }

        public async Task<Usuario> ObtenerDatosPersonal(string nombreUsuario)
        {
            return await _aftContext.Usuarios.Include(us => us.IdPermisos).SingleOrDefaultAsync(us => us.Usuario1.Equals(nombreUsuario));
        }

        public async Task<List<Permiso>> ObtenerUsuariosConPermisos(string usuario)
        {
            Usuario objUsuario = new Usuario();
            objUsuario = await ObtenerDatosPersonal(usuario);
            return objUsuario == null ? new List<Permiso>() : objUsuario.IdPermisos.ToList();
        }
    }
}
