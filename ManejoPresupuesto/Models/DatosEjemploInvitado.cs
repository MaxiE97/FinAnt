namespace ManejoPresupuesto.Models
{
    public static class DatosEjemploInvitado
    {
        public static List<(DateTime FechaTransaccion, decimal Monto, string CuentaNombre, string CategoriaNombre)> Transacciones = new List<(DateTime, decimal, string, string)>
    {
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), 300000, "Banco Nación", "Pago salario"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 3), -25000, "Banco Nación", "Gastos servicios"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 7), -35000, "Banco Nación", "Otros gastos"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10), -30000, "Banco Nación", "Gastos servicios"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 20), -10000, "Banco Nación", "Otros gastos"),

        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10), -13500, "Efectivo", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15), -3500, "Efectivo", "Otros gastos"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 23), -15500, "Efectivo", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 16), -5000, "Efectivo", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 27), -3500, "Efectivo", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 4), 60000, "Efectivo", "Dinero transferido"),


        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5), -17000, "Mercado Pago", "Cuota club"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5), 50000, "Mercado Pago", "Pago beca"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 17), -6000, "Mercado Pago", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15), -3500, "Mercado Pago", "Gastos comida"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 2), 10500, "Mercado Pago", "Venta realizada"),
        (new DateTime(DateTime.Now.Year, DateTime.Now.Month, 26), 13500, "Mercado Pago", "Venta realizada"),

        // Agrega más datos de ejemplo aquí...
    };
    }
}

