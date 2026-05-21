using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RetailShop.Database
{
    public static class DB
    {
        public static string ConnectionString =
            "Server=venlks\\VENLKS;Database=RetailShop;Integrated Security=True;";

        public static SqlConnection GetConn() => new SqlConnection(ConnectionString);

        public static bool TestConnection()
        {
            try { using (var c = GetConn()) { c.Open(); return true; } }
            catch { return false; }
        }

        public static DataTable Query(string sql, params SqlParameter[] p)
        {
            var dt = new DataTable();
            try
            {
                using (var c = GetConn()) { c.Open();
                    using (var cmd = new SqlCommand(sql, c)) { cmd.Parameters.AddRange(p);
                        new SqlDataAdapter(cmd).Fill(dt); } }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка БД:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return dt;
        }

        public static int Exec(string sql, params SqlParameter[] p)
        {
            try
            {
                using (var c = GetConn()) { c.Open();
                    using (var cmd = new SqlCommand(sql, c)) { cmd.Parameters.AddRange(p); return cmd.ExecuteNonQuery(); } }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка БД:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return -1; }
        }

        public static object Scalar(string sql, params SqlParameter[] p)
        {
            try
            {
                using (var c = GetConn()) { c.Open();
                    using (var cmd = new SqlCommand(sql, c)) { cmd.Parameters.AddRange(p); return cmd.ExecuteScalar(); } }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка БД:\n" + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); return null; }
        }

        public static SqlParameter P(string name, object val) =>
            new SqlParameter(name, val ?? DBNull.Value);
    }
}