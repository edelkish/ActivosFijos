namespace ActivosFijos.Interfaces
{
    public interface iUtiles
    {
        /// <summary>
        /// Elimina todos los caracteres / hasta que se encuentre uno diferente comenzando por la derecha.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        string EliminarSlashsHaciaIzquierda(string valor);
        /// <summary>
        /// Elimina todos los caracteres / hasta que se encuentre uno diferente comenzando por la izquierda.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        string EliminarSlashDesdeIzquierda(string valor);
        /// <summary>
        /// Agrega un caracter / al final de la cadena.
        /// </summary>
        /// <param name="valor">Cadena a evaluar</param>
        /// <returns></returns>
        string AgregarSlashFinalDerecha(string valor);
        /// <summary>
        /// Agrega una clave a un objeto de tipo Diccionario genérico donde T representan las claves y K los valores. (Si existe una clave con el mismo valor, se reemplazará su valor por el nuevo).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="iDictionary">Diccionario genérico</param>
        /// <param name="clave">Clave a adicionar</param>
        /// <param name="valor">Valor para la clave</param>
        void AgregarClaveValorIDictionary<T, K>(IDictionary<T, K> iDictionary, T clave, K valor);
    }
}
