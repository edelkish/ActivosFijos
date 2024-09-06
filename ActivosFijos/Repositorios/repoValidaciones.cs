using ActivosFijos.Interfaces;
using ActivosFijos.Models.Contexts;
using ActivosFijos.Models.EntidadesBD;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ActivosFijos.Repositorios
{
    public class repoValidaciones
    {
        private aftContext _context;
        private readonly iUtiles utiles;

        public repoValidaciones(aftContext context, iUtiles utiles)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            this.utiles = utiles;
        }
        public IDictionary<string, string> EspaciosEnBlanco(string valor, string campo)
        {
            var dictionaryErrors = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(valor))
            {
                utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo no puede estar en blanco. Favor de rectificarlo.");
            }
            else if (valor.IndexOf(" ", StringComparison.Ordinal) > -1)
            {
                if ((valor.IndexOf(" ", StringComparison.Ordinal) == 0) || (valor.IndexOf(" ", valor.Length - 1, StringComparison.Ordinal) == valor.Length - 1))
                    utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo no puede empezar ni terminar con el espacio en blanco. Favor de rectificarlo.");
                else
                {
                    int posicion = valor.IndexOf(" ", StringComparison.Ordinal);

                    for (int i = posicion + 1; i < valor.Length; i++)
                    {
                        if (valor[i] == ' ')
                        {
                            utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo no puede tener espacios continuos en blanco. Favor de rectificarlo.");

                            break;
                        }

                        i = valor.IndexOf(" ", i + 1, StringComparison.Ordinal);

                        if (i == -1)
                            break;
                    }
                }
            }

            return dictionaryErrors;
        }
        public IDictionary<string, string> ContrasennaEnBlanco(string valor, string campo)
        {
            var dictionaryErrors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(valor))
            {
                utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo no puede estar en blanco. Favor de rectificarlo.");
            }

            return dictionaryErrors;
        }
        public IDictionary<string, string> VerificarSeguridadContrasenna(string valor, string campo)
        {
            var dictionaryErrors = new Dictionary<string, string>();
            bool isValida = Regex.IsMatch(valor, "^(?=.{8,})(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*\\W).*$");
            if (!isValida)
            {
                utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "Su contraseña no cumple con las restricciones de seguridad (Debe contener al menos una Mayúscula, Minúscula, Número ó Caracter especial y un mínimo de 8 caracteres). Favor de rectificarlo.");
            }
            return dictionaryErrors;
        }

        public IDictionary<string, string> VerificarCampoFila(long? fila, string campo)
        {
            var dictionaryErrors = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(fila.ToString()))
            {
                utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo Fila no puede estar en blanco. Favor de rectificarlo.");
            } 
            else if (fila <= 0)
            {
                utiles.AgregarClaveValorIDictionary(dictionaryErrors, campo, "El campo Fila es incorrecto. Favor de rectificarlo.");
            }
            return dictionaryErrors;
        }        
    }
}
