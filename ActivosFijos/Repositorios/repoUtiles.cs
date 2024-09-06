using ActivosFijos.Interfaces;

namespace ActivosFijos.Repositorios
{
    public class repoUtiles: iUtiles
    {
        /// <summary>
        /// Elimina todos los caracteres / hasta que se encuentre uno diferente comenzando por la derecha.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        public string EliminarSlashsHaciaIzquierda(string valor)
        {
            for (int i = valor.Length - 1; i >= 0; i--)
            {
                if (valor[i] != '/')
                    return valor;
                else
                {
                    valor = valor.Remove(i);
                    i--;
                }
            }
            return valor;
        }

        /// <summary>
        /// Elimina todos los caracteres / hasta que se encuentre uno diferente comenzando por la izquierda.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        public string EliminarSlashDesdeIzquierda(string valor)
        {
            for (int i = 0; i < valor.Length; i++)
            {
                if (valor[i] == '/')
                {
                    valor = valor.Remove(i);
                    i--;
                }
                else
                    break;
            }
            return valor;
        }
        /// <summary>
        /// Agrega un caracter / al final de la cadena.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        public string AgregarSlashFinalDerecha(string valor)
        {
            if (valor[valor.Length - 1] != '/')
                valor += "/";
            return valor;
        }
        /// <summary>
        /// Agrega una clave a un objeto de tipo Diccionario genérico donde T representan las claves y K los valores. (Si existe una clave con el mismo valor, se reemplazará su valor por el nuevo).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="iDictionary">Diccionario genérico</param>
        /// <param name="clave">Clave a adicionar</param>
        /// <param name="valor">Valor para la clave</param>
        public void AgregarClaveValorIDictionary<T, K>(IDictionary<T, K> iDictionary, T clave, K valor)
        {
            if (iDictionary.ContainsKey(clave))
                iDictionary.Keys.Remove(clave);
            iDictionary.Add(clave, valor);
        }
    }
}
