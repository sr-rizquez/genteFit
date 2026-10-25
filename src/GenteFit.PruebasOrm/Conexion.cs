namespace GenteFit.PruebasOrm
{
    // Misma cadena que usa GenteFitContext: la variable de entorno GENTEFIT_BD o, si no existe, LocalDB
    public static class Conexion
    {
        public static string Cadena =>
            Environment.GetEnvironmentVariable("GENTEFIT_BD")
            ?? @"Server=(localdb)\MSSQLLocalDB;Database=GenteFitDB;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
